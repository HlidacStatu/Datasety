using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZasedaniZastupitelstev
{

    public class Record
    {
        public class Blok
        {
            public long sekundOdZacatku { get; set; }
            public string text { get; set; }
        }

        public static string UniqueID(string url) => Devmasters.Crypto.Hash.ComputeHashToHex(url).ToLower();

        public string id { get; set; }
        public string ico { get; set; }
        public string mesto { get; set; }
        public DateTime datum { get; set; }
        public string nazev { get; set; }
        public string popis { get; set; }
        public string url { get; set; }
        public long? delka { get; set; } = 0;
        public string[] tags { get; set; }
        public string HsProcessType { get; set; } = "audio";
        public string AudioUrl { get; set; }
        public Blok[] PrepisAudia { get; set; }

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
