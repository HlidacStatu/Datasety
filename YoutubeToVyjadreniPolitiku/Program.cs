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
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<record> api2 = null;
        static void Main(string[] arguments)
        {
            var conf = new HlidacStatu.Api.V2.CoreApi.Client.Configuration();
            conf.AddDefaultHeader("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            conf.Timeout = 180 * 1000;

            api = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(conf);

            api2 = HlidacStatu.Api.V2.Dataset.Typed.Dataset<record>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], DataSetId);

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
            int threads = 5;
            if (args.ContainsKey("/t"))
                threads = int.Parse(args["/t"]);

            string filter = "";
            if (args.ContainsKey("/filter"))
                filter = Devmasters.TextUtil.RemoveDiacritics(args["/filter"]).ToLower();

            DateTime? fromDate = null;
            if (args.ContainsKey("/fromdate"))
                fromDate = DateTime.ParseExact(args["/fromdate"], "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal);

            string[] vids = null;
            if (args.ContainsKey("/ids"))
                vids = args["/ids"].Split(',');

            string[] mp3 = null;
            if (args.ContainsKey("/mp3"))
                mp3 = args["/mp3"].Split(',');

            if (string.IsNullOrEmpty(playlist) && vids==null  )
            {
                Help(); return;
            }


            var yt = new YoutubeClient();
            List<YoutubeExplode.Videos.Video> videos = new List<YoutubeExplode.Videos.Video>();
            IReadOnlyList<YoutubeExplode.Videos.Video> videosL = null;
            if (vids == null)
            {
                videosL = yt.Playlists.GetVideosAsync(playlist).ToListAsync().Result;
            }
            else
            {
                videosL = vids
                    .Select(m => YT.GetVideoInfo(m))
                    .Where(v => v != null)
                    .ToList();
            }
            foreach (var v in videosL.OrderByDescending(m => m.UploadDate.DateTime))
            {
                Console.WriteLine("analyzing " + v.Title);
                if (!string.IsNullOrEmpty(filter))
                {
                    if (!Devmasters.TextUtil.RemoveDiacritics(v.Title.ToLower()).Contains(filter))
                        continue;
                }
                if (fromDate.HasValue)
                {
                    if (v.UploadDate.DateTime < fromDate.Value)
                        continue;
                }

                string recId = Devmasters.Crypto.Hash.ComputeHashToHex(v.Url).ToLower();

                if (Program.api.ApiV2DatasetyDatasetItemExists(Program.DataSetId, recId))
                {
                    //fix
                    if (false)
                    {
                        var vinf = YT.GetVideoInfo(v.Id);

                        string title = vinf.Title + "\n\n"
                            + vinf.Description + "\n"
                            + "---------" + "\n";

                        var item = api2.GetItem(recId);
                        item.datum = vinf.UploadDate.DateTime;
                        if (item.text.StartsWith(title) == false)
                            item.text = title + item.text;
                        api2.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                    }
                    continue;
                }

                videos.Add(v);

            }


            Console.WriteLine();
            Console.WriteLine($"Processing {videos.Count} videos, total {videos.Sum(m => m.Duration.TotalMinutes)} mins ");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Devmasters.Batch.Manager.DoActionForAll(videos,
                vid =>
                {

                    //var rec = Newtonsoft.Json.JsonConvert.DeserializeObject<record>(System.IO.File.ReadAllText(@"c:\!\yt_2CAD2F3C20C701DC7354B6E50700EB41"));
                    var idx = 0;
                    if (mp3!=null)
                        idx = mp3.ToList().IndexOf(vid.Id);
                    record rec = null;
                    string fnBak = $@"c:\!\yt_{Devmasters.Crypto.Hash.ComputeHashToHex(vid.Url).ToLower()}";
                    if (System.IO.File.Exists(fnBak))
                    {
                        rec = Newtonsoft.Json.JsonConvert.DeserializeObject<record>(System.IO.File.ReadAllText(fnBak));
                    }
                    else
                    {
                        rec = YT.process(vid, mp3==null ? null : mp3[idx]);
                    }
                    if (rec != null)
                    {
                        rec.osobaid = osobaId;
                        System.IO.File.WriteAllText(fnBak, Newtonsoft.Json.JsonConvert.SerializeObject(rec));
                        string data = Newtonsoft.Json.JsonConvert.SerializeObject(rec);
                        var ret = api.ApiV2DatasetyDatasetItemUpdate(DataSetId, rec.id, rec, "skip");
                    }

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: threads
                );


        }




        static void Help()
        {
            Console.WriteLine("YoutubeToVyjadreniPolitiku /osobaid= /playlist= /filter= /ids= /fromDate=yyyy-MM-dd /t= /mp3=");
            Console.WriteLine();

        }

    }
}
