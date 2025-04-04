﻿using Devmasters.Log;
using Devmasters.SpeechToText;
using HlidacStatu.Api.V2.Dataset.Client;
using HlidacStatu.Api.V2.Dataset;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace ZasedaniZastupitelstev
{


    class Program
    {
        public static IConfigurationRoot jsonconf;

        static string[] inName = { "zastupitelstv", "jednani", "zasedani" };
        public static Devmasters.Args args = null;
        public const string DataSetId = "zasedani-zastupitelstev";
        static string apikey = "";
        static string mp3path = "";
        //public static RESTCall api = new RESTCall(apikey, 60*1000);

        //public static HlidacStatu.Api.V2.CoreApi.DatasetyApi api = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record> api = null;

        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("ZasedaniZastupitelstev",
    Devmasters.Log.Logger.DefaultConfiguration()
    .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
    .AddFileLoggerFilePerLevel("/Data/Logs/ZasedaniZastupitelstev/", "slog.txt",
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                      rollingInterval: RollingInterval.Day,
                      fileSizeLimitBytes: null,
                      retainedFileCountLimit: 9,
                      shared: true
                      )
    .WriteTo.Console()
   );

        public static Devmasters.Batch.MultiOutputWriter outputWriter =
             new Devmasters.Batch.MultiOutputWriter(
                Devmasters.Batch.Manager.DefaultOutputWriter,
                new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Debug).OutputWriter
             );

        public static Devmasters.Batch.MultiProgressWriter progressWriter =
            new Devmasters.Batch.MultiProgressWriter(
                new Devmasters.Batch.ActionProgressWriter(1.0f, Devmasters.Batch.Manager.DefaultProgressWriter).Writer,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Information).ProgressWriter).Writer
            );

        public static HlidacStatu.Api.VoiceToText.Client v2tApi = null;

        static void Main(string[] arguments)
        {


            jsonconf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.live.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .Build();

            apikey = jsonconf["apikey"];
            mp3path = jsonconf["mp3path"];


            args = new Devmasters.Args(arguments);


            //create dataset

            if (!args.MandatoryPresent() || args.Exists("/?") || args.Exists("/h"))
            { Help(); return; }

            string ico = args.Get("/ico");

            int threads = args.GetNumber("/t") ?? 5;

            int max = args.GetNumber("/max") ?? 300;
            string playlist = args.Get("/playlist");

            string[] vids = args.GetArray("/ids");

            string filter = args.Get("/filter", null);


            System.IO.Directory.CreateDirectory(mp3path + @"\" + DataSetId);


            try
            {
                api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.OpenDataset(apikey, DataSetId);

            }
            catch (ApiException e)
            {
                //api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.CreateDataset(apikey, Registration());

            }
            catch (Exception e)
            {
                throw;
            }


            //Prvni zkontroluj zpracovane VoiceToText
            //System.Net.Http.HttpClient.DefaultProxy = new System.Net.WebProxy("127.0.0.1", 8888);
            HttpClient apiHttpClient = new HttpClient();
            apiHttpClient.DefaultRequestHeaders.Add("Authorization", apikey);

            v2tApi = new HlidacStatu.Api.VoiceToText.Client(apikey, timeOut: TimeSpan.FromMinutes(10));
            HlidacStatu.DS.Api.Voice2Text.Task[] tasks = null;

            do
            {
                Console.WriteLine("Loading zasedani-zastupitelstev voice2text results");

                tasks = v2tApi.GetTasksAsync(10, DataSetId, status: HlidacStatu.DS.Api.Voice2Text.Task.CheckState.Done)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        try
                        {
                            Console.WriteLine($"procesing voice2text results for task {task.QId}");
                            if (task.Result.Any() == false)
                                goto setstatus;

                            if (task.Status == HlidacStatu.DS.Api.Voice2Text.Task.CheckState.Error)
                                continue;

                            Term[] terms = task.Result;
                            var text = terms.ToText(true);
                            var prepis = terms.ToTextWithTimestamps(TimeSpan.FromSeconds(20), speakerTagName: "speaker")
                                  .Select(t => new Record.Blok() { sekundOdZacatku = (long)t.Start.TotalSeconds, text = t.Text })
                                  .ToArray();
                            var vp_record = api.GetItemSafe(task.CallerTaskId);
                            if (vp_record != null && !string.IsNullOrEmpty(text))
                            {
                                Console.WriteLine($"procesing YT video title&Description for task {task.QId}");
                                if (string.IsNullOrEmpty(vp_record.nazev))
                                {
                                    //vezmi to z YT videa
                                    var ytrec = YTDL.GetVideoInfo(vp_record.url);
                                    if (ytrec != null)
                                    {
                                        vp_record.nazev = ytrec.nazev;
                                        vp_record.popis = ytrec.popis;
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(20000);
                                        continue;//skip to next record
                                    }
                                }

                                //vp_record. = text;
                                vp_record.PrepisAudia = prepis;
                                //vp_record.pocetSlov = CountWords(text);
                                Console.WriteLine($"saving prepis into dataset for task {task.QId}");
                                _ = api.AddOrUpdateItem(vp_record, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                            }

                        setstatus:
                            Console.WriteLine($"changing status for task {task.QId}");
                            bool ok = v2tApi.SetTaskStatusAsync(task.QId, HlidacStatu.DS.Api.Voice2Text.Task.CheckState.ResultTaken)
                                .ConfigureAwait(false).GetAwaiter().GetResult();

                        }
                        catch (Exception)
                        {

                        }
                    }
                }

            } while (tasks?.Any() == true);






            var jsonResult = apiHttpClient.GetStringAsync("https://api.hlidacstatu.cz/api/v2/firmy/social?typ=Zaznam_zastupitelstva")
                        .Result;
            var firmy = Newtonsoft.Json.JsonConvert.DeserializeObject<firma[]>(jsonResult);
            if (!string.IsNullOrEmpty(ico) && !string.IsNullOrEmpty(playlist))
            {
                ProcessIco(ico, playlist, threads, max, vids, filter);

            }
            else
            {
                foreach (var f in firmy.Where(m => string.IsNullOrEmpty(ico) || m.Ico == ico))
                {

                    foreach (var url in f.SocialniSite)
                    {
                        ProcessIco(f, url.Url, threads, max, vids, filter);
                    }
                }
            }

        }

        public class firma
        {
            public string Ico { get; set; }
            public string Jmeno { get; set; }
            public string Profile { get; set; }
            public Socialnisite[] SocialniSite { get; set; } = new Socialnisite[] { };

            public class Socialnisite
            {
                public string Type { get; set; }
                public string Id { get; set; }
                public string Url { get; set; }
            }
        }




        public static void ProcessIco(firma f, string playlist, int threads, int max, string[] vids, string filter)
        {
            logger.Info("Starting {Firma} ({Ico}) for {playlist} ", f.Ico, f.Jmeno);
            ProcessIco(f.Ico, playlist, threads, max, vids, filter);
        }

        public static void ProcessIco(string fIco, string playlist, int threads, int max, string[] vids, string filter)
        {
            logger.Info("Starting {Ico} for {playlist} ", fIco, playlist);

            var apiConf = new HlidacStatu.Api.V2.Dataset.Client.Configuration();
            apiConf.DefaultHeaders.Add("Authorization", apikey);
            apiConf.Timeout = 180 * 1000;


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
                logger.Info("Starting yt-dlp playlist video list with {param}", $"--flat-playlist --get-id --playlist-end {max} {playlist}");
                pe.Start();

                videos = pe.StandardOutput
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => "https://www.youtube.com/watch?v=" + m)
                    .ToList();
            }
            Console.WriteLine();
            Program.logger.Info($"Processing {videos.Count} videos");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Devmasters.Batch.Manager.DoActionForAll(videos,
                vid =>
                {

                    string uniqId = Record.UniqueID(vid);
                    Record rec = null;
                    bool merge = false;
                    bool changed = false;
                    if (Program.api.ItemExists(uniqId))
                    {
                        rec = Program.api.GetItem(uniqId);
                        merge = true;
                    }
                    else
                    {
                        rec = YTDL.GetVideoInfo(vid);
                        if (rec == null)
                            return new Devmasters.Batch.ActionOutputData();
                        if (vids == null || vids?.Count() == 0)
                        {
                            bool okVideo = false;

                            okVideo = inName.Any(n => Devmasters.TextUtil.RemoveDiacritics(rec.nazev).ToLower().Contains(n));
                            okVideo = okVideo || inName.Any(n => Devmasters.TextUtil.RemoveDiacritics(rec.popis).ToLower().Contains(n));
                            //vyjimky
                            //usti
                            okVideo = okVideo || (fIco == "00081531" && rec.nazev.Contains("ZM "));
                            if (okVideo == false)
                            {
                                logger.Info($"Name: {rec.nazev}\nSkip {rec.url} ");
                                return new Devmasters.Batch.ActionOutputData();
                            }
                        }
                        rec.AudioUrl = "https://somedata.hlidacstatu.cz/mp3/" + DataSetId + $"/{rec.id}.mp3";
                        rec.ico = fIco;
                        changed = true;
                    }

                    if (filter != null && rec.nazev.ToLower().Contains(filter) == false)
                        return new Devmasters.Batch.ActionOutputData();


                    var mp3 = new MP3(mp3path, apikey);
                    mp3.CheckDownloadAndStartV2TOrGet(DataSetId, rec.id, vid);

                    Program.logger.Info("Saving converted Text into rec {recId}", rec.id);
                    api.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);

                    return new Devmasters.Batch.ActionOutputData();
                },
                Program.outputWriter.OutputWriter,
                Program.progressWriter.ProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: threads
                );


        }


        static HlidacStatu.Api.V2.Dataset.Model.Registration Registration()
        {
            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(Record)).ToString();

            HlidacStatu.Api.V2.Dataset.Model.Registration reg = new HlidacStatu.Api.V2.Dataset.Model.Registration(
                "Jednání zastupitelstev", DataSetId,
                "",
                "https://github.com/HlidacStatu/Datasety/tree/master/deMinimis/ZasedaniZastupitelstev",
                "Přepis jednání zastupitelstev.",
                genJsonSchema, betaversion: true, allowWriteAccess: false,
                orderList: new string[,] {
                    { "Podle datumu konání", "DatumJednani" },
                },
                defaultOrderBy: "DatumJednani desc",
                searchResultTemplate: new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("Detail", @"<a href=""{{ fn_DatasetItemUrl item.id }}"">{{item.nazev}}</a>")
                    .AddColumn("Datum jednání", "{{ fn_FormatDate item.datum }}")
                    .AddColumn("Délka", "{{item.delka}}&nbsp;min")
                    .AddColumn("", @"{{if item.PrepisAudia && item.PrepisAudia.size > 0 }}
  Přepis audiozáznamu dostupný
{{end}}")
                ,
                detailTemplate: new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("ID jednání", @"{{item.id}}")
                    .AddColumn("Jednání", "{{item.nazev}}")
                    .AddColumn("Datum jednání", "{{ fn_FormatDate item.datum }}")
                    .AddColumn("Odkaz na audio", "<a target='_blank' href='{ { item.AudioUrl } }'>{{item.AudioUrl }}</a>")
                    .AddColumn("Délka audio záznamu", "{{ item.delka }} min")
                    .AddColumn(null, @"
{{if item.PrepisAudia && item.PrepisAudia.size > 0 }}

    <b style='font-size:140%;'>Přepis audio záznamu</b> <i class='text-muted'>(vznikl díky velké pomoci od <a href='https://twitter.com/OndrejKlejch'>Ondřeje Klejcha</a> z <a href='https://twitter.com/InfAtEd'>University of Edinburgh</a>.)</i><br/><br/>
    {{if item.PrepisAudia && item.PrepisAudia.size > 0 }}
       <audio style='width:99%' id='player' controls src='{{item.AudioUrl}}' type='audio/mp3'>
         Váš prohlížeč neumí přehrávat MP3 z prohlížeče.
       </audio>
       <b>Stačí kliknout na větu v textu a spustí se audiozáznam z daného místa</b>. <i>V Safari na OSX zlobí posun v souboru, doporučujeme Chrome</i>
       <script>
         var pl = document.querySelector('#player');
         function skipTo(sec)
         {
           pl.pause();
           pl.currentTime=0;
           if (sec < 4) { sec = 0; } else { sec = sec-4;} 
       
           pl.currentTime=sec;
           pl.play();
         }
         pl.currentTime= new URLSearchParams(window.location.search).get('t');
         pl.pause();
       </script>
    {{end}}

    </td></tr>
    <tr><td colspan=2>
    <pre class='formatted'>
    {{ for bl in item.PrepisAudia -}}
        <span title='Začíná v {{ timespan.from_seconds bl.SekundOdZacatku.Value | object.format 'c'}}' class='playme' onclick='javascript:skipTo({{bl.SekundOdZacatku.Value}});' >{{bl.Text}}</span>
    {{- end}}
    </pre>
    </td></tr>

{{end}}
")
                );


            return reg;

        }


        static void Help()
        {
            Console.WriteLine("ZasedaniZastupitelstev /ico= /playlist= /ids= /t= /max=");
            Console.WriteLine();

        }

    }
}
