﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeToVyjadreniPolitiku
{


    class Program
    {
        public static Devmasters.Logging.Logger logger = new Devmasters.Logging.Logger("YoutubeToVyjadreniPolitiku");

        public static Devmasters.Args args = null;
        public const string DataSetId = "vyjadreni-politiku";

        //public static RESTCall api = new RESTCall(System.Configuration.ConfigurationManager.AppSettings["apikey"], 60*1000);

        public static HlidacStatu.Api.V2.CoreApi.DatasetyApi api = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<record> api2 = null;
        static void Main(string[] arguments)
        {
            var conf = new HlidacStatu.Api.V2.CoreApi.Client.Configuration();
            conf.AddDefaultHeader("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            conf.Timeout = 180 * 1000;

            api2 = HlidacStatu.Api.V2.Dataset.Typed.Dataset<record>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], DataSetId);

            args = new Devmasters.Args(arguments, new string[] { "/mp3path" });
            logger.Info("Starting with params" + string.Join(" ", args.Arguments));


            //create dataset

            if (!args.MandatoryPresent())
            { Help(); return; }

            string osobaId = args["/osobaid"];

            string playlist = args["/playlist"];

            int threads = args.GetNumber("/t") ?? 5;

            int max = args.GetNumber("/max") ?? 300;


            string[] vids = args.GetArray("/ids");

            string mp3path = args["/mp3path"];


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);

            var jsonResult = httpClient.GetStringAsync("https://api.hlidacstatu.cz/api/v2/osoby/social?typ=Youtube")
                        .Result;
            var osoby = Newtonsoft.Json.JsonConvert.DeserializeObject<osoba[]>(jsonResult);
            foreach (var o in osoby)
            {

                foreach (var url in o.SocialniSite)
                {
                    if (string.IsNullOrEmpty(osobaId))
                        Process(o, url.Url, threads, max, vids,mp3path);
                    else if (o.NameId== osobaId)
                        Process(o, url.Url, threads, max, vids, mp3path);
                }
            }

        }


        public class osoba
        {
            public string TitulPred { get; set; }
            public string Jmeno { get; set; }
            public string Prijmeni { get; set; }
            public string NameId { get; set; }
            public string Profile { get; set; }
            public Socialnisite[] SocialniSite { get; set; }
            public class Socialnisite
            {
                public string Type { get; set; }
                public string Id { get; set; }
                public string Url { get; set; }
            }
        }



        public static void Process(osoba o, string playlist, int threads, int max, string[] vids, string mp3path)
        {
            logger.Info($"Starting {o.Jmeno} {o.Prijmeni} {o.NameId} for {playlist} ");

            List<string> videos = null;
            if (vids?.Count() > 0)
                videos = vids
                    .Select(m => "https://www.youtube.com/watch?v=" + m)
                    .ToList();
            else
            {

                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo("yt-dlp",
                    $"--flat-playlist --get-id --playlist-end {max} " + playlist
                    );
                Devmasters.ProcessExecutor pe = new Devmasters.ProcessExecutor(pi, 60 * 6 * 24);
                logger.Info($"Starting yt-dlp playlist video list ");
                pe.Start();

                videos = pe.StandardOutput
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => "https://www.youtube.com/watch?v=" + m)
                    .ToList();
            }
            Console.WriteLine();
            Console.WriteLine($"Processing {videos.Count} videos");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Devmasters.Batch.Manager.DoActionForAll(videos,
                vid =>
                {

                    string uniqId = record.UniqueID(vid);
                    record rec = null;
                    bool merge = false;
                    bool changed = false;
                    if (Program.api2.ItemExists(uniqId))
                    {
                        rec = Program.api2.GetItem(uniqId);
                        merge = true;
                    }
                    else
                    {
                        rec = YTDL.GetVideoInfo(vid);
                        if (rec == null)
                            return new Devmasters.Batch.ActionOutputData();

                        rec.osobaid = o.NameId;
                        changed = true;
                    }
                    string recId = uniqId;
                    string fnFile = $"{mp3path}\\{DataSetId}\\{recId}";
                    var MP3Fn = $"{fnFile}.mp3";
                    var newtonFn = $"{fnFile}.mp3.raw_s2t";
                    var dockerFn = $"{fnFile}.ctm";

                    if (System.IO.File.Exists(MP3Fn) == false)
                    {
                        System.Diagnostics.ProcessStartInfo piv =
                        new System.Diagnostics.ProcessStartInfo("yt-dlp.exe",
                            $"--no-progress --extract-audio --audio-format mp3 --postprocessor-args \" -ac 1 -ar 16000\" -o \"{fnFile}.%(ext)s\" " + vid
                            );
                        Devmasters.ProcessExecutor pev = new Devmasters.ProcessExecutor(piv, 60 * 6 * 24);
                        pev.StandardOutputDataReceived += (ox, e) => { logger.Debug(e.Data); };

                        logger.Info($"Starting yt-dlp for {vid} ");
                        pev.Start();

                    }
                    bool exists_S2T = System.IO.File.Exists(newtonFn) || System.IO.File.Exists(dockerFn);
                    if (exists_S2T == false && rec.prepisAudia == null)
                    {
                        using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(
                            $"https://api.hlidacstatu.cz/api/v2/internalq/Voice2TextNewTask/{DataSetId}/{recId}?priority=2")
                        )
                        {
                            net.Method = Devmasters.Net.HttpClient.MethodEnum.POST;
                            net.RequestParams.Headers.Add("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
                            net.GetContent();
                        }
                    }
                    if (exists_S2T && !(rec.prepisAudia?.Count() > 0))
                    {
                        if (System.IO.File.Exists(dockerFn))
                        {
                            var tt = new KaldiASR.SpeechToText.VoiceToTerms(System.IO.File.ReadAllText(dockerFn));
                            var blocks = new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms)
                               .TextWithTimestamps(TimeSpan.FromSeconds(10), true)
                               .Select(t => new record.Blok() { sekundOdZacatku = (long)t.Start.TotalSeconds, text = t.Text })
                               .ToArray();

                            //TODO opravit casem
                            var tmpRec = YTDL.GetVideoInfo(vid);
                            if (tmpRec != null)
                                rec.text = tmpRec.text + "\n\n" + new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms).Text(true);
                            rec.prepisAudia = blocks;
                            changed = true;

                        }
                    }
                    if (changed)
                        api2.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: threads
                );


        }




        static void Help()
        {
            Console.WriteLine("YoutubeToVyjadreniPolitiku /osobaid= /mp3path= /playlist= /ids= /t= /max=");
            Console.WriteLine();

        }

    }
}
