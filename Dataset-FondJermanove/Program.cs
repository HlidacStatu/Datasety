using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FondJermanove
{
    class Program
    {
        public class item
        {
            public string Id { get; set; }
            public string Oblast_podpory { get; set; }
            public int Rok_dotacniho_titulu { get; set; }
            public string Cislo { get; set; }
            public string Nazev_projektu { get; set; }
            public string Popis { get; set; }
            public string Stav { get; set; }
            public decimal? Celkove_naklady { get; set; }
            public decimal? Pozadovana_dotace { get; set; }
            public decimal? Proplacena_dotace { get; set; }
            public decimal? Podil_pozadovane_dotace_k_celkovym_nakladum { get; set; }

        }
        static string apiToken = "Token ..."; //svůj najdes na HlidacStatu.cz/API
        static string apiRoot = "https://www.hlidacstatu.cz/api/v1";

        static Registration registration = new Registration()
        {
            name = "Středočeský Fond hejtmana a zmírnění následků živelních katastrof", //povinne, verejne jmeno datasetu
            description = "Schválené, neschválené a podané žádosti o dotaci z fondu hejtmanky ing. Jaroslavy Pokorné Jermanové. Zdrojové údaje získány žádostí dle 106/1999 Sb.",
            datasetId = "fond-hejtmanky-sc", //nepovinne, doporucujeme uvest. Jednoznacny identifikator datasetu v URL a ve volani API
            origUrl = "https://www.infoprovsechny.cz/request/seznam_projektu_z_fondu_hejtmank#incoming-12393", //zdroj dat datasetu
            betaversion = true, // pokud true, pak dataset neni videt v seznam datasetu na HlidacStatu.cz/data
            allowWriteAccess = false, // pokud true, pak data v datasetu muze kdokoliv přepsat nebo smazat. Stejně tak údaje v registraci.
                                      // pokud false, pak kdokoliv muze data pridat, ale nemuze je prepsat či smazat
            orderList = new string[,] {
                { "Rok dotačního titulu", "Rok_dotacniho_titulu" },
                { "Název projektu", "Nazev_projektu.Jmeno" },
                { "Stav schválení", "Stav" },
                { "Požadovaná výše dotace", "Pozadovana_dotace" },
                { "Proplacená dotace", "Proplacena_dotace" },
                { "Podíl požadované dotace na celkových nákladech", "Podil_pozadovane_dotace_k_celkovym_nakladum" },
            },
        };

        static void Main(string[] args)
        {
            if (args.Count() > 0) //predany token a URL v parametrech volani? Pouzij je
            {
                apiToken = "Token " + args[0];
            }

            
            //Register();
            //Parse();
            SetTemplates();
        }
        public static string Register()
        {
            Newtonsoft.Json.Schema.Generation.JSchemaGenerator jsonGen = new Newtonsoft.Json.Schema.Generation.JSchemaGenerator();
            jsonGen.DefaultRequired = Newtonsoft.Json.Required.Default;

            registration.jsonSchema = jsonGen.Generate(typeof(item)); //JSON schema 


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);


            var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(registration));
            var jsonResult = httpClient.PostAsync(apiRoot + "/Datasets", content)
                        .Result.Content
                        .ReadAsStringAsync().Result;
            var result = JObject.Parse(jsonResult);
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



        public static void SetTemplates()
        {
            var searchTemplateHtml = new Registration.Template
            {
                header = @"
<table class=""table table-hover"">
    <thead>
        <tr>
            <th>ČJ</th>
            <th>Rok</th>
            <th>Oblast podpory</th>
            <th>Název</th>
            <th>Stav schválení</th>
            <th>Požadováno</th>
            <th>Proplaceno</th>
        </tr>
    </thead>
    <tbody>",
                body = @"
            <tr>
                <td style=""white-space: nowrap;"">
                    <a href=""@(fn_DatasetItemUrl(item.Id))"">@item.Cislo</a>
                </td>
                <td style=""white-space: nowrap;"">
                    @(item.Rok_dotacniho_titulu)
                </td>
                <td>
                    @(item.Oblast_podpory)
                </td>
                <td>
                    @fn_ShortenText(item.Nazev_projektu,200)
                </td>
                <td>
                    @(item.Stav)
                </td>
                <td>
                    @fn_FormatPrice(item.Pozadovana_dotace)
                </td>
                <td>
                    @fn_FormatPrice(item.Proplacena_dotace)
                </td>
            </tr>",
                footer = @"</tbody></table>"
            };

            var detailTemplateHtml = new Registration.Template
            {
                title = "@item.Vec",
                header = "",
                body = @"<table class=""table table-hover"">
        <tbody>
            <tr>
                <td>Číslo jednací</td>
                <td>@item.Cislo</td>
            </tr>
            <tr>
                <td>Oblast podpory</td>
                <td>@item.Oblast_podpory</td>
            </tr>
            <tr>
                <td>Rok dotačního titulu</td>
                <td>@item.Rok_dotacniho_titulu</td>
            </tr>
            <tr>
                <td>Název projektu</td>
                <td>@item.Nazev_projektu</td>
            </tr>
            <tr>
                <td>Popis projektu</td>
                <td>@item.Popis</td>
            </tr>
            <tr>
                <td>Stav schvalování</td>
                <td>@item.Stav</td>
            </tr>
            <tr>
                <td>Celkové náklady projektu</td>
                <td>@fn_FormatPrice(item.Celkove_naklady) (jak je deklaruje žadatel, nelze ověřit)</td>
            </tr>
            <tr>
                <td>Požadovaná dotace</td>
                <td>@fn_FormatPrice(item.Pozadovana_dotace) </td>
            </tr>
            <tr>
                <td>Proplacená dotace</td>
                <td>@fn_FormatPrice(item.Proplacena_dotace) (do roku 2017 neevidováno)</td>
            </tr>
            <tr>
                <td>Podíl celkové požadované dotace k celkovým nákladům</td>
                <td>@fn_FormatPrice(item.Podil_pozadovane_dotace_k_celkovym_nakladum) (jak je deklaruje žadatel, nelze ověřit)</td>
            </tr>
        </tbody>
    </table>",
                footer = "",
            };
            registration.searchResultTemplate = searchTemplateHtml;
            registration.detailTemplate = detailTemplateHtml;
            registration.betaversion = false; // pokud true, pak dataset neni videt v seznam datasetu na HlidacStatu.cz/data

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

            var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(registration));
            var jsonResult = httpClient.PutAsync(apiRoot + "/Datasets/" + registration.datasetId, content)
                        .Result.Content
                        .ReadAsStringAsync().Result;
        }

        static void Parse()
        {
            string startingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            int id = 0;
            List<item> items = new List<item>();
            foreach (var fn in System.IO.Directory.EnumerateFiles(startingPath, "zadosti*.csv"))
            {
                Console.Write(fn);
                using (System.IO.TextReader readfile = System.IO.File.OpenText(fn)) //
                {
                    var csv = new CsvReader(readfile, new CsvHelper.Configuration.Configuration()
                    {
                        HasHeaderRecord = true,
                        IgnoreBlankLines = true,
                        CultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en-us")
                    });
                    int lines = 1;
                    csv.Read(); csv.ReadHeader();
                    while (csv.Read())
                    {
                        lines++;
                        id++;
                        var item = new item();
                        item.Oblast_podpory = csv.GetField<string>(0);
                        item.Rok_dotacniho_titulu = csv.GetField<int>(1);
                        item.Cislo = csv.GetField<string>(2);
                        item.Nazev_projektu = csv.GetField<string>(3);
                        item.Popis = csv.GetField<string>(4);
                        item.Stav = csv.GetField<string>(5);
                        item.Celkove_naklady = csv.GetField<decimal?>(6);
                        item.Pozadovana_dotace = csv.GetField<decimal?>(7);
                        item.Proplacena_dotace = csv.GetField<decimal?>(8);
                        item.Podil_pozadovane_dotace_k_celkovym_nakladum = csv.GetField<decimal?>(9);

                        item.Id = System.Text.RegularExpressions.Regex.Replace(item.Cislo, @"\W", "-");
                        items.Add(item);
                    }
                    Console.WriteLine("  : " + lines);
                }
            }

            Console.WriteLine("lines: " + items.Count);
            string datasetId = registration.datasetId;
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

            int count = 0;
            foreach (var item in items.Skip(1230))
            {
                count++;
                if (count % 10 == 0)
                    Console.WriteLine(count);

                var content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                var s = httpClient.PostAsync(apiRoot + $"/DatasetItem/{datasetId}/{item.Id}", content).Result.Content.ReadAsStringAsync().Result;
                JObject result = JObject.Parse(s);
                if (result["error"] == null)
                {
                    Console.WriteLine($"OK  {result["id"].Value<string>()}");
                }
                else
                    Console.WriteLine($"ERR  {result["error"]["description"].Value<string>()}");
            }
        }
    }
}
