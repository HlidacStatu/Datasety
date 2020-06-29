using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace YoutubeToVyjadreniPolitiku
{


    class Program
    {
        public static Dictionary<string, string> args = new Dictionary<string, string>();
        public const string DataSetId = "vyjadreni-politiku";

        //public static RESTCall api = new RESTCall(System.Configuration.ConfigurationManager.AppSettings["apikey"], 60*1000);

        public static HlidacStatu.Api.V2.CoreApi.DatasetyApi api = null;

        static void Main(string[] arguments)
        {
            var conf = new HlidacStatu.Api.V2.CoreApi.Client.Configuration();
            conf.AddDefaultHeader("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            conf.Timeout = 180 * 1000;

            api = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(conf);


            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);


            //create dataset


            string osobaId = "";
            if (args.ContainsKey("/osobaid"))
                osobaId = args["/osobaid"];
            else
            {
                Help(); return;
            }
            string playlist = "";
            if (args.ContainsKey("/playlist"))
                playlist = args["/playlist"];
            else
            {
                Help(); return;
            }
            string filter = "";
            if (args.ContainsKey("/filter"))
                filter = Devmasters.Core.TextUtil.RemoveDiacritics(args["/filter"]).ToLower();

            string[] vids = null;
            if (args.ContainsKey("/ids"))
                vids = args["/ids"].Split(',');

            var yt = new YoutubeClient();
            List<string> videos = new List<string>();
            if (vids == null)
            {
                var videosL = yt.Playlists.GetVideosAsync(new YoutubeExplode.Playlists.PlaylistId(playlist)).BufferAsync(300).Result;
                foreach (var v in videosL)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (Devmasters.Core.TextUtil.RemoveDiacritics(v.Title.ToLower()).Contains(filter))
                            videos.Add(v.Id);
                    }
                    else
                        videos.Add(v.Id);
                }
            }
            else
                videos.AddRange(vids);


            foreach (var vid in videos)
            {

                //var rec = Newtonsoft.Json.JsonConvert.DeserializeObject<record>(System.IO.File.ReadAllText(@"c:\!\yt_2CAD2F3C20C701DC7354B6E50700EB41"));
                var rec = YT.process(vid);

                if (rec != null)
                {
                    rec.osobaid = osobaId;
                    System.IO.File.WriteAllText($@"c:\!\yt_{rec.id}", Newtonsoft.Json.JsonConvert.SerializeObject(rec));
                    string data = Newtonsoft.Json.JsonConvert.SerializeObject(rec);
                    var ret = api.ApiV2DatasetyDatasetItemUpdate(DataSetId, rec.id, rec, "skip");
                }
            }


        }




        static void Help()
        {
            Console.WriteLine("YoutubeToVyjadreniPolitiku /osobaid= /playlist= /filter= /ids=");
            Console.WriteLine();

        }

    }
}
