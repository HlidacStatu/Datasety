using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiskove_konference_vlady_CR
{
    public class zapis : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        public class vstup
        {
            public string HsProcessType { get; } = "person";
            public string jmeno { get; set; }
            public string prijmeni { get; set; }
            public string osobaId { get; set; }
            public DateTime? narozeni { get; set; }
            public string osobainfo { get; set; }

            public string text { get; set; }
            public int poradi { get; set; }

            public int pocetslov { get; set; } = 0;

        }
        public class odkaz
        {
            public string url { get; set; }
            public string nazev { get; set; }
        }

        private string _id = null;
        public string Id {
            get { return _id; }
            set { _id = value; }
        }
        public DateTime datum { get; set; }
        public string nazev { get; set; }
        public string url { get; set; }
        public odkaz audio { get; set; }
        public vstup[] vstupy { get; set; }
        public odkaz[] souvisejici { get; set; }

        public int celkovyPocetSlov { get; set; } = 0;


        public void PrepareBeforeSave()
        {
            foreach (var v in this.vstupy)
            {
                v.pocetslov = Parse.CountWords(v.text);
            }
            this.celkovyPocetSlov = this.vstupy.Sum(m => m.pocetslov);
        }

    }
}
