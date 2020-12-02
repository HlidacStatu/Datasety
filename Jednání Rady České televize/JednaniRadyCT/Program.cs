using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JednaniRadyCT
{
    class Program
    {
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Jednani> ds = null;

        public static Dictionary<string, string> args = new Dictionary<string, string>();
        public const string DataSetId = "Rada-Ceske-Televize";
        static string YTDL = "";
        static string apiKey = "";
        static string urlPrefix = "https://www.ceskatelevize.cz";
        static bool rewrite = false;
        static DateTime afterDay = DateTime.MinValue;
        static string[] ids = null;
        static string startPath = "";

        static string s2t_username = "";
        static string s2t_password = "";
        static bool skips2t = false;
        static void Main(string[] arguments)
        {
            Console.WriteLine("Jednání Rady ČT - SpeechToText");
            Devmasters.Logging.Logger.Root.Debug("Jednání Rady ČT starting with " + string.Join(',', arguments));
            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);



            if (args.Count < 3)
            {
                Help(); return;
            }

            s2t_username = args["/s2tu"];
            s2t_password = args["/s2tp"];

            if (args.ContainsKey("/utdl"))
                YTDL = args["/utdl"];
            else
                YTDL = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\youtube-dl.exe";

            startPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            apiKey = args["/apikey"];
            rewrite = args.ContainsKey("/rewrite");
            afterDay = args.ContainsKey("/daysback") ? DateTime.Now.Date.AddDays(-1 * Convert.ToInt32(args["/daysback"])) : DateTime.MinValue;
            if (args.ContainsKey("/ids"))
                ids = args["/ids"].Split(',', ';');
            skips2t = args.ContainsKey("/skips2t");


            try
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Jednani>.OpenDataset(apiKey, DataSetId);

            }
            catch (ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Jednani>.CreateDataset(apiKey, Registration());

            }
            catch (Exception e)
            {
                throw;
            }





            string nextPages = "https://www.ceskatelevize.cz/ivysilani/10000000064-jednani-rady-ceske-televize/dalsi-casti/{0}";

            int page = 0;
            bool stop = false;
            List<Jednani> jednani = new List<Jednani>();
            do
            {
                page++;
                using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(string.Format(nextPages, page)))
                {
                    Console.WriteLine($"Page {page}");
                    net.IgnoreHttpErrors = true;
                    net.Tries = 5;
                    net.TimeInMsBetweenTries = 2000;
                    string html = "";
                    try
                    {
                        Devmasters.Logging.Logger.Root.Debug($"downloading {net.Url} ");
                        html = net.GetContent().Text;
                    }
                    catch (Exception e)
                    {
                        Devmasters.Logging.Logger.Root.Error($"{net.Url} failed", e);
                    }

                    Devmasters.XPath xp = new Devmasters.XPath(html);
                    var links = xp.GetNodes("//li[contains(@class,'itemBlock')]");
                    if (links == null || links.Count == 0)
                        break;

                    foreach (var link in links)
                    {
                        Jednani j = new Jednani();
                        j.Odkaz = urlPrefix + Devmasters.XPath.Tools.GetNodeAttributeValue(link, "div/h3/a[@class='itemSetPaging']", "href");
                        j.Titulek = Devmasters.XPath.Tools.GetNodeText(link, "div/h3/a[@class='itemSetPaging']").Trim();
                        j.DatumJednani = Devmasters.DT.Util.ToDate(Devmasters.XPath.Tools.GetNodeText(link, "div/p").Trim()) ?? DateTime.MinValue;
                        j.Id = Devmasters.RegexUtil.GetRegexGroupValue(j.Odkaz, "/ivysilani/10000000064-jednani-rady-ceske-televize/(?<id>\\d{2,})", "id");
                        if (j.DatumJednani > afterDay
                            && (ids == null || ids.Contains(j.Id))
                            )
                        {
                            jednani.Add(j);
                        }
                    }

                }


            } while (stop == false);

            //
            Devmasters.Logging.Logger.Root.Debug($"Starting {jednani.Count} items ");

            Devmasters.Batch.Manager.DoActionForAll<string>(jednani.Select(m => m.Id).Reverse(),
                id =>
                {
                    if (!string.IsNullOrEmpty(id)
                        && (ds.ItemExists(id) == false || rewrite)
                    )
                    {

                        Devmasters.Logging.Logger.Root.Debug($"Start parsing {id} ");
                        var fullJ = ParseJednani(jednani.First(m => m.Id == id));

                        Devmasters.Logging.Logger.Root.Debug($"Saving {id} ");
                        ds.AddOrUpdateItem(fullJ, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.merge);
                    }
                    return new Devmasters.Batch.ActionOutputData() { Log = id };
                }, true, maxDegreeOfParallelism: 5);

        }

        static Jednani ParseJednani(Jednani j)
        {
            using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(j.Odkaz))
            {
                var html = net.GetContent().Text;

                var doc = new Devmasters.XPath(html);

                j.Delka = Devmasters.TextUtil.ConvertToInt(Regex.Replace(doc.GetNodeText("//p[@class='duration']"), "\\D", ""), 0).Value;
                j.Materialy = GetMaterialy(j);
                j.Zapisy = GetZapisy(j);
                j.PrepisAudia = Audio(j);
            }


            return j;
        }

        private static Jednani.Blok[] Audio(Jednani j)
        {
            //youtube-dl.exe --extract-audio --audio-format mp3 --audio-quality 3 -o asdf.%(ext)s https://www.ceskatelevize.cz/ivysilani/10000000064-jednani-rady-ceske-televize/220251000560016

            //download video/audio
            string fnFile = $"{startPath}\\wav\\{j.Id}";
            var fnmp3 = $"{fnFile}.mp3";
            var rawext = ".raw_s2t";

            if (System.IO.File.Exists(fnmp3 + rawext) == false && System.IO.File.Exists(fnFile) == false)
            {
                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo(YTDL, $"--no-progress --extract-audio --audio-format mp3 --audio-quality 3 -o \"{fnFile}.%(ext)s\" " + j.Odkaz);
                pi.WorkingDirectory = System.IO.Path.GetDirectoryName(YTDL);
                Devmasters.ProcessExecutor pe = new Devmasters.ProcessExecutor(pi, 60 * 6 * 24);
                pe.StandardOutputDataReceived += (o, e) => { Devmasters.Logging.Logger.Root.Debug(e.Data); };

                Devmasters.Logging.Logger.Root.Info($"Starting Youtube-dl for {j.Id} ");
                pe.Start();
            }

            if (System.IO.File.Exists(fnmp3 + rawext) == false)
            {
                if (skips2t == false)
                    return null;

                var s2t_id = "ntx.v2t.engine.EngineService/cz/t-broadcast/v2t";
                var s2t_label = "vad+v2t+ppc+pnc";
                var s2t = new Newton.SpeechToText.Cloud.FileAPI.VoiceToText(fnmp3, s2t_username, s2t_password, s2t_id, s2t_label);
                Devmasters.Logging.Logger.Root.Info($"Starting SpeechToText for {j.Id} ");
                s2t.Convert();
                System.IO.File.WriteAllText(fnmp3 + rawext, s2t.Raw);
            }
            var res = new Newton.SpeechToText.Cloud.FileAPI.VoiceToText(System.IO.File.ReadAllText(fnmp3 + rawext))
                .TextWithTimestamps(TimeSpan.FromSeconds(10), true)
                .Select(t => new Jednani.Blok() { SekundOdZacatku = (long)t.Start.TotalSeconds, Text = t.Text })
                .ToArray();

            return res;
        }
            else
                return null;
        }

    static string hzapis = null;
    private static Jednani.Dokument[] GetZapisy(Jednani j)
    {
        if (hzapis == null)
        {
            using (Devmasters.Net.HttpClient.URLContent nzapis = new Devmasters.Net.HttpClient.URLContent("https://www.ceskatelevize.cz/rada-ct/zapisy-z-jednani/"))
            {
                hzapis = nzapis.GetContent().Text;
            }
        }
        var dzapis = new Devmasters.XPath(hzapis);

        var zapisy = dzapis.GetNodes("//a[@class='pdf']");

        List<Jednani.Dokument> docs = new List<Jednani.Dokument>();
        foreach (var z in zapisy)
        {
            if (z.InnerText.Contains($"({j.DatumJednani.ToString("d. M. yyyy")})"))
            {
                Uri? url = null;
                Uri.TryCreate(new Uri(urlPrefix), z.Attributes["href"].Value, out url);
                docs.Add(new Jednani.Dokument()
                {
                    HsProcessType = "document",
                    DocumentUrl = url.AbsoluteUri,
                    Nazev = z.InnerText.Trim(),
                    Typ = "zápis"
                });
            }
        }

        return docs.ToArray();
    }



    static string mzapis = null;
    private static Jednani.Dokument[] GetMaterialy(Jednani j)
    {
        if (mzapis == null)
        {
            using (Devmasters.Net.HttpClient.URLContent nzapis = new Devmasters.Net.HttpClient.URLContent("https://www.ceskatelevize.cz/rada-ct/materialy-projednane-radou/"))
            {
                mzapis = nzapis.GetContent().Text;
            }
        }
        var dzapis = new Devmasters.XPath(mzapis);

        var casti = dzapis.GetNodes("//div[contains(@class,'contentArticle')]/h4[@class='odsazeni']");

        List<Jednani.Dokument> docs = new List<Jednani.Dokument>();
        foreach (var z in casti)
        {
            if (z.InnerText.Contains($"{j.DatumJednani.ToString("d. M. yyyy")}"))
            {
                var pars = Devmasters.XPath.Tools.GetNodes(z, "following::*");
                //jdi az do dalsiho h4
                foreach (var par in pars)
                {
                    if (par.Name == "p")
                    {
                        var link = par.ChildNodes.Where(m => m.Name == "a").FirstOrDefault();
                        if (link != null)
                        {
                            Uri? url = null;
                            Uri.TryCreate(new Uri(urlPrefix), link.Attributes["href"].Value, out url);
                            docs.Add(new Jednani.Dokument()
                            {
                                HsProcessType = "document",
                                DocumentUrl = url.AbsoluteUri,
                                Typ = "material",
                                Nazev = link.InnerText
                            });

                        }
                    }
                    if (par.Name == "h4" && par.Attributes.FirstOrDefault()?.Value == "odsazeni")
                        goto end; //dalsi h4, pryc
                }

            }
        }
    end:
        return docs.ToArray();
    }


    public static void Help()
    {
        Console.WriteLine("\n" +
            "/utdl=[FullPathTo] - cesta k youtube-dl\n" +
            "/token=[Hlidac API token]\n" +
            "/s2tu - newtown api login\n" +
            "/s2tp - newtown api password]\n" +
            "/rewrite \n" +
            "/ids={id,id,id,...} - specific ids\n" +
            "/skips2t - skip speech to text" +
            "");
    }

    static HlidacStatu.Api.V2.CoreApi.Model.Registration Registration()
    {
        var jsonGen = new JSchemaGenerator
        {
            DefaultRequired = Required.Default
        };
        var genJsonSchema = jsonGen.Generate(typeof(Jednani)).ToString();

        HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
            "Jednání Rady České televize", DataSetId,
            "https://www.ceskatelevize.cz/rada-ct/prenos-jednani-po-internetu/",
            "https://github.com/HlidacStatu/Datasety/tree/master/deMinimis/Jednání%20Rady%20České%20televize",
            "Jednání, zápisy, materiály a přepis audiozáznamů jednání Rad České televize.",
            genJsonSchema, betaversion: true, allowWriteAccess: false,
            orderList: new string[,] {
                    { "Podle datumu konání", "DatumJednani" },
            },
            defaultOrderBy: "DatumJednani desc",
            searchResultTemplate: new ClassicTemplate.ClassicSearchResultTemplate()
                .AddColumn("Detail", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{item.Titulek}}</a>")
                .AddColumn("Datum jednání", "{{ fn_FormatDate item.DatumJednani }}")
                .AddColumn("Délka", "{{item.Delka}}&nbsp;min")
                .AddColumn("Materiály", @"
{{ if item.Zapisy && item.Zapisy.size > 0  }}
    {{ for doc in item.Zapisy }}
          {{ doc.Nazev }},
    {{ end }}
{{end}}
{{ if item.Materialy && item.Materialy.size > 0  }}
   {{ fn_Pluralize item.Materialy.size """" ""jeden projednávaný materiál"" ""{0} projednávané materiály"" ""{0} projednávaných materiálů"" }}
{{end}}

")
            ,
            detailTemplate: new ClassicTemplate.ClassicDetailTemplate()
                .AddColumn("ID jednání", @"{{item.Id}}")
                .AddColumn("Jednání", "{{item.Titulek }}")
                .AddColumn("Datum jednání", "{{item.DatumJednani}}")
                .AddColumn("Zápis", @"
{{ if item.Zapisy && item.Zapisy.size > 0  }}
    <ul>
    {{ for doc in item.Zapisy }}
       <li> 
          {{ doc.Nazev }} - 
          {{fn_LinkTextDocumentWithHighlighting doc """ + DataSetId + @""" item.Id ""Obsah dokumentu"" highlightingData }}
       </li>

    {{ end }}
    </ul>
{{end}}
")
                .AddColumn("Projednávané materiály", @"
{{ if item.Materialy && item.Materialy.size > 0  }}
    <ul>
    {{ for doc in item.Materialy }}
       <li> 
          {{ doc.Nazev }} - 
          {{fn_LinkTextDocumentWithHighlighting doc """ + DataSetId + @""" item.Id ""Obsah dokumentu"" highlightingData }}
       </li>

    {{ end }}
    </ul>
{{end}}
")
                .AddColumn("Odkaz na audio", "{{item.Odkaz }}")
                .AddColumn("Délka audio záznamu", "{{ item.Delka }} min")
            );


        return reg;

    }

}
}
