using Devmasters.Log;

using HlidacStatu.Api.V2.CoreApi.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rozhodnuti_UOHS
{
    class Program
    {
        static bool debug = false;

        static string datasetId = "rozhodnuti-uohs"; //zvol vlastni unikatni jmeno

        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<UOHSData> ds = null;
        static HttpClient httpClient = null;

        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("RozhodnutiUOHS",
            Devmasters.Log.Logger.DefaultConfiguration()
            .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
            .AddFileLoggerFilePerLevel("/Data/Logs/RozhodnutiUOHS/", "slog.txt",
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

        static AppSettings settings = null;

        const int ExitCodeSuccess = 0;
        const int ExitCodeFailure = 1;
        const int ExitCodeInvalidArguments = 2;

        static async Task<int> Main(string[] _args)
        {
            Console.WriteLine("Rozhodnuti-UOHS \n");
            try
            {
                logger.Info("Starting with params " + RunOptions.ToLogString(_args));

                RunOptions options;
                try
                {
                    options = RunOptions.Parse(_args);
                }
                catch (RunOptionsException e)
                {
                    logger.Error("Invalid command line: {Error}", e.Message);
                    Console.WriteLine(e.Message);
                    Console.WriteLine(RunOptions.Usage);
                    return ExitCodeInvalidArguments;
                }

                settings = AppSettings.Load(Path.Combine(AppContext.BaseDirectory, AppSettings.FileName));
                debug = options.Debug;

                InitDS(options.RecreateDataset, options.ApiKey);

                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", options.ApiKey);

                var selection = await SelectDecisionIdsAsync(options);
                logger.Info("{DecisionCount} decision IDs selected for download in {SelectionMode} mode",
                    selection.Ids.Count, selection.Mode);

                if (options.DryRun)
                {
                    logger.Info("Dry run, nothing is downloaded. Selected decision IDs: {DecisionIds}", selection.Ids);
                    return ExitCodeSuccess;
                }

                ParsePages(selection.Ids); //stahnuti, parsovani dat z UOHS a vlozeni do Datasetu
                logger.Info("Finished, {DecisionCount} decision IDs processed", selection.Ids.Count);
                return ExitCodeSuccess;
            }
            catch (OperationCanceledException)
            {
                logger.Warning("Cancelled by user");
                return ExitCodeFailure;
            }
            catch (Exception e)
            {
                logger.Fatal("Run failed", e);
                return ExitCodeFailure;
            }
            finally
            {
                logger.CloseAndFlush();
            }
        }

        /// <summary>
        /// Chooses decision IDs to download; Ctrl+C cancels the selection (discovery can take minutes).
        /// Afterwards Ctrl+C terminates the process as usual.
        /// </summary>
        static async Task<DecisionIdSelection> SelectDecisionIdsAsync(RunOptions options)
        {
            using var cancellation = new CancellationTokenSource();
            ConsoleCancelEventHandler onCancel = (_, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };

            Console.CancelKeyPress += onCancel;
            try
            {
                var selection = await new DecisionIdSelector(settings.Import).SelectAsync(
                    options,
                    () => ds.Search("", 1).Total == 0,
                    FindNewDecisionIdsAsync,
                    cancellation.Token);

                if (selection.Mode == SelectionMode.IdRange && selection.Ids.Count > 0)
                    logger.Info("Range mode, decision IDs {FirstId} - {LastId}", selection.Ids[0], selection.Ids[^1]);
                if (selection.Mode == SelectionMode.EmptyDataset)
                    logger.Info("Dataset {DatasetId} is empty, processing decision IDs 0 - {LastId}", datasetId, selection.Ids.Count - 1);

                return selection;
            }
            finally
            {
                Console.CancelKeyPress -= onCancel;
            }
        }

        /// <summary>
        /// Scans the UOHS listing of all areas from the newest decisions plus the recent ID window
        /// and returns IDs not stored in the dataset yet.
        /// </summary>
        static async Task<IReadOnlyList<int>> FindNewDecisionIdsAsync(CancellationToken cancellationToken)
        {
            using var listingClient = UohsListingClient.Create(settings.Uohs, logger);
            var finder = new NewDecisionFinder(
                settings.Uohs.StopAfterKnownListingPages, settings.Uohs.MaxListingPages,
                settings.Uohs.RecentIdWindowSize, settings.Uohs.ListingRequestDelay);

            var result = await finder.FindAsync(
                listingClient.GetPageAsync,
                id => ds.Api.ApiV2DatasetyDatasetItemExists(ds.DatasetId, id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                scan => logger.Info("UOHS listing page {PageNumber}: {IdsOnPage} decisions, {NewIdsOnPage} missing in dataset, {TotalNewIds} missing in total",
                    scan.PageNumber, scan.IdsOnPage, scan.NewIdsOnPage, scan.TotalNewIds),
                cancellationToken);

            logger.Info("UOHS discovery finished: listing scan {StopReason} after {PagesScanned} pages, {ListingIdCount} listed decisions missing in dataset, "
                + "{WindowIdCount} more IDs missing in the recent window of {WindowSize} IDs up to {MaxListedId}",
                result.StopReason, result.PagesScanned, result.ListingIds.Count,
                result.RecentWindowIds.Count, settings.Uohs.RecentIdWindowSize, result.MaxListedId);

            if (result.StopReason is not DiscoveryStopReason.KnownPagesThresholdReached)
                logger.Warning("UOHS listing scan ended by {StopReason} before reaching already stored decisions; older missing decisions may exist, download them with /start= /num=",
                    result.StopReason);

            return result.NewIds;
        }

        public static void ParsePages(IEnumerable<int> decisionIds)
        {

            Devmasters.Batch.Manager.DoActionForAll<int>(decisionIds,
            //jedeme v 2 threadech, bud ohleduplny a nedavej vice
                (i) =>
                {
                    string url = "";
                    try
                    {

                        //stahnutí HTML stránky s rozhodnutím UOHS.
                        //rozhodnutí jsou na samostatnych stránkach, s jednoduchym URL, kde cislo stranky s rozhodnutim postupně roste.
                        // k 1.9.2018 ma posledni rozhodnuti cislo asi 15500
                        string html = "";
                        url = string.Format(System.Globalization.CultureInfo.InvariantCulture, settings.Uohs.DetailUrlTemplate, i);
                        
                        //stahnuti HTML
                        System.Net.WebClient wc = new System.Net.WebClient();
                        wc.Encoding = System.Text.Encoding.UTF8;
                        html = wc.DownloadString(url);

                        //prevedeni do XHTML pomoci HTMLAgilityPacku.
                        //XPath je trida a sada funkci pro jednodusi XPath parsovani
                        Devmasters.XPath page = new Devmasters.XPath(html);

                        //vsechna ziskavana data jsou ziskana pomoci XPATH


                        //stranka neexistuje, tak ji preskocime 
                        if (page.GetNodeText("//head/title")?.Contains("stránka neexistuje") == true)
                            return new Devmasters.Batch.ActionOutputData();

                        logger.Debug($"parsing {url}");

                        //do item davam postupně získané údaje
                        var item = new UOHSData();
                        item.Url = url;
                        item.Id = i.ToString();

                        //žádný obsah není mimo tento DIV, tak si ho sem dam, abych tento retezec nemusel porad opakovat
                        var root = "//div[@id='content']";

                        //parsování pomocí XPath.
                        item.Cj = page.GetNodeText(root + "//div/h1/strong[1]")?.Replace("Rozhodnutí: ", "");
                        item.SpisovaZnacka = page.GetNodeText(root + "//div/h1/strong[2]")?.Replace("Rozhodnutí: ", "");
                        item.SoudniRozhodnuti = page.GetNodeText(root + "//div//h1/following-sibling::h2[1]");


                        item.Instance = page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Instance')]/parent::tr/td");

                        item.Vec = page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Věc')]/parent::tr/td");

                        var ucastniciNode = page.GetNodes(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Účastníci')]/parent::tr/td/ol/li");
                        List<UOHSData.Ucastnik> ucastnici = new List<UOHSData.Ucastnik>();
                        if (ucastniciNode != null)
                        {
                            foreach (var node in ucastniciNode)
                            {
                                var firmaJmeno = System.Net.WebUtility.HtmlDecode(node.InnerText); //konverze HTML entity to UTF-8;  &eacute; -> é


                                //dohledat ICO
                                var ico = httpClient.GetAsync("https://api.hlidacstatu.cz/api/v2/firmy/" + System.Net.WebUtility.UrlEncode(firmaJmeno))
                                                .Result.Content
                                                .ReadAsStringAsync().Result;
                                try
                                {
                                    var icoRes = Newtonsoft.Json.Linq.JObject.Parse(ico);
                                    if (icoRes["ico"] == null)
                                        ucastnici.Add(new UOHSData.Ucastnik() { Jmeno = firmaJmeno });
                                    else
                                        ucastnici.Add(new UOHSData.Ucastnik()
                                        {
                                            Jmeno = firmaJmeno,
                                            ICO = icoRes["ico"].Value<string>()
                                        });

                                }
                                catch (Exception)
                                {
                                    ucastnici.Add(new UOHSData.Ucastnik() { Jmeno = firmaJmeno });
                                }

                            }
                        }
                        item.Ucastnici = ucastnici.ToArray();

                        item.Typ_spravniho_rizeni = page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Typ správního řízení')]/parent::tr/td");
                        item.Typ_rozhodnuti = page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Typ rozhodnutí')]/parent::tr/td");
                        item.Rok = page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Rok')]/parent::tr/td");

                        item.PravniMoc = ToDateTimeFromCZ(
                            page.GetNodeText(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Datum nabytí právní moci')]/parent::tr/td")
                            );

                        var souvis_urls = page.GetNodes(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Související rozhodnutí')]/parent::tr/td/a");
                        if (souvis_urls != null)
                        {
                            item.SouvisejiciUrl = souvis_urls
                                .Select(m => UohsLinks.ToAbsoluteUrl(settings.Uohs.LinkBaseUrl, m.Attributes["href"]?.Value))
                                .Where(m => m != null)
                                .ToArray();
                        }


                        item.Rozhodnuti = new UOHSData.Dokument();

                        var documents = page.GetNodes(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Dokumenty')]/parent::tr/td/a");


                        item.Rozhodnuti.Url = UohsLinks.ToAbsoluteUrl(settings.Uohs.LinkBaseUrl,
                            page.GetNode(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Dokumenty')]/parent::tr/td/a")
                                ?.Attributes["href"]?.Value);

                        item.Rozhodnuti.PlainText = page.GetNode("//div[@id='content']//div[@class='res_text']")?.InnerText ?? "";


                        //parsovani hotovo, jdu ulozit zaznam do Datasetu
                        logger.Debug($"adding item {item.Id} - {item.Url}");
                        
                        ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                    }
                    catch (System.Net.WebException e) when ((e.Response as System.Net.HttpWebResponse)?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        //gap in UOHS numbering (ID without a published decision), not an error
                        logger.Info("Decision {DecisionId} is not published on UOHS: {Url}", i, url);
                    }
                    catch (Exception e)
                    {
                        logger.Error(url, e);
                    }

                    return new Devmasters.Batch.ActionOutputData();
                },
                    outputWriter.OutputWriter, progressWriter.ProgressWriter,
                    !System.Diagnostics.Debugger.IsAttached,
                    maxDegreeOfParallelism: settings.Import.MaxDegreeOfParallelism
              );


        }


        public static DateTime? ToDateTimeFromCZ(string value)
        {
            return ToDateTime(value,
                "d.M.yyyy", "d. M. yyyy",
                "dd.MM.yyyy", "dd. MM. yyyy",
                "dd.MM.yy", "dd. MM. yy",
                "d.M.yy", "d. M. yy"
                );
        }

        public static DateTime? ToDateTime(string value, params string[] formats)
        {
            foreach (var f in formats)
            {
                var dt = ToDateTime(value, f);
                if (dt.HasValue)
                    return dt;
            }
            return null;
        }
        public static DateTime? ToDateTime(string value, string format)
        {
            DateTime tmp;
            if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                return new DateTime?(tmp);
            else
                return null;
        }



        static void InitDS(bool recreate, string apiKey)
        {

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(UOHSData)).ToString();


            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
    "Rozhodnutí UOHS", datasetId,
    "http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/",
    "https://github.com/HlidacStatu/Datasety/tree/master/Rozhodnuti-UOHS",
    "Sbírka rozhodnutí Úřadu pro ochranu hospodářské soutěže od roku 1999 v oblastech hospodářská soutěž, veřejné zakázky, veřejná podpora a významná tržní síla.",
    genJsonSchema, betaversion: false, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle datumu právní moci", "PravniMoc" },
                    { "Podle roku", "Rok" },
                    { "Podle IČ subjektu", "ico" },
    },
    defaultOrderBy: "PravniMoc desc",

    searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() {
        Body = @"
<!-- scriban {{ date.now }} -->
  <table class=""table table-hover"">        
    <thead>
      <tr>                    
        <th>ČJ
        </th>
        <th>Instance
        </th>
        <th>Nabytí právní moci
        </th>
        <th>Účastníci řízení
        </th>
        <th>Věc
        </th>
        <th>Typ správního řízení
        </th>
      </tr>
    </thead>
    <tbody>
      {{ for item in model.Result }}                 
      <tr>
        <td style=""white-space: nowrap;"">                              
          <a href=""{{fn_DatasetItemUrl item.Id}}"">{{ fn_ShortenText item.Cj 20 }}
          </a>
        </td>
        <td style=""white-space: nowrap;"">
          {{item.Instance}}                
        </td>
        <td>
          {{fn_FormatDate item.PravniMoc  ""dd.MM.yyyy""}}        
        </td>
        <td>
          {{ for u in item.Ucastnici}}
          {{ if (fn_IsNullOrEmpty u.ICO)}}
          {{ fn_NormalizeText u.Jmeno }}
          {{else}}                                      
          <a href=""https://www.hlidacstatu.cz/subjekt/{{u.ICO}}"">{{fn_NormalizeText u.Jmeno}}
          </a>
          <br />
            {{end #if }}
            {{ end #for }}                    
          </td>
          <td>
            {{fn_ShortenText item.Vec 200}}                          
          </td>
          <td>{{item.Typ_spravniho_rizeni}}          
          </td>
        </tr>
        {{end }}
      </tbody>
    </table>
            " },
    detailTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"
<!-- scriban {{ date.now }} -->
  {{this.item = model}}  
  <table class=""table table-hover"">
    <tbody>                  
      <tr>
        <td>Číslo jednací        
        </td>
        <td>{{item.Cj}}        
        </td>
      </tr>
      <tr>
        <td>Instance        
        </td>
        <td>{{item.Instance}}        
        </td>
      </tr>
      <tr>
        <td>Věc        
        </td>
        <td>{{ fn_HighlightText highlightingData item.Vec ""Vec"" }}        
        </td>
      </tr>
      <tr>
        <td>Účastníci        
        </td>
        <td>
          {{for u in item.Ucastnici}}
          {{ if (fn_IsNullOrEmpty u.ICO) }}
          {{fn_NormalizeText u.Jmeno}}
          {{else}}                                      
          <a href=""https://www.hlidacstatu.cz/subjekt/{{u.ICO}}"">{{fn_NormalizeText u.Jmeno}}
          </a>
          <br />
            {{end}}
            {{end}}                
          </td>
        </tr>
        <tr>          
          <td>Typ řízení
          </td>
          <td>{{item.Typ_spravniho_rizeni}}
          </td>
        </tr>
        <tr>          
          <td>Typ rozhodnutí
          </td>
          <td>{{item.Typ_spravniho_rizeni}}
          </td>
        </tr>
        <tr>          
          <td>Nabytí právní moci
          </td>
          <td>{{fn_FormatDate item.PravniMoc ""dd.MM.yyyy""}}
          </td>
        </tr>
        <tr>          
          <td>Související řízení
          </td>
          <td>
            {{for u in item.SouvisejiciUrl}}                                    
            <a href=""{{u}}"">{{u}}
            </a>
            <br />
              {{end}}            
            </td>
          </tr>
          <tr>                
            <td>Zdroj na UOHS            
            </td>
            <td>              
              <a href=""{{item.Url}}"" target=""_blank"">{{item.Url}}
              </a>
            </td>
          </tr>
          {{ if item.Rozhodnuti}}                
          <tr>            
            <td>Rozhodnutí
            </td>
            <td>
              <pre>
                {{ fn_HighlightText highlightingData item.Rozhodnuti.PlainText ""Rozhodnuti.PlainText"" }}
              </pre>
            </td>
          </tr>
          {{end}}
        </tbody>
      </table>

" }

    );


            try
            {
                if (recreate)
                {
                    Configuration configuration = new Configuration();
                    configuration.AddDefaultHeader("Authorization", apiKey);
                    if (debug)
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        configuration.BasePath = "https://local.hlidacstatu.cz";
                    }
                    HlidacStatu.Api.V2.CoreApi.DatasetyApi datasetyApi = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(configuration);
                    datasetyApi.ApiV2DatasetyDelete(reg.DatasetId);
                }

                if (debug)
                {

                    System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                    ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<UOHSData>.OpenDataset(apiKey, datasetId, "https://local.hlidacstatu.cz");
                }
                else 
                    ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<UOHSData>.OpenDataset(apiKey, datasetId);
            }
            catch (HlidacStatu.Api.V2.CoreApi.Client.ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<UOHSData>.CreateDataset(apiKey, reg);

            }
            catch (Exception e)
            {
                throw;
            }

        }
    }
}
