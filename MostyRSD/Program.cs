﻿
using Newtonsoft.Json.Linq;

using SharpKml.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MostyRSD
{
    class Program
    {
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Most> ds = null;

        static void Main(string[] arguments)
        {
            var args = new Devmasters.Args(arguments, new string[] { "/apikey" });


            //create dataset

            if (!args.MandatoryPresent())
            {
                Console.WriteLine("MostyRSD /apikey=....");
                return;
            }
            try
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Most>.OpenDataset(args["/apikey"], "stav-mostu");

            }
            catch (HlidacStatu.Api.V2.CoreApi.Client.ApiException e)
            {
                //ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Most>.CreateDataset(apiKey, reg);

            }
            catch (Exception e)
            {
                throw;
            }
            //download, parse and save data into dataset
            var mosty = DownloadData(ds);
            GenerateAllKML(mosty);
        }

        static List<Most> DownloadData(HlidacStatu.Api.V2.Dataset.Typed.Dataset<Most> ds)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

            //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json;charset=UTF-8");


            Console.Write($"Reading data from source ....");

            var content = new System.Net.Http.StringContent(
                "{\"bounds\":{\"epsg\":\"5514\",\"esri\":\"10267\",\"xmin\":-991863.6,\"xmax\":-77136.4,\"ymin\":-1255189.7,\"ymax\":-896165},\"layers\":\" Most\",\"zoomIndex\":17}" //cela cr
                                                                                                                                                                                      //"{\"bounds\":{\"epsg\":\"5514\",\"esri\":\"10267\",\"xmin\":-727484.2,\"xmax\":-720530.9,\"ymin\":-1065980.9,\"ymax\":-1063864.3},\"layers\":\"Most Podjezd\",\"zoomIndex\":14}" //test mirosovice
                , System.Text.Encoding.UTF8, "application/json");
            var jsonResult = httpClient.PostAsync("http://bms.clevera.cz/api/assetregistermap/GetMapAllObjects?t=1&d=0", content)
                        .Result.Content
                        .ReadAsStringAsync().Result;

            Console.WriteLine($"Done.");

            var data = Newtonsoft.Json.Linq.JObject.Parse(jsonResult);
            //JArray mosty = data["MapObjects"];

            List<Most> mosty = new List<Most>();
            int count = 0;
            int total = data["MapObjects"].Count();

            ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 5 };

            Parallel.ForEach<JToken>(data["MapObjects"], po, jo =>
            {
                var c = System.Threading.Interlocked.Increment(ref count);
                if (c % 20 == 0)
                    Console.WriteLine($"{count} z {total}");
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
                    mosty.Add(m);
                    var id = ds.AddOrUpdateItem(m, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);

                }

            });
            return mosty;
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

        static void GenerateAllKML(List<Most> mosty)
        {
            for (int i = 0; i < 8; i++)
            {
                GenerateKML(i,mosty.Where(m=>m.Stav == i));
            }
        }
        static void GenerateKML(int stav, IEnumerable<Most> data )
        {

            int page = 1;

            Kml kml = new Kml();

            var style0 = new Style();
            style0.Id = "stav0"; style0.Icon = new IconStyle();
            style0.Icon.Color = new SharpKml.Base.Color32(255, 00, 200, 0); //yell
            style0.Icon.ColorMode = ColorMode.Normal; style0.Icon.Scale = 1.0;

            var style5 = new Style();
            style5.Id = "stav5"; style5.Icon = new IconStyle();
            style5.Icon.Color = new SharpKml.Base.Color32(255, 66, 191, 244); //yell
            style5.Icon.ColorMode = ColorMode.Normal; style5.Icon.Scale = 1.0;

            var style6 = new Style();
            style6.Id = "stav6"; style6.Icon = new IconStyle();
            style6.Icon.Color = new SharpKml.Base.Color32(255, 66, 134, 244); //ORAnGE
            style6.Icon.ColorMode = ColorMode.Normal; style6.Icon.Scale = 1.0;

            var style7 = new Style();
            style7.Id = "stav7"; style7.Icon = new IconStyle();
            style7.Icon.Color = new SharpKml.Base.Color32(255, 0, 0, 255); //red
            style7.Icon.ColorMode = ColorMode.Normal; style7.Icon.Scale = 1.0;

            var document = new Document();
            document.AddStyle(style0); document.AddStyle(style5);
            document.AddStyle(style6); document.AddStyle(style7);

            var fdata = new SharpKml.Dom.Folder();
            fdata.Name = "Stav " + stav;
            foreach (var m in data.Where(m => m.Stav == stav))
            {

                Point point = new Point();
                point.Coordinate = new SharpKml.Base.Vector(m.GPS_Lat, m.GPS_Lng);

                Placemark placemark = new Placemark();
                placemark.Id = m.Id;
                placemark.Name = m.Jmeno;
                placemark.Description = new Description() { Text = $"{m.MistniNazev}. Stav {m.PopisStavu}. Poslední kontrola: {m.ProhlidkaPopis}" };

                var style = "#stav";
                if (m.Stav > 4)
                    style = style + m.Stav;
                else
                    style = style + "0";

                placemark.StyleUrl = new Uri(style, UriKind.Relative);
                placemark.ExtendedData = new ExtendedData();
                placemark.ExtendedData.AddData(new Data() { Value = m.MistniNazev, Name = "Mistni nazev" });
                placemark.ExtendedData.AddData(new Data() { Value = m.Oznaceni, Name = "Označení" });
                placemark.ExtendedData.AddData(new Data() { Value = m.PopisStavu, Name = "Stav" });
                placemark.ExtendedData.AddData(new Data() { Value = m.ProhlidkaPopis, Name = "Poslední prohlídka" });
                placemark.ExtendedData.AddData(new Data() { Value = m.SpravaOrganizace, Name = "Spravující organizace" });
                placemark.ExtendedData.AddData(new Data() { Value = m.SpravaProvozniUsek, Name = "Spravující provozní úsek" });
                placemark.ExtendedData.AddData(new Data() { Value = m.SpravaStredisko, Name = "Spravující středisko" });
                placemark.Geometry = point;
                fdata.AddFeature(placemark);
                //fdata.AddChild(placemark);
            }

            document.AddFeature(fdata);



            kml.Feature = document;
            //kml.AddChild(document);
            SharpKml.Base.Serializer serializer = new SharpKml.Base.Serializer();
            serializer.Serialize(kml);
            System.IO.File.WriteAllText($"mosty_{stav}.kml", serializer.Xml);

        }

    }
}
