using HlidacStatu.Api.Dataset.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tiskove_konference_vlady_CR
{
    public class Parse
    {

        public const string datasetname = "tiskove-konference-vlady";
        public static bool parallel = true;
        static System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                                | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static void DownloadAllData(DatasetConnector dsc, DateTime? from = null)
        {
            from = from ?? new DateTime(2006, 1, 1);

            List<zapis> tiskovky = new List<zapis>();

            int page = 1;
            bool nextPage = false;
            do
            {
                nextPage = false;
                string url = string.Format(Program.startUrl, page);
                Console.WriteLine($"Page {page}");

                string html = "";
                using (var net = new Devmasters.Net.Web.URLContent(url))
                {
                    html = net.GetContent().Text;
                }
                var xp = new XPath(html);
                var items = xp.GetNodes("//div[@class='record-offset']//div[@class='record']");
                foreach (var item in items)
                {
                    zapis zap = new zapis();
                    zap.datum = DateTime.ParseExact(
                        XPath.Tools.GetNodeText(item, "./p[@class='info']"),
                        "d. M. yyyy",
                        System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.AssumeLocal);
                    zap.nazev = XPath.Tools.GetNodeText(item, "./h2/a");
                    zap.url = "https://www.vlada.cz" + XPath.Tools.GetNodeAttributeValue(item, "./h2/a", "href");

                    //https://www.vlada.cz/cz/media-centrum/tiskove-konference/videozaznam-z-tiskove-konference-predsedy-vlady-cr-mirka-topolanka-s-predsedou-vlady-sr-robertem-ficem-19001/
                    //id from unique page id from URL - last integer in url
                    zap.Id = Regex.Match(zap.url, @"\w* - (?<id>\d{1,6}) / $", options).Groups["id"].Value;

                    tiskovky.Add(zap);
                }
                if (tiskovky.Last().datum <= from)
                    goto parse;


                //Dokumenty 1301 až 1305 z 1305
                var counter = xp.GetNodeText("//p[@class='counter']");
                var m = Regex.Match(counter, @"\d* \s* až \s* (?<to>\d{1,4})\s* z \s* (?<z>\d{1,4})", options);
                if (m.Success)
                {
                    var to = m.Groups["to"].Value;
                    var z = m.Groups["z"].Value;
                    nextPage = to != z;
                }

                page++;
            } while (nextPage);



        parse:
            Devmasters.Core.Batch.Manager.DoActionForAll<zapis>(tiskovky,
                (zap) =>
                {
                    try
                    {
                        var ret = ParseTiskovku(dsc, zap);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    return new Devmasters.Core.Batch.ActionOutputData();
                }
                , Devmasters.Core.Batch.Manager.DefaultOutputWriter
                , new Devmasters.Core.Batch.ActionProgressWriter(0.1f).Write
                , !System.Diagnostics.Debugger.IsAttached, 20
                );



        }
        static string ParseTiskovku(DatasetConnector dsc, zapis zap)
        {
            string html = "";
            using (var net = new Devmasters.Net.Web.URLContent(zap.url))
            {
                html = net.GetContent().Text;
            }
            var xp = new XPath(html);
            var casti = xp.GetNodes("//div[@class='content-main']//div[@class='detail']/p");
            if (casti == null)
                return ""; // zatim neni prepsana, jdi na dalsi


            List<zapis.vstup> vyjadreni = new List<zapis.vstup>();
            int poradi = 1;
            zapis.vstup vyj = new zapis.vstup();
            vyj.poradi = poradi;
            string prevMluvci = "~~~";
            foreach (var cast in casti)
            {
                string text = System.Net.WebUtility.HtmlDecode(cast.InnerText);
                string mluvci = XPath.Tools.GetNodeText(cast, "./strong");
                if (string.IsNullOrWhiteSpace(mluvci))
                {
                    vyj.text = vyj.text + "\n" + text;
                }
                else if (mluvci != prevMluvci)
                {
                    if (prevMluvci != "~~~" && !string.IsNullOrEmpty(vyj.jmeno) && !string.IsNullOrEmpty(vyj.prijmeni))
                    {
                        vyjadreni.Add(vyj);

                        poradi++;
                        vyj = new zapis.vstup();
                        vyj.poradi = poradi;
                    }
                    using (var net = new Devmasters.Net.Web.URLContent($"https://www.hlidacstatu.cz/api/v1/OsobaFromText?text={System.Net.WebUtility.UrlEncode(mluvci)}&Authorization={System.Configuration.ConfigurationManager.AppSettings["apikey"]}"))
                    {
                        var osobahtml = net.GetContent().Text;
                        var osoba = Newtonsoft.Json.Linq.JObject.Parse(osobahtml);
                        if (!string.IsNullOrEmpty(osoba.Value<string>("jmeno")))
                        {
                            vyj.jmeno = osoba.Value<string>("jmeno");
                            vyj.prijmeni = osoba.Value<string>("prijmeni");
                            vyj.osobaId = osoba.Value<string>("osobaid");
                            var info = Regex.Replace(mluvci, $@"({vyj.jmeno} \s {vyj.prijmeni} \s* ,?) | ({vyj.prijmeni} \s {vyj.jmeno} \s* ,?)", "", options)
                                        .Replace(":", "");
                            vyj.osobainfo = info?.Trim();
                        }
                        else
                        {
                            vyj.osobainfo = mluvci;
                        }
                    }
                    text = text.Replace(mluvci, "").Trim();
                    vyj.text = text;
                    prevMluvci = mluvci;

                }
            }
            if (!string.IsNullOrEmpty(vyj.text) && !string.IsNullOrEmpty(vyj.jmeno) && !string.IsNullOrEmpty(vyj.prijmeni))
            {
                vyjadreni.Add(vyj);
            }

            ////////// END vyjadreni

            zap.vstupy = vyjadreni.ToArray();

            //mp3
            var mp3urls = xp.GetNodes("//div[@class='record']//a");
            if (mp3urls != null && mp3urls.Count > 0)
            {
                var ahref = mp3urls
                   .FirstOrDefault(m => m.Attributes["href"]?.Value?.ToLower()?.EndsWith(".mp3") == true);
                if (ahref != null)
                    zap.audio = new zapis.odkaz() { url = "https://www.vlada.cz" + ahref.Attributes["href"].Value, nazev = "Zvukový záznam" };
            }
            ////// MP3

            //RELATED

            var relsdt = xp.GetNodes("//dl[@class='related']/dt");
            var relsdd = xp.GetNodes("//dl[@class='related']/dd");

            if (relsdd?.Count > 0 && relsdd?.Count == relsdt?.Count)
            {
                List<zapis.odkaz> odkazy = new List<zapis.odkaz>();
                for (int i = 0; i < relsdt.Count; i++)
                {
                    var odkaz = new zapis.odkaz();
                    odkaz.nazev = System.Net.WebUtility.HtmlDecode(relsdt[i].InnerText) + ": " + System.Net.WebUtility.HtmlDecode(relsdd[i].InnerText);
                    odkaz.url = "https://www.vlada.cz" + XPath.Tools.GetNodeAttributeValue(relsdd[i], "./a", "href");
                    odkazy.Add(odkaz);
                }
                zap.souvisejici = odkazy.ToArray();
            }

            if (zap.vstupy.Count() == 0)
            {
                if (html.Contains("<ifram")
                       || html.Contains("jwplayer")
                       )
                {
                    zap.vstupy = new zapis.vstup[] { new zapis.vstup() { poradi = 1, text = "Přepis tiskové konference není dostupný, pouze videozáznam." } };
                }
                else
                {
                    zap.vstupy = new zapis.vstup[] { new zapis.vstup() { poradi = 1, text = "Přepis tiskové konference není dostupný." } };
                }
            }
            Console.WriteLine(zap.Id + " " + zap.nazev);
            zap.PrepareBeforeSave();
            var id = dsc.AddItemToDataset<zapis>(Parse.datasetname, zap, DatasetConnector.AddItemMode.Rewrite).Result;

            return id;
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

