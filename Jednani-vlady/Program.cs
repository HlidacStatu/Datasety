using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json;
using HlidacStatu.Api.V2.CoreApi.Client;
using Newtonsoft.Json.Linq;
using Devmasters.Log;
using Serilog;

namespace Jednani_vlady
{
    class Program
    {
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<jednani> dsc;
        public static Dictionary<string, string> args = new Dictionary<string, string>();
        public static string apiKey = "";

        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("deMinimis",
            Devmasters.Log.Logger.DefaultConfiguration()
            .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
            .AddFileLoggerFilePerLevel("/Data/Logs/deMinimis/", "slog.txt",
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
                new Devmasters.Batch.ActionProgressWriter(1.0f, Devmasters.Batch.Manager.DefaultProgressWriter).Write,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Information).ProgressWriter).Write
            );

        static void Main(string[] arguments)
        {

            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            if (args.ContainsKey("/?") || args.ContainsKey("/h") || args.ContainsKey("/apikey") == false)
            {
                Console.WriteLine("Jednani vlady downloader");
                Console.WriteLine("[/h] [/from=yyyy] /apikey=xxxyyy");
                return;
            }

            if (args.ContainsKey("/debug"))
                Parse.parallel = false;

            apiKey = args["/apikey"];

            int? from = null;
            if (args.ContainsKey("/from"))
                from = int.Parse(args["/from"]);

            //create dataset
            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(jednani)).ToString();
            HlidacStatu.Api.V2.CoreApi.Model.Registration dsDef = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
                "Jednání vlády ČR", Parse.datasetname, "https://apps.odok.cz/zvlady", "Databáze \"Jednání vlády\" zobrazuje a umožňuje prohledávat veřejnosti programy jednání vlády, záznamy, usnesení a uveřejňované materiály pro jednání vlády, nepodléhají-li režimu utajení.",
                "https://github.com/HlidacStatu/Datasety/tree/master/jednani-vlady", genJsonSchema,
                "michal@michalblaha.cz", DateTime.Now, false, false,false,
                new HlidacStatu.Api.V2.Dataset.ClassicTemplate.ClassicDetailTemplate() { Body= @"
<!-- scriban {{ date.now }} --> 
<table class='table table-hover'>
                        <thead>
                            <tr>
<th style='min-width:120px'>Detail</th>
<th style='min-width:120px'>Datum jednání</th>
<th>Obsah bodu</th>
</tr>
</thead>
<tbody>
{{ for item in model.Result }}
<tr>
<td ><a href='{{ fn_DatasetItemUrl item.Id }}'>{{ item.bod }}</a></td>
<td>{{ fn_FormatDate item.datum 'dd. MM. yyyy' }}</td>
<td >{{ item.vec }}</td>
</tr>
{{ end }}

</tbody></table>
" },
                new HlidacStatu.Api.V2.Dataset.ClassicTemplate.ClassicDetailTemplate() { Body = @"
{{this.item = model}}

<table class=""table table-hover""><tbody>

<tr><td>Datum jednání</td><td >{{ item.datum }}</td></tr>
<tr><td>Bod jednání</td><td >{{ item.bod }}</a></td></tr>

{{ if !(fn_IsNullOrEmpty item.cisloJednaci)  }}
   <tr><td>Číslo jednací</td><td >{{ item.cisloJednaci }}</td></tr>
{{end }}

<tr><td>Obsah projednáváného bodu</td><td >{{ item.vec }}</td></tr>


{{ if item.dokumenty.size > 0  }}

   <tr><td>Projednávané dokumenty</td><td >
<ul>
{{ for doc in item.dokumenty }}

   <li> 
      {{ doc.jmeno }} - 
      {{fn_LinkTextDocumentWithHighlighting doc ""jednani-vlady"" item.Id ""Obsah dokumentu"" highlightingData }}
   </li>

{{ end }}
</ul>
<div class=""text-muted small"">Vláda uveřejňuje dokumenty obvykle ve dvou formátech (DOC a PDF). Pro úplnost zpracováváme obě verze souboru, i když jsou obvykle shodné</div>
</td></tr>


{{end }}

{{ if item.souvisejici.size > 0  }}

   <tr><td>Související usnesení a dokumenty</td><td >
<ul>
{{ for doc in item.souvisejici }}

   <li> 

      Toto usnesení {{ doc.zmena }} 
      {{ if fn_IsNullOrEmpty doc.usneseniCislo }}
           {{ doc.usneseni }}
      {{ else }}
          <a href=""https://www.hlidacstatu.cz/data/Hledat/jednani-vlady?Q=cisloJednaci.keyword:{{ string.replace doc.usneseniCislo ""/20"" ""%2F"" }}"">
          {{ doc.usneseni }}</a>
      {{ end }}
   </li>

{{ end }}
</ul>
</td></tr>


{{end }}


</table>

" }
                , null, new string[,] { { "Datum jednání", "datum" } }

                );

            try
            {
                dsc = HlidacStatu.Api.V2.Dataset.Typed.Dataset<jednani>.OpenDataset(
                apiKey, Parse.datasetname
                    );

            }
            catch (ApiException e)
            {
                //api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.CreateDataset(apikey, Registration());

            }
            catch (Exception e)
            {
                logger.Error("open dataset error", e);
                throw;
            }


            //Parse.ParseUsneseni(new DateTime(2019,8,26), "624");
            //var js = Parse.ParseAgenda("2020-03-16");
            //IEnumerable<jednani> js = new jednani[] { };
            //js = Parse.ParseAgenda("2020-02-17");
            ///var ids = js.Select(m => m.Id).Distinct().ToArray();
            //if (ids.Count() != js.Count())
            //{
            //    System.Diagnostics.Debugger.Break();
            //}

            Parse.DownloadAllData(dsc,from);
        }


    }
}
