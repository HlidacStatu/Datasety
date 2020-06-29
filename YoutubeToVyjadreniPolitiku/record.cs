using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeToVyjadreniPolitiku
{

    public class record 
    {
        public string server { get; set; }
        public string typserveru { get; set; }
        public string osobaid { get; set; }
        public DateTime datum { get; set; }
        public string origid { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        public int pocetSlov { get; set; }
    }

}
