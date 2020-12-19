using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ZasedaniZastupitelstev
{


    class Program
    {

        static string[] inName = { "zastupitelstv","jednani","zasedani" };
        public static Devmasters.Args args = null;
        public const string DataSetId = "zasedani-zastupitelstev";

        //public static RESTCall api = new RESTCall(System.Configuration.ConfigurationManager.AppSettings["apikey"], 60*1000);

        //public static HlidacStatu.Api.V2.CoreApi.DatasetyApi api = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record> api = null;
        static void Main(string[] arguments)
        {
            var conf = new HlidacStatu.Api.V2.CoreApi.Client.Configuration();
            conf.AddDefaultHeader("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            conf.Timeout = 180 * 1000;

            try
            {
                api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], DataSetId);

            }
            catch (ApiException e)
            {
                api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.CreateDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], Registration());

            }
            catch (Exception e)
            {
                throw;
            }

            args = new Devmasters.Args(arguments, new string[] { "/ico" });


            //create dataset

            if (!args.MandatoryPresent())
            { Help(); return; }

            string ico = args["/ico"];

            string playlist = args["/playlist"];

            int threads = args.GetNumber("/t") ?? 5;

            int max = args.GetNumber("/max") ?? 300;

            string filter = args.Get("/filter", null);

            string[] vids = args.GetArray("/ids");

            string mp3path = System.Configuration.ConfigurationManager.AppSettings["mp3path"];
            if (string.IsNullOrEmpty(mp3path))
            { Help(); return; }
            else
                System.IO.Directory.CreateDirectory(mp3path + @"\" + DataSetId);



            List<string> videos =null;
            if (vids?.Count() > 0)
                videos = vids
                    .Select(m => "https://www.youtube.com/watch?v=" + m)
                    .ToList();
            else
            {

                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo("youtube-dl",
                    $"--flat-playlist --get-id --playlist-end {max} " + playlist
                    );
                Devmasters.ProcessExecutor pe = new Devmasters.ProcessExecutor(pi, 60 * 6 * 24);
                Devmasters.Logging.Logger.Root.Info($"Starting Youtube-dl playlist video list ");
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

                        if (!inName.Any(n => Devmasters.TextUtil.RemoveDiacritics(rec.nazev).ToLower().Contains(n)))
                        {
                            Devmasters.Logging.Logger.Root.Info($"Skip {rec.url} ");
                            return new Devmasters.Batch.ActionOutputData();
                        }
                        rec.AudioUrl = "https://somedata.hlidacstatu.cz/mp3/" + DataSetId + $"/{rec.id}.mp3";
                        rec.ico = ico;
                        changed = true;
                    }

                    if (filter !=null && rec.nazev.ToLower().Contains(filter)==false)
                        return new Devmasters.Batch.ActionOutputData();


                    string recId = rec.id;
                    string fnFile = $"{mp3path}\\{DataSetId}\\{recId}";
                    var MP3Fn = $"{fnFile}.mp3";
                    var newtonFn = $"{fnFile}.mp3.raw_s2t";
                    var dockerFn = $"{fnFile}.ctm";

                    if (System.IO.File.Exists(MP3Fn) == false)
                    {
                        System.Diagnostics.ProcessStartInfo piv =
                        new System.Diagnostics.ProcessStartInfo("youtube-dl.exe",
                            $"--no-progress --extract-audio --audio-format mp3 --postprocessor-args \" -ac 1 -ar 16000\" -o \"{fnFile}.%(ext)s\" " + vid
                            );
                        Devmasters.ProcessExecutor pev = new Devmasters.ProcessExecutor(piv, 60 * 6 * 24);
                        pev.StandardOutputDataReceived += (o, e) => { Devmasters.Logging.Logger.Root.Debug(e.Data); };

                        Devmasters.Logging.Logger.Root.Info($"Starting Youtube-dl for {vid} ");
                        pev.Start();

                    }
                    bool exists_S2T = System.IO.File.Exists(newtonFn) || System.IO.File.Exists(dockerFn);
                    if (exists_S2T == false && rec.PrepisAudia == null)
                    {
                        using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(
                            $"https://www.hlidacstatu.cz/api/v2/internalq/Voice2TextNewTask/{DataSetId}/{recId}")
                        )
                        {
                            net.Method = Devmasters.Net.HttpClient.MethodEnum.POST;
                            net.RequestParams.Headers.Add("Authorization", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
                            net.GetContent();
                        }
                    }
                    if (exists_S2T && !(rec.PrepisAudia?.Count()>0))
                    {
                        if (System.IO.File.Exists(dockerFn))
                        {
                            var tt = new KaldiASR.SpeechToText.VoiceToTerms(System.IO.File.ReadAllText(dockerFn));
                            var blocks = new Devmasters.SpeechToText.VoiceToTextFormatter(tt.Terms)
                               .TextWithTimestamps(TimeSpan.FromSeconds(10), true)
                               .Select(t => new Record.Blok() { sekundOdZacatku = (long)t.Start.TotalSeconds, text = t.Text })
                               .ToArray();

                            //TODO opravit casem
                            rec.PrepisAudia = blocks;
                            changed = true;

                        }
                    }
                    if (changed)
                        api.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: threads
                );


        }


        static HlidacStatu.Api.V2.CoreApi.Model.Registration Registration()
        {
            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(Record)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
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
