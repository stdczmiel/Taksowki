using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Taksowki
{
    public struct GPS
    {
        double wys;
        double szer;

        public GPS(double w, double s) { wys = w; szer = s; }

        // odleglosc w kilometrach (przy zalozeniu 1 stopien = 111 km )
        public static double Odleglosc(GPS l, GPS r)
        {
            return (Math.Abs(l.wys - r.wys) + Math.Abs(l.szer - r.szer)) * 111.1;
        }

        // czas przejazdu w minutach (przy zalozeniu predkosci 50km/h)
        public static double CzasPrzejazdu(GPS l, GPS r)
        {
            return Odleglosc(l, r) * 60.0 / 50.0;
        }
    }

    public class TabuSearch
    {
        const int dlugoscKolejkiTabu = 1;
        const double maxOpoznienie = 10;    // maksymalne Opoznienie taksowki w minutach
        const double maxRoznica = 120;       // maksymalna roznica miedzy kierowca najdluzej i najkrocej pracujacym w minutach


        public ObservableCollection<Kierowca> kierowcy;    // tu jest zapisany harmonogram
        Queue<Realizacja> kolejkaTabu;
        public double funkcjaCelu;

        public TabuSearch()
        {
            InitDebug();
        }

        public void Init()
        {
            BazaDanych bazaDanych = new BazaDanych();

            // ******* Pobranie tabel z bazy danych **********
            var enumerableKierowcy = from kier in bazaDanych.Kierowcy
                                     select kier;      // moze lepiej użyć array i ToArray()
            kierowcy = new ObservableCollection<Kierowca>(enumerableKierowcy);
            foreach (Kierowca k in kierowcy)
            {
                var enumerableLista = (from real in bazaDanych.Realizacje
                                       where real.Kierowca == k     // albo real.Kierowca.Id == k.Id
                                       orderby real.Godzina
                                       select real).ToList();
                k.ListaZlecen = new ObservableCollection<Realizacja>(enumerableLista);
            }

            funkcjaCelu = FunkcjaCelu();

            kolejkaTabu = new Queue<Realizacja>();
        }

        //DEBUG
        public void InitDebug()
        {
            kierowcy = new ObservableCollection<Kierowca>();
            kierowcy.Add(new Kierowca());
            kierowcy.Add(new Kierowca());
            kierowcy.Add(new Kierowca());

            int id = 0;
            foreach (Kierowca k in kierowcy)
            {
                k.CzasPracy = 0;
                k.ListaZlecen = new ObservableCollection<Realizacja>();
                k.Id = id;
                id++;
            }

            kolejkaTabu = new Queue<Realizacja>();
        }
        //DEBUG

        public void DodajZlecenie(Zlecenie z)
        {

            // *********** Wstawiamy nowe zlecenie na pierwsza taksowke        *********

            Kierowca najmniejZajety = kierowcy.First();
            foreach (Kierowca k in kierowcy)
            {
                if (k.CzasPracy < najmniejZajety.CzasPracy)
                    najmniejZajety = k;
            }

            // przechodzimy do godziny, od której mozna wstawic nasze zadanie
            int indeks = 0;
            while ((indeks < najmniejZajety.ListaZlecen.Count()) && (najmniejZajety.ListaZlecen[indeks].Godzina < z.zadanaGodzina))
            {
                indeks++;
            }

            // wstawiamy zadanie
            z.Id = Zlecenie.iloscZlecen;
            Zlecenie.iloscZlecen++;
            Realizacja r = new Realizacja();
            r.Zlecenie = z;
            r.Kierowca = najmniejZajety;
            r.CzasPrzejazdu = GPS.CzasPrzejazdu(z.skad, z.dokad);
            najmniejZajety.ListaZlecen.Insert(indeks, r);
            PrzeliczCzasy(najmniejZajety);
            funkcjaCelu = FunkcjaCelu();

            // mozna by jeszcze tutaj sprawdzic ograniczenia

            Algorytm();
        }

        // 1. Szuka zadania o najdluzszym czasie dojazdu.
        // 2. Wstawia je na kazde mozliwe miejsce i jesli jest lepsza funkcja celu, to je tam zostawia.
        // 3. Zapisuje obecne zadanie na kolecje Tabu i wraca do kroku 1.
        void Algorytm()
        {
            Kierowca kierNajdluzsze = kierowcy.First();  // kierowca z zadaniem o najdluzszym czasie dojazdu
            int indeksNajdluzsze = -1;     // indeks zadania o najdluzszym czasie dojazdu
            double najdluzszyCzasDojazdu = 0;

            // szukamy zadania o najdluzszym czasie dojazdu
            foreach (Kierowca k in kierowcy)
            {
                for (int i = 0; i < k.ListaZlecen.Count(); i++)
                {
                    if (k.ListaZlecen[i].CzasDojazdu > najdluzszyCzasDojazdu)
                    {
                        if (!kolejkaTabu.Contains(k.ListaZlecen[i]))    // sprawdzenie, czy zadanie nie jest na liscie tabu
                        {
                            najdluzszyCzasDojazdu = k.ListaZlecen[i].CzasDojazdu;
                            kierNajdluzsze = k;
                            indeksNajdluzsze = i;
                        }
                    }
                }
            }

            // jesli nie ma zadnych zadan to wroc
            if (indeksNajdluzsze == -1)
            {
                return;
            }

            // dodajemy zadanie na kolejke tabu
            kolejkaTabu.Enqueue(kierNajdluzsze.ListaZlecen[indeksNajdluzsze]);
            if (kolejkaTabu.Count() > dlugoscKolejkiTabu)
            {
                kolejkaTabu.Dequeue();
            }

            // probujemy wstawic je na kazde mozliwe miejsce
            foreach (Kierowca k in kierowcy)
            {
                // przechodzimy do godziny, od której mozna wstawic nasze zadanie
                int idx = 0;
                while ((idx < k.ListaZlecen.Count()) && (k.ListaZlecen[idx].Godzina < kierNajdluzsze.ListaZlecen[indeksNajdluzsze].ZadanaGodzina))
                {
                    idx++;
                }

                do
                {
                    if (PrzestawJesliLepsze(kierNajdluzsze, indeksNajdluzsze, k, idx))
                    {
                        kierNajdluzsze = k;
                        indeksNajdluzsze = idx;
                    }
                    idx++;
                } while (idx < k.ListaZlecen.Count());

            }

        }

        // Sprawdza przesueniecie z k1[indeks1] do k2[indeks2]
        // Jeśli spełnia ograniczenia i jest lepsze zwraca true
        // Jeśli nie, to zwraca false
        bool PrzestawJesliLepsze(Kierowca k1, int indeks1, Kierowca k2, int indeks2)
        {
            if (k1 == k2 && indeks1 == indeks2)
            {
                return false;
            }

            if (k1 == k2)
            {
                k1.ListaZlecen.Move(indeks1, indeks2);
                PrzeliczCzasy(k1);
            }
            else
            {
                k2.ListaZlecen.Insert(indeks2, k1.ListaZlecen[indeks1]);
                k1.ListaZlecen.RemoveAt(indeks1);
                k2.ListaZlecen[indeks2].Kierowca = k2;
                PrzeliczCzasy(k1);
                PrzeliczCzasy(k2);
            }
            double nowaFunkcjaCelu = FunkcjaCelu();

            // jesli nowy harmonogram nie spelnia ograniczen lub jest gorszy od poprzedniego, 
            // to wroc do poprzedniego harmonogramu
            if (!SprawdzOgraniczenia() || (nowaFunkcjaCelu < funkcjaCelu))
            {
                if (k1 == k2)
                {
                    k1.ListaZlecen.Move(indeks2, indeks1);
                    PrzeliczCzasy(k1);
                }
                else
                {
                    k1.ListaZlecen.Insert(indeks1, k2.ListaZlecen[indeks2]);
                    k2.ListaZlecen.RemoveAt(indeks2);
                    PrzeliczCzasy(k1);
                    PrzeliczCzasy(k2);
                    k1.ListaZlecen[indeks1].Kierowca = k1;
                }
                return false;
            }
            else
            {
                funkcjaCelu = nowaFunkcjaCelu;
                return true;
            }

        }

        // Przelicza czasy dojazdu, czasy przejazdu,opoznienia, godziny i czasy pracy dla wszystkich zadan dla kierowcy k
        void PrzeliczCzasy(Kierowca k)
        {
            k.ListaZlecen[0].CzasDojazdu = 0;  // zakladam, ze dojazd z bazy na miejsce pierwszego zlecenia = 0
            k.ListaZlecen[0].CzasPrzejazdu = GPS.CzasPrzejazdu(k.ListaZlecen[0].Zlecenie.skad, k.ListaZlecen[0].Zlecenie.dokad);
            k.ListaZlecen[0].Godzina = k.ListaZlecen[0].Zlecenie.zadanaGodzina;
            for (int i = 1; i < k.ListaZlecen.Count(); i++)
            {
                k.ListaZlecen[i].CzasDojazdu = GPS.CzasPrzejazdu(k.ListaZlecen[i - 1].Zlecenie.dokad, k.ListaZlecen[i].Zlecenie.skad);
                // czas przejazdu jest staly, wiec powinien byc ustalony przy dodaniu zlecenia
                //k.ListaZlecen[0].czas_przejazdu = GPS.CzasPrzejazdu(k.ListaZlecen[i].Zlecenie.skad, k.ListaZlecen[i].Zlecenie.dokad);
                if (k.ListaZlecen[i - 1].Godzina + k.ListaZlecen[i - 1].CzasPrzejazdu + k.ListaZlecen[i].CzasDojazdu < k.ListaZlecen[i].Zlecenie.zadanaGodzina)
                {
                    k.ListaZlecen[i].Godzina = k.ListaZlecen[i].Zlecenie.zadanaGodzina;
                }
                else
                {
                    k.ListaZlecen[i].Godzina = k.ListaZlecen[i - 1].Godzina + k.ListaZlecen[i - 1].CzasPrzejazdu + k.ListaZlecen[i].CzasDojazdu;
                }

                k.ListaZlecen[i].Opoznienie = k.ListaZlecen[i].Godzina - k.ListaZlecen[i].Zlecenie.zadanaGodzina;
            }

            k.CzasPracy = k.ListaZlecen.Last().Godzina - k.ListaZlecen.First().Godzina + k.ListaZlecen.Last().CzasPrzejazdu;
        }

        // Sprawdza czy ograniczenia sa spelnione w aktualnym harmonogramie
        // 1. Maksymalne Opoznienie nie przekracza 15 min
        // 2. Roznica w czasie pracy miedzy kierowcami nie przekracza 1h
        bool SprawdzOgraniczenia()
        {
            double czasMinimalny = double.MaxValue;
            double czasMaksymalny = 0;

            foreach (Kierowca k in kierowcy)
            {
                foreach (Realizacja r in k.ListaZlecen)
                {
                    if (r.Godzina - r.Zlecenie.zadanaGodzina > maxOpoznienie)
                    {
                        return false;
                    }
                }

                if (k.CzasPracy > czasMaksymalny)
                {
                    czasMaksymalny = k.CzasPracy;
                }
                if (k.CzasPracy < czasMinimalny)
                {
                    czasMinimalny = k.CzasPracy;
                }

            }

            if (czasMaksymalny - czasMinimalny > maxRoznica)
            {
                return false;
            }
            return true;

        }

        // Zwraca wartosc funkcji celu (łączny czas dojazdów) dla obecnego harmonogramu.
        // Czasy dojazdów są proporcjonalne do zużycia paliwa.
        double FunkcjaCelu()
        {
            double fc = 0;
            foreach (Kierowca k in kierowcy)
            {
                foreach (Realizacja r in k.ListaZlecen)
                {
                    fc += r.CzasDojazdu;
                }
            }
            return fc;
        }

    }
}
