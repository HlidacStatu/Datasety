using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Dataset_RozhodnutiUOHS
{
    class Program
    {
        static void Main(string[] args)
        {
            string datasetId = "rozhodnuti-uohs"; //zvol vlastni unikatni jmeno

            if (args.Count() > 1) //predany token a URL v parametrech volani? Pouzij je
            {
                apiToken = "Token " + args[0];
                apiRoot = args[1];
            }
            if (args.Contains("update"))
            {

                //find last ID
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);
                string jsonResult = httpClient.GetAsync(apiRoot + "/Datasetsearch/" + datasetId+"?q=*&sort=PravniMoc+desc")
                            .Result.Content
                            .ReadAsStringAsync().Result;
                var result = JObject.Parse(jsonResult);
                if (result["error"] == null)
                {
                    int? lastId = result["results"]?.First()?["Id"]?.Value<int>();
                    lastId = lastId.HasValue ? lastId.Value-200 : 1;
                    //vratim se zpet o 200 cisel zpet a dalsich 1500 pro jistotu (nevime, kolik pribylo)
                    ParsePages(datasetId, lastId.Value,1500); //stahnuti, parsovani dat z UOHS a vlozeni do Datasetu
                }
                else
                {
                    Console.WriteLine("Chyba " + result["error"]["description"]);
                }
                return;
            }

            //datasetId = Register(datasetId); //registrace datasetu, a pripadne smazani puvodniho
                                            // funkce vrati jmeno vytvoreneho datasetu
            SetTemplates(datasetId); //nastaveni templatu v extra kroku

            ParsePages(datasetId); //stahnuti, parsovani dat z UOHS a vlozeni do Datasetu
        }

        static string apiToken = "Token ..."; //svůj najdes na HlidacStatu.cz/API
        static string apiRoot = "https://www.hlidacstatu.cz/api/v1";

        public static string Register(string datasetId)
        {
            Newtonsoft.Json.Schema.Generation.JSchemaGenerator jsonGen = new Newtonsoft.Json.Schema.Generation.JSchemaGenerator();
            jsonGen.DefaultRequired = Newtonsoft.Json.Required.Default;


            var registration = new
            {
                name = "Rozhodnuti UOHS", //povinne, verejne jmeno datasetu
                datasetId = datasetId, //nepovinne, doporucujeme uvest. Jednoznacny identifikator datasetu v URL a ve volani API
                origUrl = "http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/", //zdroj dat datasetu
                jsonSchema = jsonGen.Generate(typeof(UOHSData)), //JSON schema 
                betaversion = true, // pokud true, pak dataset neni videt v seznam datasetu na HlidacStatu.cz/data
                allowWriteAccess = false, // pokud true, pak data v datasetu muze kdokoliv přepsat nebo smazat. Stejně tak údaje v registraci.
                                          // pokud false, pak kdokoliv muze data pridat, ale nemuze je prepsat či smazat
                orderList = new string[,] {  { "Nabytí právní moci","PravniMoc" }, { "Účastníci","Ucastnici.Jmeno"} } ,
            };

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

            string jsonResult = httpClient.GetAsync(apiRoot + "/Datasets/" + registration.datasetId)
                        .Result.Content
                        .ReadAsStringAsync().Result;
            var result = JContainer.Parse(jsonResult);
            if (result.HasValues == true) //dataset uz existuje
            {
                //smazu ho
                //zde uvedeno jako priklad,
                jsonResult = httpClient.DeleteAsync(apiRoot + "/Datasets/" + registration.datasetId)
                            .Result.Content
                            .ReadAsStringAsync().Result;

             }

            //vytvoreni nove registrace

            var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(registration));
            jsonResult = httpClient.PostAsync(apiRoot + "/Datasets", content)
                        .Result.Content
                        .ReadAsStringAsync().Result;
            result = JObject.Parse(jsonResult);
            if (result["error"] == null)
            {
                Console.WriteLine("Zaregistrovan dataset " + result["datasetId"].Value<string>());
                return result["datasetId"].Value<string>();
            }
            else
            {
                Console.WriteLine("Chyba " + result["error"]["description"]);
                return null;
            }
        }

        public static void SetTemplates(string datasetId)
        {
            var searchTemplateHtml = new
            {
                header = @"
<table class=""table table-hover"">
    <thead>
        <tr>
            <th>ČJ</th>
            <th>Instance</th>
            <th>Nabytí právní moci</th>
            <th>Účastníci řízení</th>
            <th>Věc</th>
            <th>Typ správního řízení</th>
        </tr>
    </thead>
    <tbody>",
                body = @"
            <tr>
                <td style=""white-space: nowrap;"">
                    <a href=""@(fn_DatasetItemUrl(item.Id))"">@item.Cj</a>
                </td>
                <td style=""white-space: nowrap;"">
                    @item.Instance
                </td>
                <td>
                    @(fn_FormatDate(item.PravniMoc, ""dd.MM.yyyy""))
                </td>
                <td>
                    @if (item.Ucastnici != null)
                    {
                        foreach (var u in item.Ucastnici)
                        {
                            if (fn_IsNullOrEmpty(u.ICO))
                            {
                                @fn_NormalizeText(u.Jmeno)
                            }
                            else
                            {
                                <a href=""https://www.hlidacstatu.cz/subjekt/@u.ICO"">@fn_NormalizeText(u.Jmeno)</a><br />
                            }
                        }
                    }
                </td>
                <td>
                    @fn_ShortenText(item.Vec,200)
                </td>
                <td>@item.Typ_spravniho_rizeni</td>
            </tr>",
                footer = @"</tbody></table>"
            };

            var detailTemplateHtml = new 
            {
                title = "@item.Vec",
                header = "",
                body = @"<table class=""table table-hover"">
        <tbody>
            <tr>
                <td>Číslo jednací</td>
                <td>@item.Cj</td>
            </tr>
            <tr>
                <td>Instance</td>
                <td>@item.Instance</td>
            </tr>
            <tr>
                <td>Věc</td>
                <td>@item.Vec</td>
            </tr>
            <tr>
                <td>Účastníci</td>
                <td>

                    @if (item.Ucastnici != null)
                    {
                        foreach (var u in item.Ucastnici)
                        {
                            if (fn_IsNullOrEmpty(u.ICO))
                            {
                                @fn_NormalizeText(u.Jmeno)
                            }
                            else
                            {
                                <a href=""https://www.hlidacstatu.cz/subjekt/@u.ICO"">@fn_NormalizeText(u.Jmeno)</a><br />
                            }
                        }
                    }

                </td>
            </tr>
            <tr>
                <td>Typ řízení</td>
                <td>@(item.Typ_spravniho_rizeni)</td>
            </tr>
            <tr>
                <td>Typ rozhodnutí</td>
                <td>@(item.Typ_spravniho_rizeni)</td>
            </tr>
            <tr>
                <td>Nabytí právní moci</td>
                <td>@(fn_FormatDate(item.PravniMoc, ""dd.MM.yyyy""))</td>
            </tr>
            <tr>
                <td>Související řízení</td>
                <td>
                    @if (item.SouvisejiciUrl != null)
                    {
                        foreach (var u in item.SouvisejiciUrl)
                        {
                            <a href=""@u"">@u</a><br />
                        }
                    }

                </td>
            </tr>
            <tr>
                <td>Zdroj na UOHS</td>
                <td><a href=""@(item.Url)"" target=""_blank"">@(item.Url)</a></td>
            </tr>
            @if (item.Rozhodnuti != null)
            {
                <tr>
                    <td>Rozhodnutí</td>
                    <td>
                        <pre>
                            @fn_FixPlainText(item.Rozhodnuti.PlainText)
                        </pre>
                    </td>
                </tr>
            }
        </tbody>
    </table>",
                footer = "",
            };
            var registration = new
            {
                name = "Rozhodnutí UOHS",
                datasetId = datasetId,
                origUrl = "http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/",
                searchResultTemplate = searchTemplateHtml,
                detailTemplate = detailTemplateHtml,
                orderList = new string[,] { { "Nabytí právní moci", "PravniMoc" }, { "Účastníci", "Ucastnici.Jmeno" } },
                betaversion = false, // pokud true, pak dataset neni videt v seznam datasetu na HlidacStatu.cz/data
                allowWriteAccess = false, // pokud true, pak data v datasetu muze kdokoliv přepsat nebo smazat. Stejně tak údaje v registraci.
            };

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

            var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(registration));
            var jsonResult = httpClient.PutAsync(apiRoot + "/Datasets/" + datasetId, content)
                        .Result.Content
                        .ReadAsStringAsync().Result;


        }

        public static void ParsePages(string datasetId, int startFrom = 10000, int count = 600)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

            ParallelOptions parOpt = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
            if (System.Diagnostics.Debugger.IsAttached)
                parOpt.MaxDegreeOfParallelism = 1;

            //jedeme v 2 threadech, bud ohleduplny a nedavej vice
            Parallel.ForEach<int>(Enumerable.Range(startFrom, count),
                    parOpt,
                    (i) =>
                    {

                        //stahnutí HTML stránky s rozhodnutím UOHS.
                        //rozhodnutí jsou na samostatnych stránkach, s jednoduchym URL, kde cislo stranky s rozhodnutim postupně roste.
                        // k 1.9.2018 ma posledni rozhodnuti cislo asi 15500
                        string html = "";
                        var url = $"http://www.uohs.cz/cs/verejne-zakazky/sbirky-rozhodnuti/detail-{i}.html";

                        //stahnuti HTML
                        System.Net.WebClient wc = new System.Net.WebClient();
                        wc.Encoding = System.Text.Encoding.UTF8;
                        html = wc.DownloadString(url);

                        //prevedeni do XHTML pomoci HTMLAgilityPacku.
                        //XPath je trida a sada funkci pro jednodusi XPath parsovani
                        XPath page = new XPath(html);

                        //vsechna ziskavana data jsou ziskana pomoci XPATH


                        //stranka neexistuje, tak ji preskocime 
                        if (page.GetNodeText("//head/title")?.Contains("stránka neexistuje") == true)
                            return;

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
                                var ico = httpClient.GetAsync(apiRoot + "/CompanyId?companyName=" + System.Net.WebUtility.UrlEncode(firmaJmeno))
                                                .Result.Content
                                                .ReadAsStringAsync().Result;
                                try
                                {
                                    var icoRes = Newtonsoft.Json.Linq.JObject.Parse(ico);
                                    if (icoRes["ICO"] == null)
                                        ucastnici.Add(new UOHSData.Ucastnik() { Jmeno = firmaJmeno });
                                    else
                                        ucastnici.Add(new UOHSData.Ucastnik()
                                        {
                                            Jmeno = firmaJmeno,
                                            ICO = icoRes["ICO"].Value<string>()
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

                        var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                        var s = httpClient.PostAsync(apiRoot + $"/DatasetItem/{datasetId}/{item.Id}", content).Result.Content.ReadAsStringAsync().Result;
                        JObject result = JObject.Parse(s);
                        if (result["error"] == null)
                        {
                            Console.WriteLine($"OK {i} - {result["id"].Value<string>()}");
                        }
                        else
                            Console.WriteLine($"ERR {i} - {result["error"]["description"].Value<string>()}");

                    }
              ); //parallels.For    


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



    }

}
