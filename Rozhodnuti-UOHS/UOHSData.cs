using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozhodnuti_UOHS
{
    public class UOHSData
    {
        public class Ucastnik
        {
            public string Jmeno { get; set; }
            public string ICO { get; set; }
        }
        public class Dokument
        {
            public string Url { get; set; }
            public string PlainText { get; set; }
        }
        public string Id { get; set; }
        public string Url { get; set; }
        public string Cj { get; set; }
        public string SpisovaZnacka { get; set; }
        public string Instance { get; set; }
        public Ucastnik[] Ucastnici { get; set; }
        public string Typ_spravniho_rizeni { get; set; }
        public string Typ_rozhodnuti { get; set; }
        public string Rok { get; set; }
        public DateTime? PravniMoc { get; set; }
        public string[] SouvisejiciUrl { get; set; }
        public Dokument Rozhodnuti { get; set; }
        public DateTime? Uverejneno { get; set; }
        public string Vec { get; set; }
        public string SoudniRozhodnuti { get; set; }
    }
}
