using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vybory_PSP
{
    public class jednani : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        public class dokument
        {
            public string HsProcessType { get; } = "documentsave";
            public string DocumentUrl { get; set; }
            public string DocumentPlainText { get; set; }
            public string jmeno { get; set; }
            public string popis { get; set; }
            public string typ { get; set; }
        }
        public class mp3
        {
            public string HsProcessType { get; } = "audiosave";
            public string DocumentUrl { get; set; }
            public string DocumentPlainText { get; set; }
            public string jmeno { get; set; }
        }

        public void SetId()
        {
            var s = $"{vyborId}-{cisloJednani}-{datum:yyyyMMdd}";

            _id = s;
        }

        private string _id = null;
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public DateTime datum { get; set; }
        public int cisloJednani { get; set; }
        public string vec { get; set; }

        public string vybor { get; set; }
        public int vyborId { get; set; }
        public string vyborUrl { get; set; }

        public dokument[] dokumenty { get; set; }
        public mp3[] audio { get; set; }

    }
}
