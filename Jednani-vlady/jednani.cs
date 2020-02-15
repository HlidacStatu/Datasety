using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jednani_vlady
{
    public class jednani : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        public class dokument
        {
            public string HsProcessType { get; } = "document";
            public string DocumentUrl { get; set; }
            public string DocumentPlainText { get; set; }
            public string jmeno { get; set; }

            public DateTime stahnuto { get; set; } = DateTime.Now;
        }
        public class souvis
        {
            public string zmena { get; set; }
            public string usneseni { get; set; }
            public string usneseniCislo { get; set; }
            public string usneseniOrigUrl { get; set; }
        }

        public void SetId()
        {
            var s = datum.ToString("yyyyMMdd") + "-";
            if (string.IsNullOrEmpty(usneseni))
                s = s + Devmasters.Core.TextUtil.NormalizeToPureTextLower(bod).Replace(" ", "-");
            else
                s = s + $"usneseni-{usneseni.Replace("/", "-")}"; //Devmasters.Core.TextUtil.NormalizeToPureTextLower(bod).Replace(" ", "-");

            _id = s;
        }

        private string _id = null;
        public string Id {
            get { return _id; }
            set { _id = value; }
        }
        public DateTime datum { get; set; }
        public string bod { get; set; }
        public string usneseni { get; set; }
        public string[] veklep { get; set; }
        public dokument[] dokumenty { get; set; }
        public string vec { get; set; }
        public souvis[] zmeneno { get; set; }
        public souvis[] meni { get; set; }
        public souvis[] zruseno { get; set; }
        public souvis[] rusi { get; set; }
        public string cisloJednaci { get; set; }

    }
}
