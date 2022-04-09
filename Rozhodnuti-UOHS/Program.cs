using HlidacStatu.Api.V2.CoreApi.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Rozhodnuti_UOHS
{
    class Program
    {
        static bool debug = false;

        public static Devmasters.Logging.Logger logger = new Devmasters.Logging.Logger("Rozhodnuti-UOHS");
        static string datasetId = "rozhodnuti-uohs"; //zvol vlastni unikatni jmeno
        static Devmasters.Batch.MultiOutputWriter outputWriter =
new Devmasters.Batch.MultiOutputWriter(
    Devmasters.Batch.Manager.DefaultOutputWriter,
    new Devmasters.Batch.LoggerWriter(logger, Devmasters.Logging.PriorityLevel.Debug).OutputWriter
);
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<UOHSData> ds = null;
        static HttpClient httpClient = null;

        static Devmasters.Batch.MultiProgressWriter progressWriter =
            new Devmasters.Batch.MultiProgressWriter(
                new Devmasters.Batch.ActionProgressWriter(1.0f, Devmasters.Batch.Manager.DefaultProgressWriter).Write,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Logging.PriorityLevel.Information).ProgressWriter).Write
            );
        static void Main(string[] _args)
        {
            Console.WriteLine("Rozhodnuti-UOHS \n");
            Devmasters.Args args = new Devmasters.Args(_args, new[] { "/apiKey" });
            logger.Info("Starting with params" + string.Join(" ", args.Arguments));

            if (args.MandatoryPresent() == false)
            {
                Console.WriteLine("/apikey=xxx   [/new] [/num=]");
                return;            
            }
            debug = args.Exists("/debug");

            InitDS(args.Exists("/new"), args["/apiKey"]);

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", args["/apiKey"]);


            //check last Rozhodnuti
            var res = ds.Search("", 1, "DbCreated", true);
            int lastId = 0;
            if (res.Total > 0)
                lastId = Convert.ToInt32(res.Results.First().Id);

            int num = args.GetNumber("/num", 1500).Value;
            if (lastId == 0)
                num = 90000;


            ParsePages(datasetId, lastId, num); //stahnuti, parsovani dat z UOHS a vlozeni do Datasetu
        }

        public static void ParsePages(string datasetId, int startFrom = 10000, int count = 600)
        {

            Devmasters.Batch.Manager.DoActionForAll<int>(Enumerable.Range(startFrom, count),
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
                        url = $"http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{i}.html";
                        
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
                        item.Cj = "ÚOHS-" + page.GetNodeText(root + "//div/h1").Replace("Rozhodnutí: ", "");
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
                                var ico = httpClient.GetAsync("https://www.hlidacstatu.cz/api/v2/firmy/" + System.Net.WebUtility.UrlEncode(firmaJmeno))
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
                                .Select(m => m.Attributes["href"]?.Value)
                                .Where(m => m != null)
                                .Select(u => "http://www.uohs.cz" + u)
                                .ToArray();
                        }


                        item.Rozhodnuti = new UOHSData.Dokument();

                        var documents = page.GetNodes(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Dokumenty')]/parent::tr/td/a");


                        item.Rozhodnuti.Url = page.GetNode(root + "//table[@id='resolution_detail']//tr//th[contains(text(),'Dokumenty')]/parent::tr/td/a")
                            ?.Attributes["href"]?.Value;
                        if (!string.IsNullOrEmpty(item.Rozhodnuti.Url))
                            item.Rozhodnuti.Url = "http://www.uohs.cz" + item.SouvisejiciUrl;

                        item.Rozhodnuti.PlainText = page.GetNode("//div[@id='content']//div[@class='res_text']")?.InnerText ?? "";


                        //parsovani hotovo, jdu ulozit zaznam do Datasetu
                        logger.Debug($"adding item {item.Id} - {item.Url}");
                        
                        ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                    }
                    catch (Exception e)
                    {
                        logger.Error(url, e);
                    }

                    return new Devmasters.Batch.ActionOutputData();
                },
                    outputWriter.OutputWriter, progressWriter.ProgressWriter,
                    !System.Diagnostics.Debugger.IsAttached
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
    "Skuteční majitelé firem", datasetId,
    "https://esm.justice.cz/",
    "https://github.com/HlidacStatu/Datasety/tree/master/Rozhodnuti-UOHS",
    "Evidence skutečných majitelů firem podle zákona č. 37/2021 Sb.",
    genJsonSchema, betaversion: true, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle datumu zápisu", "datum_zapis" },
                    { "Podle IČ subjektu", "ico" },
    },
    defaultOrderBy: "datum_zapis desc",

    searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"
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
