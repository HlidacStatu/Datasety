using Devmasters.SpeechToText;
using System.Text.RegularExpressions;
using WordcabTranscribe.SpeechToText;

namespace YoutubeToVyjadreniPolitiku
{


    class Program
    {
        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("YoutubeToVyjadreniPolitiku");

        public static Devmasters.Args args = null;
        public const string DataSetId = "vyjadreni-politiku";

        //public static RESTCall api = new RESTCall(System.Configuration.ConfigurationManager.AppSettings["apikey"], 60*1000);

        //public static HlidacStatu.Api.V2.CoreApi.DatasetyApi api = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<rec> api2 = null;
        static HlidacStatu.Api.VoiceToText.Client v2tApi = null;
        static HttpClient httpcl = new HttpClient();

        static void Main(string[] arguments)
        {

            args = new Devmasters.Args(arguments, new string[] { "/mp3path" });
            logger.Info("Starting with params" + string.Join(" ", args.Arguments));


            //create dataset

            if (!args.MandatoryPresent())
            { Help(); return; }

            api2 = HlidacStatu.Api.V2.Dataset.Typed.Dataset<rec>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], DataSetId);


            string osobaId = args["/osobaid"];

            string playlist = args["/playlist"];

            int threads = args.GetNumber("/t") ?? 5;

            int max = args.GetNumber("/max") ?? 300;


            string[] vids = args.GetArray("/ids");

            string mp3path = args["/mp3path"];


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);


            //Prvni zkontroluj zpracovane VoiceToText
            //System.Net.Http.HttpClient.DefaultProxy = new System.Net.WebProxy("127.0.0.1", 8888);

            Console.WriteLine("Loading voice2text results");
            v2tApi = new HlidacStatu.Api.VoiceToText.Client(System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            var tasks = v2tApi.GetTasksAsync(1000, "vyjadreni-politiku", status: HlidacStatu.DS.Api.Voice2Text.Task.CheckState.Done)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    Console.WriteLine($"procesing voice2text results for task {task.QId}");
                    if (string.IsNullOrEmpty(task.Result))
                        continue;
                    if (task.Status == HlidacStatu.DS.Api.Voice2Text.Task.CheckState.Error)
                        continue;
                    
                    WordcabTranscribe.SpeechToText.TranscribeResult res = System.Text.Json.JsonSerializer.Deserialize<WordcabTranscribe.SpeechToText.TranscribeResult>(
                        task.Result, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    var text = res.ToTerms().ToText(true);
                    if (text == "<EMPTY AUDIO>")
                    {
                        text = "";
                        res.utterances = Array.Empty<TranscribeResult.Utterance>();
                    }
                    var prepis = res.ToTerms()
                          .Select(t => new rec.Blok() { sekundOdZacatku = (long)t.TimestampInTS.TotalSeconds, text = t.Value })
                          .ToArray();
                    try
                    {
                        var vp_record = api2.GetItemSafe(task.CallerTaskId);
                        if (vp_record != null && !string.IsNullOrEmpty(text))
                        {
                            Console.WriteLine($"procesing YT video title&Description for task {task.QId}");
                            if (string.IsNullOrEmpty(vp_record.titulek))
                            {
                                //vezmi to z YT videa
                                var ytrec = YTDL.GetVideoInfo(vp_record.url);
                                if (ytrec != null)
                                {
                                    vp_record.titulek = ytrec.titulek;
                                    vp_record.popis = ytrec.popis;
                                }
                                else
                                {
                                    System.Threading.Thread.Sleep(20000);
                                    continue;//skip to next record
                                }
                            }

                            vp_record.text = text;
                            vp_record.prepisAudia = prepis;
                            vp_record.pocetSlov = CountWords(text);
                            Console.WriteLine($"saving prepis into dataset for task {task.QId}");
                            _ = api2.AddOrUpdateItem(vp_record, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }
                        Console.WriteLine($"changing status for task {task.QId}");
                        bool ok = v2tApi.SetTaskStatusAsync(task.QId, HlidacStatu.DS.Api.Voice2Text.Task.CheckState.ResultTaken)
                            .ConfigureAwait(false).GetAwaiter().GetResult();

                    }
                    catch (Exception)
                    {

                    }
                }
            }


            var jsonResult = httpClient.GetStringAsync("https://api.hlidacstatu.cz/api/v2/osoby/social?typ=Youtube")
                        .Result;
            var osoby = Newtonsoft.Json.JsonConvert.DeserializeObject<osoba[]>(jsonResult);
            foreach (var o in osoby)
            {

                foreach (var url in o.SocialniSite)
                {
                    if (string.IsNullOrEmpty(osobaId))
                        Process(o, url.Url, threads, max, vids, mp3path);
                    else if (o.NameId == osobaId)
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

                    string uniqId = YoutubeToVyjadreniPolitiku.rec.UniqueID(vid);
                    rec rec = null;
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
                        var localUrl = $"https://somedata.hlidacstatu.cz/mp3/{DataSetId}/{rec.id}.mp3";
                        var respLocal = httpcl.GetAsync(localUrl, HttpCompletionOption.ResponseHeadersRead)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        if (respLocal.IsSuccessStatusCode)
                        {
                            //Console.WriteLine(localUrl);
                            bool diarization = false;
                            if (rec.delka_s < 90 * 60)//90 min
                                diarization = true;
                            _ = v2tApi.AddNewTaskAsync(
                                new HlidacStatu.DS.Api.Voice2Text.Options()
                                {
                                    datasetName = DataSetId,
                                    itemId = rec.id,
                                    audioOptions = new WordcabTranscribe.SpeechToText.AudioRequestOptions() { diarization = diarization, source_lang = "cs" }
                                },
                                new Uri(localUrl), DataSetId, rec.id, 2);

                        }
                    }
                    /*
                    if (exists_S2T && !(rec.prepisAudia?.Count() > 0))
                    {
                        if (System.IO.File.Exists(dockerFn))
                        {
                            var tt = new KaldiASR.SpeechToText.VoiceToTerms(System.IO.File.ReadAllText(dockerFn));
                            var blocks = new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms)
                               .TextWithTimestamps(TimeSpan.FromSeconds(10), true)
                               .Select(t => new rec.Blok() { sekundOdZacatku = (long)t.Start.TotalSeconds, text = t.Text })
                               .ToArray();

                            //TODO opravit casem
                            var tmpRec = YTDL.GetVideoInfo(vid);
                            if (tmpRec != null)
                                rec.text = tmpRec.text + "\n\n" + new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms).Text(true);
                            rec.prepisAudia = blocks;
                            changed = true;

                        }
                    }
                    */
                    if (changed)
                        api2.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: threads
                );


        }



        public static int CountWords(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            MatchCollection matchCollection = Regex.Matches(s, "[\\S]+");
            return matchCollection.Count;
        }
        static void Help()
        {
            Console.WriteLine("YoutubeToVyjadreniPolitiku /osobaid= /mp3path= /playlist= /ids= /t= /max=");
            Console.WriteLine();

        }

    }
}
