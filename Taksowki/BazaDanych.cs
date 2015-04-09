using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;


namespace Taksowki
{
    
    [Database(Name="BazaDanych")]
    public class BazaDanych : DataContext
    {
        public BazaDanych( ) : base( "Tu ma byc chyba wpisane polaczenie z baza danych" ){}
 
        public Table<Zlecenie> Zlecenia;
        public Table<Kierowca> Kierowcy;
        public Table<Realizacja> Realizacje;
    }
}
