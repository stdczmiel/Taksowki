using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace Taksowki
{

    [Table(Name = "Realizacja")]
    public class Realizacja : INotifyPropertyChanged
    {
        [Column(IsPrimaryKey = true, Name = "Zlecenie")]
        private int zlecenieId;

        [Column(IsPrimaryKey = true, Name = "Kierowca")]
        private int kierowcaId;

        [Column]
        public double czasDojazdu { get; set; }    // czas przygotowania zadania (zmienny - zalezy od polożenia taksowki)

        [Column]
        public double czasPrzejazdu { get; set; }  // czas wykonania zadania (stały - zalezy od odległości między lokacjami)

        [Column]
        public double godzina { get; set; }

        [Column]
        public double opoznienie { get; set; }

        //// potrzebne do listy dwukierunkowej
        //Realizacja nastepna;
        //Realizacja poprzednia;
           


        private EntityRef<Zlecenie> _zlecenie = new EntityRef<Zlecenie>();
        [Association(Name = "FK_Realizacja_Zlecenie", IsForeignKey = true, Storage = "_zlecenie", ThisKey = "zlecenieId")]
        public Zlecenie Zlecenie
        {
            get { return _zlecenie.Entity; }
            set { _zlecenie.Entity = value; }
        }


        private EntityRef<Kierowca> _kierowca = new EntityRef<Kierowca>();
        [Association(Name = "FK_Realizacja_Kierowca", IsForeignKey = true, Storage = "_kierowca", ThisKey = "kierowcaId")]
        public Kierowca Kierowca
        {
            get { return _kierowca.Entity; }
            set { _kierowca.Entity = value; }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        //DEBUG

        public double CzasDojazdu
        {
            get { return czasDojazdu; }
            set
            {
                czasDojazdu = value;
                OnPropertyChanged("CzasDojazdu");
            }
        }

        public double CzasPrzejazdu
        {
            get { return czasPrzejazdu; }
            set
            {
                czasPrzejazdu = value;
                OnPropertyChanged("CzasPrzejazdu");
            }
        }

        public double Godzina
        {
            get { return godzina; }
            set
            {
                godzina = value;
                OnPropertyChanged("Godzina");
            }
        }

        public double Opoznienie
        {
            get { return opoznienie; }
            set
            {
                opoznienie = value;
                OnPropertyChanged("Opoznienie");
            }
        }

        public int ID
        {
            get { return Zlecenie.Id; }
        }

        public double ZadanaGodzina
        {
            get { return Zlecenie.zadanaGodzina; }
        }

        //DEBUG
    }
}
