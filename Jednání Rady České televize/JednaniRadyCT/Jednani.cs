using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JednaniRadyCT
{
    public class Jednani
    {
        public class Dokument
        {

            public string Nazev { get; set; }
            public string Typ { get; set; }
            public string HsProcessType { get; set; }
            public string DocumentUrl { get; set; }
            public string DocumentPlainText { get; set; }
        }
        public class Blok
        {
            public long SekundOdZacatku { get; set; }
            public string Text { get; set; }
        }

        public string Id { get; set; }
        public string Titulek { get; set; }
        public string Odkaz { get; set; }
        public DateTime DatumJednani { get; set; }

        [Description("Delka v minutách")]
        public int Delka { get; set; }

        public Blok[] PrepisAudia { get; set; }

        public Dokument[] Zapisy { get; set; }
        public Dokument[] Materialy { get; set; }


    }
}
