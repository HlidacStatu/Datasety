using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeToVyjadreniPolitiku
{

    public class record
    {
        public class Blok
        {
            public long sekundOdZacatku { get; set; }
            public string text { get; set; }
        }

        public static string UniqueID(string url) => Devmasters.Crypto.Hash.ComputeHashToHex(url).ToLower();

        public string server { get; set; }
        public string typserveru { get; set; }
        public string osobaid { get; set; }
        public DateTime datum { get; set; }
        public string origid { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        public int pocetSlov { get; set; }
        public string[] tags { get; set; }
        public Blok[] prepisAudia { get; set; }

    }

    public class Youtube_dl_video
    {
        public string description { get; set; }
        public int? view_count { get; set; }
        public string ie_key { get; set; }
        public string id { get; set; }
        public string _type { get; set; }
        public decimal? duration { get; set; }
        public string uploader { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

}
