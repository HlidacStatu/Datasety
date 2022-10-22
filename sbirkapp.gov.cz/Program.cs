using System;
using System.Globalization;
using System.IO;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

using Devmasters.Log;

using HlidacStatu.Api.V2.CoreApi.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using ObjectsComparer;

using Serilog;

namespace sbirkapp.gov.cz
{
    class Program
    {
        private const string DatasetNameId = "sbirka-pravnich-predpisu";
        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("sbirka-pravnich-predpisu",
                    Devmasters.Log.Logger.DefaultConfiguration()
                    .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
                    .AddFileLoggerFilePerLevel("c:/Data/Logs/sbirka-pravnich-predpisu/", "slog.txt",
                                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                                      rollingInterval: RollingInterval.Day,
                                      fileSizeLimitBytes: null,
                                      retainedFileCountLimit: 9,
                                      shared: true
                                      )
                   );

        static Devmasters.Batch.MultiOutputWriter outputWriter =
    new Devmasters.Batch.MultiOutputWriter(
        Devmasters.Batch.Manager.DefaultOutputWriter,
        new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Debug).OutputWriter
    );

        static Devmasters.Batch.MultiProgressWriter progressWriter =
            new Devmasters.Batch.MultiProgressWriter(
                new Devmasters.Batch.ActionProgressWriter(1.0f, Devmasters.Batch.Manager.DefaultProgressWriter).Write,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Information).ProgressWriter).Write
            );


        //static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitel_flat> ds_flat = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<record> ds = null;
        public static string apiKey = "";
        public static bool force = false;

        public static string urlData = "https://sbirkapp.gov.cz/vyhledavani/vysledek?typ=ozv&ovm=&kod_vusc=&ucinnost_od=&ucinnost_do=&oblast=&zmocneni=&nazev=&znacka=&format_exportu=csv";

        public static string urlTextTemplate = "https://sbirkapp.gov.cz/detail/{0}/text";

        static void Main(string[] parameters)
        {
            var args = new Devmasters.Args(parameters);
            logger.Info($"Starting with args {string.Join(' ', parameters)}");

            apiKey = args["/apikey"];
            force = args.Exists("/force");

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(record)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
    "Sbírky právních předpisů územních samosprávných celků a některých správních úřadů", DatasetNameId,
    "https://sbirkapp.gov.cz",
    "https://github.com/HlidacStatu/Datasety/tree/master/sbirkapp.gov.cz",
    "Sbírky právních předpisů územních samosprávných celků a některých správních úřadů slouží k vyhlašování právních předpisů územních samosprávných celků a zákonem stanovených správních úřadů a ke zveřejňování dalších zákonem vymezených aktů. ",
    genJsonSchema, betaversion: false, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle účinnosti", "DatumNabytiUcinnosti" },
                    { "Podle datumu vyvěšení na úřední desce", "DatumVyveseniNaUredniDesce" },
                    { "Podle datumu vyvěšení na úřední desce", "Vydavatel.keyword" },
                    { "Podle oblasti právní úpravy\t", "OblastPravniUpravy.keyword" },
    },
    /*Podle účinnosti|DatumNabytiUcinnosti
Podle datumu vyvěšení na úřední desce|DatumVyveseniNaUredniDesce
Podle úřadu|Vydavatel.keyword
*/

    defaultOrderBy: "datum_zapis desc",

    searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"

<!-- scriban {{ date.now }} --> 
<table class=""table table-hover"">
                        <thead>
                            <tr>
<th>Detail</th>
<th>Název</th>
<th>Druh</th>
<th>Číslo</th>
<th>Vydavatel</th>
<th>Účinnost</th>
</tr>
                        </thead>
                        <tbody>
{{ for item in model.Result }}
<tr>
<td ><a href=""{{ fn_DatasetItemUrl item.Id }}"">Detail</a></td>
<td >{{ item.Nazev }}</td>
<td >{{ item.Druh }}</td>
<td >{{ item.Cislo }}</td>
<td ><a href='/subjekt/{{ item.ICO }}'>{{ item.Vydavatel }}</td>
<td >účinné od {{ fn_FormatDate item.DatumNabytiUcinnosti }}</td>
</tr>
{{ end }}

</tbody></table>
" },
    detailTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"

<!-- scriban {{ date.now }} --> 
 {{this.item = model}}
<table class=""table table-hover""><tbody>
<tr><td>ID záznamu</td><td >{{ item.Id }}</td></tr>
<tr><td>Název</td><td >{{ item.Nazev }}</td></tr>
<tr><td>Druh</td><td >{{ item.Druh }}</td></tr>
<tr><td>Číslo</td><td >{{ item.Cislo }}</td></tr>
<tr><td>Vydavatel předpisu</td><td ><a href='/subjekt/{{ item.ICO }}'>{{ item.Vydavatel }}</td></tr>
<tr><td>Kraj vydavatele</td><td >{{ item.KrajVydavatele }}</td></tr>
<tr><td>Datum zveřejnění</td><td >{{ fn_FormatDate item.DatumZverejneni }}</td></tr>
<tr><td>Datum nabytí účinnosti</td><td >{{ fn_FormatDate item.DatumNabytiUcinnosti }} {{ item.CasNabytiUcinnosti }}</td></tr>
<tr><td>Datum schválení</td><td >{{ fn_FormatDate item.DatumSchvaleni }}</td></tr>

{{ if fn_IsNullOrEmpty(item.DatumVyveseniNaUredniDesce)==false }}
<tr><td>Datum vyvěšení na úřední desce</td><td >{{ fn_FormatDate item.DatumVyveseniNaUredniDesce }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.OblastPravniUpravy)==false }}
<tr><td>Způsob zveřejnění</td><td >{{ item.ZpusobZverejneni }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.DatumVyveseniNaUredniDesce)==false }}
<tr><td>Oblast právní úpravy</td><td >{{ item.OblastPravniUpravy }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.PravniZmocneni)==false }}
<tr><td>Právní zmocnění</td><td >{{ item.PravniZmocneni }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.Verze)==false }}
<tr><td>Verze</td><td >{{ fn_FormatNumber item.Verze }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.Usneseni)==false }}
<tr><td>Usnesení</td><td >{{ item.Usneseni }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.Kraj)==false }}
<tr><td>Kraj</td><td >{{ item.Kraj }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.PocetClenuZastupitelstva)==false }}
<tr><td>Počet členú zastupitelstva</td><td >{{ item.PocetClenuZastupitelstva }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.RokVNemzSeKonajiVolby)==false }}
<tr><td>Rok, v němž se konají volby</td><td >{{ item.RokVNemzSeKonajiVolby }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.UzemiNaKteremSeChraneneUzemiNachazi)==false }}
<tr><td>Území, na kterém se chráněné území nachází</td><td >{{ item.UzemiNaKteremSeChraneneUzemiNachazi }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.UzemiNaKteremJeStavNebezpeciVyhlasen)==false }}
<tr><td>Území, na kterém je stav nebezpečí vyhlášen</td><td >{{ item.UzemiNaKteremJeStavNebezpeciVyhlasen }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.SpisovaZnackaUstavnihoSoudu)==false }}
<tr><td>Spisová značka Ústavního soudu</td><td >{{ item.SpisovaZnackaUstavnihoSoudu }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.SpisovaZnacka)==false }}
<tr><td>Spisová značka</td><td >{{ item.SpisovaZnacka }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.TypSmlouvy)==false }}
<tr><td>Typ smlouvy</td><td >{{ item.TypSmlouvy }}</td></tr>
{{end}}

{{ if fn_IsNullOrEmpty(item.TypRozhodnuti)==false }}
<tr><td>Typ rozhodnutí</td><td >{{ item.TypRozhodnuti }}</td></tr>
{{end}}
</table>
<h2>Text předpisu</i></h2>
      <pre style=""font-size:90%;background:none;line-height:1.6em;"">
        {{ fn_HighlightText highlightingData item.TextPredpisu.DocumentPlainText ""TextPredpisu.DocumentPlainText"" | string.replace '\n' '\n\n' }}                                                                                                                                                                                                                                                                  
      </pre>
<a href='{{ item.TextPredpisu.DocumentUrl }}'>Originál dokumentu</a>


<br/><br/>

  {{for pp in item.PrilohyPredpisu }}
  <div class=""panel panel-default"">                                              
    <div class=""panel-heading"">
      <h3 class=""panel-title"">Příloha <i>{{ pp.DocumentName }}</i> </h3>
    </div>
    <div class=""panel-body"">                                                                       
      <pre style=""font-size:90%;background:none;line-height:1.6em;"">
        {{ fn_HighlightText highlightingData pp.DocumentPlainText ""PrilohyPredpisu.DocumentPlainText"" | string.replace '\n' '\n\n' }}                                                                                                                                                                                                                                                                  
      </pre>
    </div>
  </div>
{{end}}




" }

    );


            try
            {
                if (args.Exists("/new"))
                {
                    //throw new NotImplementedException();

                    Configuration configuration = new Configuration();
                    configuration.AddDefaultHeader("Authorization", apiKey);
                    HlidacStatu.Api.V2.CoreApi.DatasetyApi datasetyApi = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(configuration);
                    datasetyApi.ApiV2DatasetyDelete(reg.DatasetId);
                }
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<record>.OpenDataset(apiKey, DatasetNameId);

            }
            catch (HlidacStatu.Api.V2.CoreApi.Client.ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<record>.CreateDataset(apiKey, reg);

            }
            catch (Exception e)
            {
                throw;
            }
            var wc = new System.Net.WebClient();

            var csvData = System.Diagnostics.Debugger.IsAttached
                ? System.IO.File.ReadAllText("export.debug.csv")
                : wc.DownloadString(urlData);


            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                ExceptionMessagesContainRawData = true,
                BadDataFound = context => Console.WriteLine(context.RawRecord)

            };
            record[] records = null;
            using (var reader = new StringReader(csvData))
            using (var csv = new CsvReader(reader, config))
            {
                records = csv.GetRecords<record>().ToArray();
            }

            var comparer = new Comparer<record>();
            comparer.IgnoreMember("DocumentPlainText");

            Devmasters.Batch.Manager.DoActionForAll<record>(records,
                rec =>
             {
                 //parse detail on web to get prilohy
                 var html = Devmasters.Net.HttpClient.Simple.GetAsync($"https://sbirkapp.gov.cz/detail/{rec.Id}").Result;

                 var xp = new Devmasters.XPath(html);
                 var nodes = xp.GetNodes("//h3[contains(text(),'Přílohy')]/following::ul[1]/li/a");

                 if (nodes?.Count > 0)
                 {
                     rec.PrilohyPredpisu = nodes
                            .Select(m => new record.doc() 
                            { 
                                DocumentUrl = "https://sbirkapp.gov.cz"+ m.GetAttributeValue("href",""),
                                DocumentName = Devmasters.TextUtil.NormalizeToBlockText( m.InnerText)
                            }
                            )
                            .ToArray();
                 }



                 rec.TextPredpisu = new record.doc() { DocumentUrl= string.Format(urlTextTemplate, rec.Id), DocumentName ="Obsah předpisu" };


                 if (!ds.ItemExists(rec.Id) || force)
                 {
                     ds.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                 }
                 else
                 {
                     //check change
                     bool same = true;
                     var old = ds.GetItem(rec.Id);
                     if (old != null)
                     {
                         same = comparer.Compare(rec, old);

                     }
                     else
                         same = false;

                     if (same == false)
                     {
                         ds.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                     }
                 }

                 return new Devmasters.Batch.ActionOutputData();
             }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
         true, //!System.Diagnostics.Debugger.IsAttached,
         maxDegreeOfParallelism: 4);

        }

        private static void DownloadFile(string name)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");

            }
            catch (Exception e1)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000);
                    wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");
                }
                catch (Exception e2)
                {

                    Console.WriteLine($"Cannot download https://dataor.justice.cz/api/file/{name}.xml   ex:" + e2.Message);
                }
            }
        }
    }
}
