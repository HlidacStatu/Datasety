using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{
    public class Steno 
    {
        //csv rok,datum,schuze,fn,autor,funkce,tema,text

        public string id { get { return $"{obdobi}_{schuze}_{poradi:00000}"; } set { } }
        public int poradi { get; set; }

        public int obdobi { get; set; }
        public DateTime? datum { get; set; }
        public int schuze { get; set; }
        public string url { get; set; }

        public int? cisloHlasovani { get; set; } = null;

        public string celeJmeno { get; set; }
        public DateTime? narozeni { get; set; } 
        public string HsProcessType { get; set; } = "person";
        public string OsobaId { get; set; }
        public string funkce { get; set; }
        public string tema { get; set; }
        public string text { get; set; }

        public long pocetSlov { get { return ParsePSPWeb.CountWords(this.text ?? ""); } set { } }
        public long dobaProslovuSec { get { return this.pocetSlov/2; } set { } }
        public string politickaStrana { get; set; }

        public string[] politiciZminky { get; set; }
        public string[] temata { get; set; }
    }
}
