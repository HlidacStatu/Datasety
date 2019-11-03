using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{


    public static class ParsePSPWeb
    {

        //internal static System.Net.WebClient net = new System.Net.WebClient();
        static Dictionary<string, string> mesice = new Dictionary<string, string>()
            {
                {"ledna","1." },
                {"února","2." },
                {"března","3." },
                {"dubna","4." },
                {"května","5." },
                {"června","6." },
                {"července","7." },
                {"srpna","8." },
                {"září","9." },
                {"října","10." },
                {"listopadu","11." },
                {"prosince","12." },
            };

        static string[] vsechnyFunkce = System.IO.File.ReadAllLines("pozice.txt");

        public static int PocetSchuzi(int rok)
        {
            var url = $"http://www.psp.cz/eknih/{rok}ps/stenprot/index.htm";

            var html = GetHtml(url);

            var doc = new XPath(html);

            var maxSchuze = doc.GetNodes("//a[contains(@href,'schuz/index')] | //a[contains(@href,'schuz/')]")
                                .Select(d => d.InnerText)
                                .Where(t=> t?.Contains(". schůze")==true)
                                .Select(t => System.Text.RegularExpressions.Regex.Replace(t, "\\D", ""))
                                .Select(t => Convert.ToInt32(t))
                                .Max();
            return maxSchuze;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rok"></param>
        /// <param name="schuze"></param>
        /// <returns></returns>
        public static IEnumerable<Steno> ParseSchuze(int rok, int schuze)
        {

            var htmlfragment = GenUrl(rok, schuze);

            var item = new Steno() { tema = "Zahájení schůze", obdobi = rok, schuze = schuze };

            int proslovPoradi = 1;
            bool first = true;

            for (int i = 1; i < int.MaxValue; i++)
            {
                string html = "";
                string pageUrl = string.Format(htmlfragment, i);
                if (i == 1)
                {
                    Console.WriteLine();
                    Console.Write(pageUrl + " ");
                }
                else
                    Console.Write(".");

                try
                {
                    html = GetHtml(pageUrl);
                    if (html == null) //no more data
                        goto SendLast;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    yield break;
                }

                var d = new XPath(html);


                //Úterý&nbsp;18.&nbsp;června&nbsp;2019
                string sdate = d.GetNodeText("//div[@class='document-nav']//p[@class='date']")?.Replace("&nbsp;", " ")?.ToLower();
                if (string.IsNullOrEmpty(sdate))
                    continue;
                foreach (var kv in mesice)
                    sdate = sdate.Replace(kv.Key, kv.Value);


                DateTime? date = null;
                if (
                        DateTime.TryParseExact(sdate, "dddd d. M. yyyy", System.Globalization.CultureInfo.GetCultureInfo("cs"), System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var dd)
                    )
                    date = dd;

                var bloky = d.GetNodes("//div[@id='main-content']/p");

                for (int blokNum = 0; blokNum < bloky.Count; blokNum++)
                {
                    var b = bloky[blokNum];
                    //skip prazdne
                    if (string.IsNullOrEmpty(b.InnerText))
                        continue;

                    var rawtext = System.Net.WebUtility.HtmlDecode(b.InnerText)?.Trim();
                    if (string.IsNullOrWhiteSpace(rawtext))
                        continue;

                    var plaintext = rawtext.Replace("\n", " ").Trim();

                    //check poznamky typu 
                    //(14.40 hodin) 
                    //(pokračuje Filip)                                        
                    if (blokNum < 5 && plaintext.StartsWith("(") && plaintext.EndsWith(")"))
                        continue;

                    bool newProslov = false;
                    //nekdy je v <b><a>, nekdy pouze <a>
                    var mluvci = XPath.Tools.GetNodeText(b, "./a[starts-with(@href,'/sqw/detail')]")
                        ?? XPath.Tools.GetNodeText(b, "./b/a[starts-with(@href,'/sqw/detail')]")
                        ?? XPath.Tools.GetNodeText(b, "./b/a[starts-with(@href,'/ff/')]") //2002
                        ?? XPath.Tools.GetNodeText(b, "./b/a[starts-with(@href,'/sqw/p.sqw')]") //1998 
                        ?? XPath.Tools.GetNodeText(b, "./b/b/a[string-length(@href)=0]") //1998
                        ?? GetRegexGroupValue(b.InnerHtml, @"^\s*<b> ((?<name>(\w*\s*){2,})) [:]?  </b> \s? [:]? ", "name") //v 2006 a starsich jsou i jmena v <b> bez <a>
                        ;

                    string funkce = "";
                    if (!string.IsNullOrEmpty(mluvci))
                    {
                        //pouze jmeno, funkci stranou
                        foreach (var f in vsechnyFunkce)
                        {
                            if (mluvci.StartsWith(f, StringComparison.CurrentCultureIgnoreCase))
                            {
                                funkce = f;
                                mluvci = mluvci.Substring(f.Length).Trim();
                                break;
                            }
                        }
                        var nalez = mluvci.IndexOf(" ČR"); //+ vsechny fce s ČR v nazvu (typicky ministri)
                        if (nalez > 0)
                        {
                            funkce = mluvci.Substring(0, nalez + 3).Trim();
                            mluvci = mluvci.Substring(nalez + 3).Trim();
                        }
                    }
                    newProslov = !string.IsNullOrEmpty(mluvci);

                    string tema = null;
                    if (string.IsNullOrEmpty(mluvci))
                    {
                        if (proslovPoradi > 1) //ignoruj prvni prispevek, to je zahajeni schuze)
                        {
                            tema = XPath.Tools.GetNodeText(b, "./b");
                            if (!string.IsNullOrEmpty(tema))
                            {
                                tema = tema.Replace("\n", " ");
                                tema = Devmasters.Core.TextUtil.ReplaceDuplicates(tema, ' ').Trim();
                                item.tema = tema;
                            }
                        }

                    }


                    string text = rawtext;
                    if (newProslov)
                    {
                        string regex = "^" + funkce.Replace(" ", @"\s{1,5}")
                            + @"\s{0,5}"
                            + mluvci.Replace(" ", @"\s{1,5}")
                            + @"\s{0,1}[:]?"; //nekdy tam je :, nekdy ne

                        text = Regex.Replace(text, regex, "").Trim();
                    }


                    //hlasovaniUrl
                    //http://www.psp.cz/sqw/hlasy.sqw?G=69684
                    //nebo https://www.psp.cz/ff/fa/4c/01.htm s redirectem, to zatim ignoruju
                    string hlasovani = GetRegexGroupValue(b.InnerHtml, @"/sqw/hlasy\.sqw\?g=(?<n>\d*)", "n");

                    int? hlasovaniId = null;
                    if (!string.IsNullOrEmpty(hlasovani))
                    {
                        hlasovaniId = Convert.ToInt32(hlasovani);
                    }


                    if (newProslov
                        && first == false
                        )
                    {
                        item.poradi = proslovPoradi;


                        if (string.IsNullOrEmpty(tema))
                            tema = item.tema; //prenes tema do dalsiho prispevku

                        yield return item;

                        proslovPoradi++;
                        item = new Steno() { obdobi = rok, schuze = schuze };
                        item.poradi = proslovPoradi;
                        item.celeJmeno = mluvci;
                        item.funkce = funkce;
                        item.text = "";
                        item.url = pageUrl; //url zacatku proslovu
                        item.cisloHlasovani = hlasovaniId;
                        item.datum = date;
                        item.tema = tema;

                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        item.text += text + "\n";
                    }
                    if (string.IsNullOrEmpty(item.url)) //url zacatku proslovu
                        item.url = pageUrl;

                    if (item.cisloHlasovani.HasValue == false && hlasovaniId.HasValue)
                        item.cisloHlasovani = hlasovaniId;
                    if (item.datum.HasValue == false)
                        item.datum = date;

                    if (first)
                    {
                        item.celeJmeno = mluvci;
                        item.funkce = funkce;
                        if (!string.IsNullOrEmpty(tema))
                            item.tema = tema;
                    }

                    first = false;

                }

            }

        SendLast:
            yield return item;


        }

        static string GenUrl(int rok, int schuze)
        {
            return $"http://www.psp.cz/eknih/{rok}ps/stenprot/{schuze:000}schuz/s{schuze:000}{{0:000}}.htm";
        }


        static string GetHtml(string url)
        {
            try
            {
                //be nice to psp.cz

                System.Threading.Thread.Sleep((int)(Program.rnd.NextDouble() * 100));

                using (Devmasters.Net.Web.URLContent net = new Devmasters.Net.Web.URLContent(url))
                {
                    net.Tries = 10;
                    net.TimeInMsBetweenTries = 2000 * 10; //10s
                    return net.GetContent().Text;
                }

            }
            catch (Devmasters.Net.Web.UrlContentException ex)
            {
                var innerR = (ex.InnerException as System.Net.WebException)?.Response as System.Net.HttpWebResponse;
                if (innerR?.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public static string GetRegexGroupValue(string txt, string regex, string groupname)
        {
            if (string.IsNullOrEmpty(txt))
                return null;
            Regex myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

            try
            {
                foreach (Match match in myRegex.Matches(txt))
                {
                    if (match.Success)
                    {
                        return match.Groups[groupname].Value;
                    }
                }

            }
            catch (Exception)
            {

            }
            return string.Empty;
        }
        public static int CountWords(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;
            MatchCollection collection = Regex.Matches(s, @"[\S]+");
            return collection.Count;
        }

    }
}
