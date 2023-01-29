using DbPeriodickehoTisku;
using Devmasters.Log;
using HlidacStatu.Api.V2.CoreApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;
using Serilog;
using System;
using System.Collections.Generic;

namespace DbPeriodickehoTisku
{
    class Program
    {
        private const string DatasetNameId = "db-periodickeho-tisku";
        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger($"{DatasetNameId}",
                    Devmasters.Log.Logger.DefaultConfiguration()
                    .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
                    .AddFileLoggerFilePerLevel($"/Data/Logs/{DatasetNameId}/", "slog.txt",
                                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                                      rollingInterval: RollingInterval.Day,
                                      fileSizeLimitBytes: null,
                                      retainedFileCountLimit: 9,
                                      shared: true
                                      )
                    .WriteTo.Console()
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

        public static string urlList = "https://www.mkcr.cz/databaze-periodickeho-tisku-pro-verejnost-cs-978?paging.pageNo={0}#seznam";
        //public static string urlDetail = "https://www.mkcr.cz/databaze-periodickeho-tisku-pro-verejnost-cs-978?do[loadP]=1&item.id={0}";
        //public static string urlDetailSubj = "https://www.mkcr.cz/databaze-periodickeho-tisku-pro-verejnost-cs-978?do[loadV]=1&item.id={0}";

        static HashSet<string> ids = new HashSet<string>();

        static void Main(string[] parameters)
        {
            var args = new Devmasters.Args(parameters);
            logger.Info($"Starting with args {string.Join(' ', parameters)}");

            apiKey = args["/apikey"];
            force = args.Exists("/force");
            int startPage = args.GetNumber("/startpage", 0) ?? 0;

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(record)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
    "Databáze periodického tisku", DatasetNameId,
    "https://www.mkcr.cz/databaze-periodickeho-tisku-pro-verejnost-cs-978",
    "https://github.com/HlidacStatu/Datasety/tree/master/DbPeriodickehoTisku",
    "Databáze periodického tisku pro veřejnost dle zákona č. 46/2000 Sb.",
    genJsonSchema, betaversion: false, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle data evidence", "Evidovano" },
                    { "Podle jména periodika", "Periodikum.keyword" },
                    { "Podle vydavatele", "Vydavatel.Jmeno.keyword" },
                    { "Podle počtu vydání za rok", "Periodicita" },
    },

    defaultOrderBy: "Evidovano desc",

    searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"

<!-- scriban {{ date.now }} --> 
<table class=""table table-hover"">
                        <thead>
                            <tr>
<th>Evidenční číslo</th>
<th>Periodikum</th>
<th>Vydavatel</th>
<th>Zaměření</th>
<th>Evidováno</th>
</tr>
                        </thead>
                        <tbody>
{{ for item in model.Result }}
<tr>
<td ><a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}</a></td>
<td ><a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Periodikum }}</a></td>
<td ><a href='/subjekt/{{ item.Vydavatel.ICO }}'>{{ item.Vydavatel.Jmeno }}</td>
<td >{{ fn_FormatDate item.Evidovano }}</td>
</tr>
{{ end }}

</tbody></table>
" },
    detailTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"

<!-- scriban {{ date.now }} --> 
 {{this.item = model}}
<table class=""table table-hover""><tbody>
<tr><td>Evidenční číslo</td><td >{{ item.Id }}</td></tr>
<tr><td>Název</td><td >{{ item.Periodikum }}</td></tr>
<tr><td>Zaměření</td><td >{{ item.Zamereni }}</td></tr>
<tr><td>Vydavatel</td><td ><a href='/subjekt/{{ item.Vydavatel.ICO }}'>{{ item.Vydavatel.Jmeno }}</td></tr>
<tr><td>Okres a kraj</td><td >{{ item.Okres }} {{ item.Kraj }}</td></tr>
<tr><td>Počet vydání ročně</td><td >{{ item.Periodicita }}</td></tr>
<tr><td>Datum evidence</td><td >{{ fn_FormatDate item.Evidovano }}</td></tr>
<tr><td>Datum přerušení </td><td >{{ fn_FormatDate item.Preruseno }} {{ item.CasNabytiUcinnosti }}</td></tr>
<tr><td>Datum ukončení</td><td >{{ fn_FormatDate item.DatumSchvaleni }}</td></tr>


</table>


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

            bool onTheStartAgain = false;
            int page = startPage;
            do
            {
                string url = string.Format(urlList, page);
                logger.Info("Loading data from {url}", url);
                using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(url))
                {
                    net.Timeout = 180000;
                    net.Tries = 5;
                    net.TimeInMsBetweenTries = 5000;
                    var html = net.GetContent(System.Text.Encoding.UTF8).Text;
                    onTheStartAgain = parsePageList(html);
                }
                page++;

            } while (onTheStartAgain == false);



        }
        static bool parsePageList(string html)
        {
            var xp = new Devmasters.XPath(html);
            var rows = xp.GetNodes("//table[contains(@class,\"evidence\")]/tbody/tr");

            if (rows?.Count > 0)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var rec = new record();
                    rec.EvidencniCislo = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[2]");
                    rec.Id = rec.EvidencniCislo.Replace(" ", "");
                    if (string.IsNullOrEmpty(rec.Id))
                        continue; //fix for record 16582 from https://www.mkcr.cz/databaze-periodickeho-tisku-pro-verejnost-cs-978?paging.pageNo=829#seznam
                    rec.Periodikum = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[3]");
                    Console.Write($"{rec.Periodikum} ");
                    rec.Vydavatel = new record.vydavatel()
                    {
                        Jmeno = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[4]"),
                        ICO = ""
                    };
                    rec.Vydavatel.ICO = findCompany.FindIco(rec.Vydavatel.Jmeno);
                    Console.Write($"{rec.Vydavatel.Jmeno} ({rec.Vydavatel.ICO})");

                    rec.Zamereni = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[5]");
                    rec.Okres = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[6]");
                    rec.Kraj = Devmasters.XPath.Tools.GetNodeText(rows[i], "td[7]");
                    rec.Periodicita = Devmasters.ParseText.ToInt(Devmasters.XPath.Tools.GetNodeText(rows[i], "td[8]")) ?? 0;
                    rec.Evidovano = Devmasters.DT.Util.ToDate(Devmasters.XPath.Tools.GetNodeText(rows[i], "td[9]"));
                    rec.Preruseno = Devmasters.DT.Util.ToDate(Devmasters.XPath.Tools.GetNodeText(rows[i], "td[10]"));
                    rec.Ukonceno = Devmasters.DT.Util.ToDate(Devmasters.XPath.Tools.GetNodeText(rows[i], "td[11]"));

                    System.IO.File.AppendAllText(@"c:\!\periodTisk.csv", 
                        $"{rec.EvidencniCislo}\t{rec.Periodikum}\t{rec.Vydavatel.Jmeno}\t{rec.Vydavatel.ICO}\t{rec.Zamereni}\t{rec.Okres}\t{rec.Kraj}\t{rec.Periodicita}\t{rec.Evidovano:dd.MM.yyyy}\t{rec.Preruseno:dd.MM.yyyy}\t{rec.Ukonceno:dd.MM.yyyy}\n");

                    if (ids.Contains(rec.Id + rec.Periodikum))
                        return true;
                    ids.Add(rec.Id + rec.Periodikum);
                    if (!ds.ItemExists(rec.Id) || force)
                    {
                        ds.AddOrUpdateItem(rec, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        Console.WriteLine(" saved.");
                    }
                    else {
                        Console.WriteLine(" skipped.");

                    }
                }


                return false;
            }
            else
                return true;

        }

    }
}
