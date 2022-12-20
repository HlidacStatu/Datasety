using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

using HlidacStatu.Api.V2.CoreApi;

namespace Podpora_COVID
{
    class Program
    {

        public class pomoc
        {
            public string Id { get; set; }
            public string ministerstvo { get; set; }
            public string typ_pomoci { get; set; }
            public string program { get; set; }
            public decimal odhadovana_celkova_vyse_v_mld_kc { get; set; }
            public decimal vyplacena { get; set; }
            public decimal pocet_subjektu { get; set; }
            public DateTime udaj_ke_dni { get; set; }
            public string poznamka { get; set; }
            public string url { get; set; } 

        }

        private const string DatasetNameId = "pomoc-covid";
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"); ;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ");

            UserCredential credential;

            //credentials.json created as described in https://developers.google.com/sheets/api/quickstart/dotnet?authuser=2
            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Podpora-COVID-Update",
            });

            var ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<pomoc>.OpenDataset(System.Configuration.ConfigurationManager.AppSettings["apikey"], DatasetNameId);
            //ds.SetDeveleperUrl("http://local.hlidacstatu.cz/api/v1");

            //
            // Define request parameters.
            String spreadsheetId = "1MtsFm3W8PXkNQ1y0JWYQfb7cl_usCK87nz57uOeI3RM";
            String range = "Data!A2:J100";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values
                    .Where(r => r.Count > 6)
                    )
                {
                    var p = new pomoc();
                    if (row[0].ToString().Length > 0)
                    {
                        p.Id = row[0].ToString();
                        p.ministerstvo = row[1].ToString();
                        p.typ_pomoci = row[2].ToString();
                        p.program = row[3].ToString();
                        p.odhadovana_celkova_vyse_v_mld_kc = decimal.TryParse(row[4].ToString(), out var dd) ? decimal.Parse(row[4].ToString()) : 0 ;
                        p.vyplacena = decimal.TryParse(row[5].ToString(), out var dd1) ?  decimal.Parse(row[5].ToString()) : 0;
                        p.pocet_subjektu = int.Parse(row[6].ToString().Replace(" ","").Replace(((char)160).ToString(),string.Empty));
                        p.udaj_ke_dni = DateTime.ParseExact(row[7].ToString(), "d.M.yyyy", System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"));
                        if (row.Count > 8)
                        {
                            p.poznamka = row[8].ToString();
                        }
                        if (row.Count > 9)
                        {
                            p.url = row[9].ToString();
                        }

                        if (p.typ_pomoci.Length > 5)
                        {
                            Console.WriteLine(p.Id);
                            var id = ds.AddOrUpdateItem(p, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            //Console.Read();
        }
    }
}
