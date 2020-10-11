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
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<record> api2 =null;
         static void Main(string[] arguments)
        {
            var conf = new HlidacStatu.Api.V2.CoreApi.Client.Configuration();
            conf.AddDefaultHeader("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            conf.Timeout = 180 * 1000;

            api = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(conf);

            api2 = HlidacStatu.Api.V2.Dataset.Typed.Dataset<record>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"],DataSetId);

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
            int threads = 5;
            if (args.ContainsKey("/t"))
                threads = int.Parse(args["/t"]);

            string filter = "";
            if (args.ContainsKey("/filter"))
                filter = Devmasters.Core.TextUtil.RemoveDiacritics(args["/filter"]).ToLower();

            DateTime? fromDate = null;
            if (args.ContainsKey("/fromdate"))
                fromDate = DateTime.ParseExact(args["/fromdate"], "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal);

            string[] vids = null;
            if (args.ContainsKey("/ids"))
                vids = args["/ids"].Split(',');

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
                    if (!Devmasters.Core.TextUtil.RemoveDiacritics(v.Title.ToLower()).Contains(filter))
                        continue;
                }
                if (fromDate.HasValue)
                {
                    if (v.UploadDate.DateTime < fromDate.Value)
                        continue;
                }

                string recId = Devmasters.Core.CryptoLib.Hash.ComputeHashToHex(v.Url).ToLower();

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
            Console.WriteLine($"Processing {videos.Count} videos, total {videos.Sum(m=>m.Duration.TotalMinutes)} mins ");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Devmasters.Core.Batch.Manager.DoActionForAll(videos,
                vid =>
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

                    return new Devmasters.Core.Batch.ActionOutputData();
                }, Devmasters.Core.Batch.Manager.DefaultOutputWriter, Devmasters.Core.Batch.Manager.DefaultProgressWriter,
                true, maxDegreeOfParallelism: threads
                );


        }




        static void Help()
        {
            Console.WriteLine("YoutubeToVyjadreniPolitiku /osobaid= /playlist= /filter= /ids= /fromDate=yyyy-MM-dd /t=");
            Console.WriteLine();

        }

    }
}
