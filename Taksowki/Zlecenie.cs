using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Taksowki
{
    [Table(Name = "Zlecenie")]
    public class Zlecenie
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column]
        public GPS skad { get; set; }

        [Column]
        public GPS dokad { get; set; }

        [Column]
        public double zadanaGodzina { get; set; }

        static public int iloscZlecen=0;

        private EntitySet<Realizacja> _realizacja = new EntitySet<Realizacja>();
        [Association(Name = "FK_Realizacja_Zlecenie", Storage = "_realizacja", OtherKey = "zlecenieID", ThisKey = "Id")]
        internal ICollection<Realizacja> Realizacja
        {
            get { return _realizacja; }
            set { _realizacja.Assign(value); }
        }
    }
}
