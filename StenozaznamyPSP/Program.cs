using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{

    class Program
    {
        static string apikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Steno> dsc;
        internal static Random rnd = new Random();

        static void Help()
        {
            Console.WriteLine(@"

Zpracování steno záznamů:
StenozaznamyPSP /apikey=hlidac-Api-Key /rok=volebni-rok [/schuze=cislo-schuze] [/rewrite] /daysback=[3]

");

        }

        static void Main(string[] args)
        {
            string argValue = string.Empty;
            if (args.Count() == 0)
            {
                Help(); return;
            }

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments = args
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);


            if (!arguments.TryGetValue("/apikey", out apikey))
            {
                Help(); return;
            }


            int daysBack = 3;
            if (arguments.TryGetValue("/daysback", out argValue))
                daysBack = Convert.ToInt32(argValue);

            int rok = 0;
            if (arguments.TryGetValue("/rok", out argValue))
                rok = Convert.ToInt32(argValue);
            else
            {
                Help(); return;
            }

            bool rewrite = false;
            if (arguments.TryGetValue("/rewrite", out argValue))
                rewrite = true;

            int? schuze = null;
            if (arguments.TryGetValue("/schuze", out argValue))
                schuze = Convert.ToInt32(argValue);



            dsc = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Steno>.OpenDataset(apikey,"stenozaznamy-psp");
            //create dataset

            string datasetid = "stenozaznamy-psp";

            //var data = ParsePSPWeb.ParseSchuze(2010, 5).ToArray();
            //System.Diagnostics.Debugger.Break();

            StreamWriter reader = null;
            CsvWriter csv = null;

            HashSet<string> jmena2Check = new HashSet<string>();


            var vsechnSchuze = ParsePSPWeb.VsechnySchuze(rok);

            //find latest item already in DB

            var lastSchuzeInDb = 1;

            List<int> schuzeToParse = new List<int>();
            if (schuze.HasValue)
            {
                schuzeToParse.Add(schuze.Value);
            }
            else if (rewrite)
            {
                schuzeToParse.AddRange(vsechnSchuze.Select(m => m.schuze));
            }
            else
            {
                //za posledni 3 dny
                DateTime after = DateTime.Now.Date.AddDays(-1*daysBack);
                schuzeToParse.AddRange(vsechnSchuze.Where(m=>m.last >= after).Select(m => m.schuze));
            }



            Console.WriteLine("Zpracuji schuze " + string.Join(",", schuzeToParse));

            Devmasters.Batch.Manager.DoActionForAll<int>(schuzeToParse,
                s =>
            {
                foreach (var item in ParsePSPWeb.ParseSchuze(rok, s))
                {
                    try
                    {
                        if (rewrite == false)
                        {
                            var exists = dsc.ItemExists(item.Id);
                            if (exists)
                                continue; //exists, skip
                        }
                    }
                    catch (Exception) //doesnt exists
                    {
                    }

                    if (item.celeJmeno?.Split(' ')?.Count() > 2)
                        if (!jmena2Check.Contains(item.celeJmeno))
                            jmena2Check.Add(item.celeJmeno);

                    using (var net = new Devmasters.Net.HttpClient.URLContent($"https://www.hlidacstatu.cz/api/v1/PoliticiFromText?Authorization={apikey}"))
                    {
                        net.Method = Devmasters.Net.HttpClient.MethodEnum.POST;
                        net.RequestParams.Form.Add("text", item.text);
                        net.Timeout = 60 * 1000;
                        var sosoby = net.GetContent().Text;
                        var osoby = Newtonsoft.Json.Linq.JArray.Parse(sosoby);
                        if (osoby != null && osoby.Count > 0)
                        {
                            item.politiciZminky = osoby
                                .Select(ja => ja.Value<string>("osobaid"))
                                .Where(o => !string.IsNullOrWhiteSpace(o))
                                .ToArray();
                        }
                    }


                    if (apikey == "csv")
                    {
                        csv.WriteRecord<Steno>(item);
                        csv.NextRecord();
                        if (item.poradi % 10 == 0)
                            csv.Flush();
                    }
                    else
                        SaveItem(item, true);
                }

                return new Devmasters.Batch.ActionOutputData();
            }, !System.Diagnostics.Debugger.IsAttached);

            if (apikey == "csv")
            {
                csv.Flush();
                csv.Dispose();
                reader.Close();
            }


            Console.WriteLine();
            Console.WriteLine("Podezrela jmena:");
            foreach (var k in jmena2Check)
                Console.WriteLine(k);

            return;


            //download, parse and save data into dataset
            //GetData(dsDef, datasetid, fn);
        }
        public static string GetExecutingDirectoryName(bool endsWithBackslash)
        {
            var location = new Uri(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase);
            var dir = new FileInfo(location.AbsolutePath).Directory.FullName + (endsWithBackslash ? @"\" : "");
            return dir;
        }



    
        static void SaveItem( Steno item, bool loadOsobaId)
        {

            if (string.IsNullOrEmpty(item.OsobaId) && loadOsobaId)
            {
                var osobaId = findInHS(item.celeJmeno, item.funkce);

                item.OsobaId = osobaId;
            }

            int tries = 0;
        AddAgain:
            try
            {
                tries++;
                var id = dsc.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                Console.Write("s");

            }
            catch (Exception e)
            {
                if (tries < 300)
                {
                    Console.Write("S");
                    System.Threading.Thread.Sleep(10 * 1000);
                    goto AddAgain;
                }
                else
                    Console.WriteLine(e.Message);
            }

        }

        public static string findInHS(string fullname, string fce)
        {
            //using (var net = new System.Net.WebClient())
            //{
            //    net.Encoding = System.Text.Encoding.UTF8;
            //    string url = $"https://www.hlidacstatu.cz/api/v1/FindOsobaId?Authorization={apikey}&"
            //        + $"celejmeno={System.Net.WebUtility.UrlEncode(fullname)}&funkce={System.Net.WebUtility.UrlEncode(fce)}";
            //    var json = net.DownloadString(url);
            //    return Newtonsoft.Json.Linq.JObject.Parse(json).Value<string>("OsobaId");
            //}
            using (var net = new Devmasters.Net.HttpClient.URLContent($"https://www.hlidacstatu.cz/api/v1/PolitikFromText?Authorization={apikey}"))
            {
                net.Method = Devmasters.Net.HttpClient.MethodEnum.POST;
                net.RequestParams.Form.Add("text", $"{fullname} {fce}");
                net.Timeout = 60 * 1000;
                var sosoba = net.GetContent().Text;
                var osoba = Newtonsoft.Json.Linq.JObject.Parse(sosoba);
                return osoba.Value<string>("osobaid");
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
