using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HlidacStatu.Api.Dataset.Connector;

namespace Jednani_vlady
{
    public class Parse
    {

        public const string datasetname = "jednani-vlady";
        public static bool parallel = true;

        static string listUrl = "https://apps.odok.cz/djv-agenda-list?year={0}";
        static string agendaUrl = "https://apps.odok.cz/djv-agenda?date={0}";
        static string usneseniUrl = "https://apps.odok.cz/zvlady/usneseni/-/usn/{0}/{1}";
        public static void DownloadAllData(DatasetConnector dsc)
        {
            var years = Enumerable.Range(2007, DateTime.Now.Year - 2007 + 1);
            int totalSave = 0;

            List<string> agendy = new List<string>();
            Devmasters.Core.Batch.Manager.DoActionForAll(years.Reverse(),
                (y) =>
                {
                    agendy.AddRange(AgendaList(y).OrderBy(o=>o));
                    return new Devmasters.Core.Batch.ActionOutputData();
                }
                , Devmasters.Core.Batch.Manager.DefaultOutputWriter
                , new Devmasters.Core.Batch.ActionProgressWriter(0.1f).Write
                , false
                , maxDegreeOfParallelism: 5
            );

            Devmasters.Core.Batch.Manager.DoActionForAll(agendy,
                (ag) =>
                {
                    Console.WriteLine("Getting " + ag + " ");
                    var jednaniArr = ParseAgenda(ag);

                    Console.WriteLine("SAVING " + ag + " ");

                    foreach (var j in jednaniArr.OrderBy(m=>m.Id))
                    {
                        jednani exists = null;
                        try
                        {
                            Console.Write(j.Id + " L");
                            exists = dsc.GetItemFromDataset<jednani>(datasetname, j.Id).Result;
                        }
                        catch (Exception)
                        {
                        }
                        
                        string id = "";
                        if (exists == null)
                        {
                            id = dsc.AddItemToDataset(datasetname, j, DatasetConnector.AddItemMode.Rewrite).Result;
                            totalSave++;
                            Console.Write("S");
                        }
                        else
                        {
                            bool replace = false;
                            //compare
                            replace = replace ||
                                    (exists.dokumenty?.Count() ?? 0) != (j.dokumenty?.Count() ?? 0);

                            replace = replace ||
                                    (exists.veklep?.Count() ?? 0) != (j.veklep?.Count() ?? 0);

                            replace = replace ||
                                    (exists.meni?.Count() ?? 0) != (j.meni?.Count() ?? 0);
                            replace = replace ||
                                    (exists.zmeneno?.Count() ?? 0) != (j.zmeneno?.Count() ?? 0);
                            replace = replace ||
                                    (exists.rusi?.Count() ?? 0) != (j.rusi?.Count() ?? 0);
                            replace = replace ||
                                    (exists.zruseno?.Count() ?? 0) != (j.zruseno?.Count() ?? 0);

                            if (replace)
                            {
                                id = dsc.AddItemToDataset(datasetname, j, DatasetConnector.AddItemMode.Rewrite).Result;
                                Console.Write("S");
                                totalSave++;
                            }
                        }
                        Console.WriteLine(".");

                    }
                    return new Devmasters.Core.Batch.ActionOutputData();
                }
                , null //Devmasters.Core.Batch.Manager.DefaultOutputWriter
                , null //new Devmasters.Core.Batch.ActionProgressWriter(0.1f).Write
                , false
                , maxDegreeOfParallelism: 5, prefix:"AGENDY: "
            );

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("TOtal saved: " + totalSave);

        }

        static string regexRokFromTxt = @"č\. \s* (?<cislo>\d*)
\s*
(
	(ze \s* dne \s* ) (\d* \. \d* \. (?<rok>\d{4})) 
|
	(z \s* roku \s* ) ( (?<rok>\d{4})) 

)";

        public static jednani ParseUsneseni(DateTime den, string cislo)
        {
            int rok = den.Year;
            string html = "";
            using (var net = new Devmasters.Net.Web.URLContent(string.Format(usneseniUrl, rok, cislo)))
            {
                html = net.GetContent().Text;
            }
            var xp = new XPath(html);
            jednani j = new jednani();
            j.datum = den;
            j.usneseni = $"{cislo}/{den.Year}";
            j.bod = "Usnesení č. " + cislo;
            j.vec = xp.GetNodeText("(//table[@class='lfr-table material-table'])//tr//th[contains(text(),'Název')]/following-sibling::td");
            j.cisloJednaci = xp.GetNodeText("(//table[@class='lfr-table material-table'])//tr//th[contains(text(),'Čj')]/following-sibling::td");
            j.veklep = new string[] {
                        xp.GetNodeText("(//table[@class='lfr-table material-table'])//tr//th[contains(text(),'PID')]/following-sibling::td")
                        };

            List<jednani.dokument> jdocs = new List<jednani.dokument>();
            var xprilohyRows = xp.GetNodes("(//table[@class='lfr-table material-table'])[1]/tr");
            if (xprilohyRows != null)
            {
                foreach (var xpri in xprilohyRows)
                {
                    var jdoc = XPath.Tools.GetNodes(xpri, "td[2]/a");
                    if (jdoc != null)
                    {
                        jdocs.AddRange(jdoc.Select(m => new jednani.dokument()
                        {
                            DocumentUrl = NormalizeUrl(m.GetAttributeValue("href", "")),
                            stahnuto = DateTime.Now,
                            jmeno = XPath.Tools.GetNodeText(xpri, "td[1]")
                        })
                            );
                        j.dokumenty = jdocs.ToArray();
                    }
                }
            }
            if (xp.GetNode("//div[@class='resolutionDerogation']") != null)
            {

                var souvisTypes = new string[] { "repeals", "amends", "repealed", "amended" };

                foreach (var styp in souvisTypes)
                {
                    List<jednani.souvis> jsouvis = new List<jednani.souvis>();

                    if (xp.GetNode($"//div[@class='resolutionDerogation']/div[@class='{styp}']/p") != null)
                    {
                        var txt = xp
                            .GetNodeText($"//div[@class='resolutionDerogation']/div[@class='{styp}']/p")?
                            .Trim()?
                            .Replace(" následující", "");

                        foreach (var item in xp.GetNodes($"//div[@class='resolutionDerogation']/div[@class='{styp}']/dl/dd"))
                        {
                            var jss = new jednani.souvis()
                            {
                                zmena = txt,
                                usneseni = item.InnerText?.Trim(),
                                usneseniOrigUrl = NormalizeUrl(item.SelectSingleNode(".//a").GetAttributeValue("href", null)),
                            };
                            var srok = GetRegexGroupValue(jss.usneseni, regexRokFromTxt, "rok")?.Replace("20","");
                            var scislo = GetRegexGroupValue(jss.usneseni, regexRokFromTxt, "cislo");
                            if (!string.IsNullOrEmpty(srok) && !string.IsNullOrEmpty(scislo))
                            {
                                jss.usneseniCislo = $"{scislo}/{srok}";
                            }

                            jsouvis.Add(jss);
                        }
                    }
                    if (jsouvis != null && jsouvis.Count > 0)
                    {
                        switch (styp)
                        {
                            case "repeals":
                                j.rusi = jsouvis.ToArray();
                                break;
                            case "amends":
                                j.meni = jsouvis.ToArray();
                                break;
                            case "repealed":
                                j.zruseno = jsouvis.ToArray();
                                break;
                            case "amended":
                                j.zmeneno = jsouvis.ToArray();
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            j.SetId();
            return j;
        }

        private static string NormalizeUrl(string url, string prefix = "https://apps.odok.cz")
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;
            if (url.ToLower().StartsWith("http"))
                return url;
            if (!url.StartsWith("/"))
                return prefix + "/" + url;
            else
                return prefix + url;
        }

        public static IEnumerable<jednani> ParseAgenda(string sdatum)
        {
            string html = "";
            using (var net = new Devmasters.Net.Web.URLContent(string.Format(agendaUrl, sdatum)))
            {
                html = net.GetContent().Text;
            }

            DateTime datum = DateTime.ParseExact(sdatum, "yyyy-MM-dd", System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.AssumeLocal);
            List<string> usneseni = new List<string>();
            List<jednani> js = new List<jednani>();
            object lockObj = new object();

            var xp = new XPath(html);

            var rows = xp.GetNodes("//table[@class='lfr-table djv-agenda-table']//tr");
            int ruzneCount = 0;
            foreach (var r in rows)
            {
                if (XPath.Tools.GetNodeAttributeValue(r, "td[1]", "colspan") != null)
                    continue;
                var bod = XPath.Tools.GetNodeText(r, "td[2]") ?? "";

                var obsah = XPath.Tools.GetNode(r, "td[5]");
                if (bod==null && obsah == null)
                    obsah = XPath.Tools.GetNode(r, "td[1]");
                if (bod.Contains("Usnesení č."))
                {
                    usneseni.Add(bod);
                }
                else if (bod.Contains("Příloha č."))
                { }
                else 
                {
                    jednani j = new jednani();
                    if (string.IsNullOrEmpty(bod))
                    {
                        ruzneCount++;
                        bod = "Různé č." + ruzneCount;
                    }
                    j.bod = bod.Trim();
                    j.datum = datum;
                    j.vec = obsah?.InnerText ;
                    var docs = XPath.Tools.GetNodes(r, "td[3]/a");
                    if (docs != null)
                        j.dokumenty = docs.Select(m => new jednani.dokument()
                        {
                            DocumentUrl = NormalizeUrl(m.GetAttributeValue("href", "")),
                            stahnuto = DateTime.Now,
                            jmeno = "Znění",
                        }).ToArray();

                    if (obsah != null && XPath.Tools.GetNodes(obsah, ".//a") != null)
                    {
                        j.veklep = XPath.Tools.GetNodes(obsah, ".//a")
                            .Select(m => m.GetAttributeValue("href", null))
                            .Where(n => n?.StartsWith("/veklep") == true)
                            .Select(m => m.Replace("/veklep-detail?pid=", ""))
                            .ToArray();
                        j.dokumenty = XPath.Tools.GetNodes(obsah, ".//a")
                            .Where(m => m.GetAttributeValue("href", null) != null && m.GetAttributeValue("href", null)?.StartsWith("/veklep") == false)
                            .Select(m => new jednani.dokument()
                                    {
                                         DocumentUrl = NormalizeUrl(m.GetAttributeValue("href", null)),
                                         jmeno = m.InnerText
                                    }
                                )
                            .ToArray();
                    }

                    j.SetId();
                    js.Add(j);
                }
            }

            //parse usneseni

            Devmasters.Core.Batch.Manager.DoActionForAll(usneseni,
                (u) =>
                {
                    lock (lockObj)
                    {
                        js.Add(ParseUsneseni(datum, System.Text.RegularExpressions.Regex.Replace(u, "\\D", "")));
                    }

                    return new Devmasters.Core.Batch.ActionOutputData();
                }
                , null
                , null //new Devmasters.Core.Batch.ActionProgressWriter(0.1f).Write
                , true
                , maxDegreeOfParallelism: 5, prefix:"DAT " + sdatum + ":"
            );

            var dids = js.Select(m => m.Id).Distinct().ToArray();
            if (dids.Count() != js.Count())
            {
                for (int i = 1; i < js.Count(); i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (js[i].Id == js[j].Id)
                            js[i].Id = js[i].Id + "-" + i;
                    }
                }
            }


            return js;
        }

        public static string[] AgendaList(int year)
        {
            using (var net = new Devmasters.Net.Web.URLContent(string.Format(listUrl, year)))
            {
                var html = net.GetContent().Text;
                var xp = new XPath(html);
                return xp.GetNodes("//div[@class='content-main']//a[starts-with(@href,'/djv-agenda')]")
                    .Select(m => m.InnerText)
                    .ToArray();
            }
        }

        public static string GetRegexGroupValue(string txt, string regex, string groupname)
        {
            if (string.IsNullOrEmpty(txt))
                return null;
            Regex myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            foreach (Match match in myRegex.Matches(txt))
            {
                if (match.Success)
                {
                    return match.Groups[groupname].Value;
                }
            }
            return string.Empty;
        }

    }
}
    
