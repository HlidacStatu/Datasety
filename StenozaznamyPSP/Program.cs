using CsvHelper;
using HlidacStatu.Api.Dataset.Connector;
using Newtonsoft.Json.Linq;
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
        static HlidacStatu.Api.Dataset.Connector.DatasetConnector dsc;
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            string fn = System.IO.Path.GetFullPath(args[0]);

            if (args.Length == 2)
                apikey = args[1];

            dsc = new HlidacStatu.Api.Dataset.Connector.DatasetConnector(apikey);
            
            //create dataset
            var dsDef = new HlidacStatu.Api.Dataset.Connector.Dataset<Steno>(
                "Stenozáznamy Poslanecké sněmovny Parlamentu ČR", "stenozaznam-psp", "http://www.psp.cz", "Stenozáznamy (těsnopisecké záznamy) jednání Poslanecké sněmovny a jejích orgánů. S využitím Open dat knihovny Ondřeje Kokeše https://github.com/kokes/od/tree/master/data/psp/steno",
                "https://github.com/HlidacStatu/Datasety/tree/master/StenozaznamyPSP",
                true, true,
                new string[,] { { "Podle datumu", "Id" } },
                new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("Id", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}</a>")
                    .AddColumn("Začátek volebního období", "{{ item.rok_zahajeni_vo }}")
                    .AddColumn("Osoba", "{{ fn_RenderPersonWithLink o.OsobaId o.celeJmeno \"\" }}")
                    .AddColumn("Téma", "{{ fn_ShortenText item.tema 50 }}")
                ,
                new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("Id", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}</a>")
                    .AddColumn("Začátek volebního období", "{{ item.rok_zahajeni_vo }}")
                    .AddColumn("Osoba", "{{ fn_RenderPersonWithLink o.OsobaId o.celeJmeno \"\" }}")
                    .AddColumn("Funkce", "{{ item.funkce }}")
                    .AddColumn("Téma", "{{ item.tema }}")
                    .AddColumn("Vystoupení", "{{ item.text }}")
                );

            string datasetid = dsDef.DatasetId;
            if (!dsc.DatasetExists<Steno>(dsDef).Result)
                datasetid = dsc.CreateDataset<Steno>(dsDef).Result;
            else
            {
                //dsc.UpdateDataset<Steno>(dsDef).Result;
            }



            //download, parse and save data into dataset
            GetData(dsDef, datasetid, fn);
        }

        static void GetData(Dataset<Steno> ds, string datasetId, string fn)
        {
            int num = 0;
            using (var reader = new StreamReader(fn))
            {
                using (var csv = new CsvReader(reader, 
                    new CsvHelper.Configuration.Configuration() { 
                        HasHeaderRecord = true, Delimiter = "," }
                        )
                    )
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        //csv rok,datum,schuze,fn,autor,funkce,tema,text
                        var item = new Steno() { 
                          rok_zahajeni_vo = csv.GetField<int>("rok"),
                          datum = null,
                          schuze = csv.GetField<int>("schuze"),
                          fn = csv.GetField<string>("fn"),
                          celeJmeno = csv.GetField<string>("autor"),
                          funkce = csv.GetField<string>("funkce"),
                          tema = csv.GetField<string>("tema"),
                          text = csv.GetField<string>("text"),
                        };

                        num++;
                        item.Id = item.CalcId(num);
                        item.OsobaId = GetOsobaId(item.celeJmeno, item.funkce);

                        if (num % 50 == 0)
                            Console.WriteLine($"{fn} - {num} record");

                        var id = dsc.AddItemToDataset<Steno>(ds, item, DatasetConnector.AddItemMode.Rewrite).Result;
                    }
                }
            }
        }

        public static string GetOsobaId(string fullname, string fce)
        {
            using (var net = new System.Net.WebClient())
            {
                net.Encoding = System.Text.Encoding.UTF8;
                string url = $"https://www.hlidacstatu.cz/api/v1/FindOsobaId?Authorization={apikey}&"
                    + $"celejmeno={System.Net.WebUtility.UrlEncode(fullname)}&funkce={System.Net.WebUtility.UrlEncode(fce)}";
                var json = net.DownloadString(url);
                return Newtonsoft.Json.Linq.JObject.Parse(json).Value<string>("OsobaId");
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
