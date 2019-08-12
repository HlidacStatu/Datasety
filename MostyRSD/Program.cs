using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

using HlidacStatu.Api.Dataset.Connector; //use component from https://github.com/HlidacStatu/API/tree/master/DatasetConnector


namespace MostyRSD
{
    class Program
    {
        static HlidacStatu.Api.Dataset.Connector.DatasetConnector dsc;
        static void Main(string[] args)
        {
            dsc = new HlidacStatu.Api.Dataset.Connector.DatasetConnector(
                System.Configuration.ConfigurationManager.AppSettings["apikey"]
                );

        //create dataset
            var dsDef = new HlidacStatu.Api.Dataset.Connector.Dataset<Most>(
                "Stav Mostů v ČR", "Stav-Mostu", "http://bms.clevera.cz/Public", "Stav mostů v ČR", "",
                false, true,
                new string[,] { { "Stav mostů", "Stav" }, { "Poslední kontrola", "PosledniProhlidka" } },
                new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("Označení", @"<a href=""@(fn_DatasetItemUrl(item.Id))"">@item.Oznaceni</a>")
                    .AddColumn("Jméno", "@item.Jmeno")
                    .AddColumn("Stav mostu", "@item.PopisStavu")
                    .AddColumn("Poslední kontrola", "@fn_FormatDate(item.PosledniProhlidka,\"dd.MM.yyyy\")")
                    .AddColumn("Mapa", "<a target='blank' href='https://mapy.cz/zakladni?q=@(fn_FormatNumber(item.GPS_Lat,\"en\")),@(fn_FormatNumber(item.GPS_Lng,\"en\"))'>na mapě</a>")
                ,
                new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("Jméno", "@item.Jmeno")
                    .AddColumn("Místní název", "@item.MistniNazev")
                    .AddColumn("Poslední kontrola", "@fn_FormatDate(item.PosledniProhlidka,\"dd.MM.yyyy\")")
                    .AddColumn("Stav mostu", "@item.PopisStavu")
                    .AddColumn("Spravuje", "@item.SpravaOrganizace, @item.SpravaProvozniUsek, @item.SpravaStredisko")
                    .AddColumn("Souřadnice", "Lat: @(fn_FormatNumber(item.GPS_Lat,\"en\"))<br/>Long: @(fn_FormatNumber(item.GPS_Lng,\"en\"))")
                    .AddColumn("Mapa", "<iframe src=\"https://api.mapy.cz/frame?params=%7B%22x%22%3A@(fn_FormatNumber(item.GPS_Lng,\"en\"))%2C%22y%22%3A@(fn_FormatNumber(item.GPS_Lat,\"en\"))%2C%22base%22%3A%221%22%2C%22layers%22%3A%5B%5D%2C%22zoom%22%3A16%2C%22url%22%3A%22https%3A%2F%2Fmapy.cz%2Fs%2F3auci%22%2C%22mark%22%3A%7B%22x%22%3A%22@(fn_FormatNumber(item.GPS_Lng,\"en\"))%22%2C%22y%22%3A%22@(fn_FormatNumber(item.GPS_Lat,\"en\"))%22%2C%22title%22%3A%22Poloha+mostu%22%7D%2C%22overview%22%3Atrue%7D&amp;width=500&amp;height=333&amp;lang=cs\" width=\"500\" height=\"333\" style=\"border:none\" frameBorder=\"0\"></iframe>")
                );


           // var datasetId = dsc.CreateDataset<Most>(dsDef).Result;

            //use this later for updating dataset
            //var datasetId = dsc.UpdateDataset<Most>(dsDef).Result;


            //download, parse and save data into dataset
            DownloadData(dsDef, "stav-mostu");
        }

        static void DownloadData(Dataset<Most> ds, string datasetId)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

            //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json;charset=UTF-8");

            var content = new System.Net.Http.StringContent("{\"bounds\":{\"epsg\":\"5514\",\"esri\":\"10267\",\"xmin\":-971863.6,\"xmax\":-79136.4,\"ymin\":-1235189.7,\"ymax\":-916165},\"layers\":\" Most\",\"zoomIndex\":7}"
                ,System.Text.Encoding.UTF8, "application/json");
            var jsonResult = httpClient.PostAsync("http://bms.clevera.cz/api/assetregistermap/GetMapAllObjects?t=1&d=1", content)
                        .Result.Content
                        .ReadAsStringAsync().Result;

            var data = Newtonsoft.Json.Linq.JObject.Parse(jsonResult);
            List<Most> mosty = new List<Most>();
            foreach (var jo in data["MapObjects"])
            {
                Most m = new Most();
                m.Id = jo["g"].Value<string>();
                var gps = Geo.JTSK.ToWgs(
                    Math.Abs(jo["y"].Value<double>()),
                    Math.Abs(jo["x"].Value<double>())
                    );
                m.GPS_Lat = gps.getLatitude();
                m.GPS_Lng = gps.getLongitude();

                var dataMost = httpClient.GetStringAsync("http://bms.clevera.cz/api/assetregistermap/GetMapObjekt?g=" + m.Id).Result;
                var jsonMost = Newtonsoft.Json.Linq.JObject.Parse(dataMost);
                if (jsonMost["o"] != null)
                {
                    m.Jmeno = jsonMost["o"]["n"].Value<string>();
                    m.MistniNazev = jsonMost["o"]["m"].Value<string>();
                    m.Oznaceni = jsonMost["o"]["c"].Value<string>();
                    string[] spravce = jsonMost["o"]["sl"].Value<string>().Split('|');
                    m.SpravaOrganizace = spravce[0];
                    if (spravce.Length > 1)
                    {
                        m.SpravaStredisko = spravce[1];
                        if (spravce.Length > 2)
                            m.SpravaProvozniUsek = spravce[2];
                    }
                    m.PopisStavu = jsonMost["o"]["s"].Value<string>()?.Trim() ?? "";
                    if (m.PopisStavu.StartsWith("I "))
                        m.Stav = 1;
                    else if (m.PopisStavu.StartsWith("II "))
                        m.Stav = 2;
                    else if (m.PopisStavu.StartsWith("III "))
                        m.Stav = 3;
                    else if (m.PopisStavu.StartsWith("IV "))
                        m.Stav = 4;
                    else if (m.PopisStavu.StartsWith("V "))
                        m.Stav = 5;
                    else if (m.PopisStavu.StartsWith("VI "))
                        m.Stav = 6;
                    else if (m.PopisStavu.StartsWith("VII "))
                        m.Stav = 7;
                    else if (m.PopisStavu.StartsWith("VIII "))
                        m.Stav = 8;

                    m.ProhlidkaPopis = jsonMost["o"]["p"].Value<string>()?.Trim() ?? "";

                    var dat = GetRegexGroupValue(m.ProhlidkaPopis, @"(?<dat>\d{2}\.\d{2}\.\d{4})", "dat");
                    if (!string.IsNullOrEmpty(dat))
                    {
                        if (DateTime.TryParseExact(dat, "dd.MM.yyyy", System.Globalization.CultureInfo.GetCultureInfo("cs"),
                             System.Globalization.DateTimeStyles.AssumeLocal, out var datum))
                            m.PosledniProhlidka = datum;
                    }
                    Console.WriteLine(m.Jmeno);
                    var id = dsc.AddItemToDataset<Most>(ds, m).Result;
                }

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
