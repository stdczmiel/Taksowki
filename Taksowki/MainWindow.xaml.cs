using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Zmiana

namespace Taksowki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TabuSearch TS;

        public MainWindow()
        {
            InitializeComponent();
            TS = new TabuSearch();
            DataContext = this;
            listviewHarmonogram.ItemsSource= TS.kierowcy;
        }

        private void buttonDodaj_Click(object sender, RoutedEventArgs e)
        {
            Zlecenie z = new Zlecenie();
            z.dokad = new GPS(double.Parse(textbox_dokad1.Text),double.Parse(textbox_dokad2.Text));
            z.skad = new GPS(double.Parse(textbox_skad1.Text),double.Parse(textbox_skad2.Text));
            z.zadanaGodzina = double.Parse(textbox_godzina.Text);
            TS.DodajZlecenie(z);
            Label_funkcjaCelu.Content = TS.funkcjaCelu.ToString("F1");
        }
    }
}
