using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Taksowki
{

    [Table(Name = "Kierowca")]
    public class Kierowca : INotifyPropertyChanged
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column]
        public double czasPracy;

        public ObservableCollection<Realizacja> listaZlecen;


        private EntitySet<Realizacja> _realizacja = new EntitySet<Realizacja>();
        [Association(Name = "FK_Realizacja_Kierowca", Storage = "_realizacja", OtherKey = "kierowcaID", ThisKey = "Id")]
        internal ICollection<Realizacja> Realizacja
        {
            get { return _realizacja; }
            set { _realizacja.Assign(value); }
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
        public double CzasPracy
        {
            get { return czasPracy; }
            set
            {
                czasPracy = value;
                OnPropertyChanged("CzasPracy");
            }
        }

        public ObservableCollection<Realizacja> ListaZlecen
        {
            get { return listaZlecen; }
            set { 
                listaZlecen = value;
                OnPropertyChanged("ListaZlecen");
            }
        }
        //DEBUG
    }
}
