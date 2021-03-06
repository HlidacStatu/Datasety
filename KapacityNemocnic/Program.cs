﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CsvHelper;

using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using OfficeOpenXml;

namespace KapacityNemocnic
{
    class Program
    {
        public static Dictionary<string, string> Kraje = new Dictionary<string, string>()
        {
            {"CZ010","PHA"},
            {"CZ031","JHC"},
            {"CZ064","JHM"},
            {"CZ041","KVK"},
            {"CZ063","VYS"},
            {"CZ052","HKK"},
            {"CZ051","LBK"},
            {"CZ080","MSK"},
            {"CZ071","OLK"},
            {"CZ053","PAK"},
            {"CZ032","PLK"},
            {"CZ020","STC"},
            {"CZ042","ULK"},
            {"CZ072","ZLK"}
        };

        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData> ds = null;
        const int everyMins = 58;
        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new System.IO.FileInfo(location.AbsolutePath).Directory.FullName;
        }
        static void Main(string[] args)
        {

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ");
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ");


            var DArgs = args
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            CreateDataset(DArgs);
            //Obsazenost.LastObsazenost(ds);

            //if (DArgs.ContainsKey("/xls"))
            //{
            //    //Obsazenost.BackupImap();return;
            //    Obsazenost.Imap(DArgs["/xls"], ds);
            //    Obsazenost.LastObsazenost(ds);
            //    return;
            //}

            //ProcessPacienti();


            OpenDataDIP();

        }

        private static void ProcessPacienti()
        {
            DateTime startDt = DateTime.Now.Date.AddDays(-30);
            using (var reader = new StreamReader(new System.Net.Http.HttpClient().GetStreamAsync("https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19/hospitalizace.csv").Result))
            {
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true,
                    TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
                }))
                {
                    //csv.Context.RegisterClassMap<NemocniceMap>();
                    //var records = csv.GetRecords<Nemocnice>().ToArray();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {

                        DateTime? dt = Devmasters.DT.Util.ParseDateTime(csv["datum"], null);

                        if (dt.HasValue && dt.Value >= startDt)
                        {
                            string id = "id_" + dt.Value.ToString("yyyy-MM-dd");
                            Console.Write(dt.Value.ToString("yyyy-MM-dd "));
                            NemocniceData data = null;
                            try
                            {
                                data = ds.GetItem(id);
                            }
                            catch (Exception)
                            {
                            }
                            if (data == null)
                                continue;

                        }
                    }
                }
            }


        }

        private static void OpenDataDIP()
        {
            List<NemocniceData.Region> raw = new List<NemocniceData.Region>();

            //process https://dip.mzcr.cz/api/v1/kapacity-intenzivni-pece-vlna-2.csv

            DateTime mindate = DateTime.Now.Date.AddDays(-120);

            using (var net = new System.Net.Http.HttpClient().GetStreamAsync("https://dip.mzcr.cz/api/v1/kapacity-intenzivni-pece-zdravotnicke-zarizeni-04-2021.csv"))
            {
                using (System.IO.StreamReader rr = new StreamReader(net.Result))
                {
                    var csv = new CsvHelper.CsvReader(rr, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.GetCultureInfo("cs")) { HasHeaderRecord = true, Delimiter = "," });
                    csv.Read(); csv.ReadHeader();
                    //csv.Read();//skip second line
                    while (csv.Read())
                    {
                        DateTime? date = Devmasters.DT.Util.ParseDateTime(csv.GetField<string>("datum")?.Trim(), null);
                        if (date == null)
                            continue;
                        DateTime dt = date.Value;
                        if (dt < mindate)
                            continue;
                        Console.WriteLine(".");

                        string kraj_nuts_kod = csv.GetField<string>("kraj_nuts_kod");
                        string region = Kraje[kraj_nuts_kod];
                        var r = new NemocniceData.Region();
                        r.lastModified = date.Value;
                        r.region = region;
                        r.name = csv.GetField<string>("zz_nazev");
                        r.UPV_celkem = 0;
                        r.UPV_volna = 0;

                        r.ECMO_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("ecmo_kapacita_celkem"), 0).Value;
                        r.ECMO_volna = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("ecmo_kapacita_volna"), 0).Value;

                        r.CRRT_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("cvvhd_kapacita_celkem"), 0).Value;
                        r.CRRT_volna = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("cvvhd_kapacita_volna"), 0).Value;

                        r.IHD_celkem = 0;
                        r.IHD_volna = 0;

                        r.AROJIP_luzka_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_upv_niv_kapacita_celkem"), 0).Value;
                        r.AROJIP_luzka_covid = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_upv_niv_kapacita_volna_covid_pozitivni"), 0).Value;
                        r.AROJIP_luzka_necovid = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_upv_niv_kapacita_volna_covid_negativni"), 0).Value;

                        r.Standard_luzka_s_kyslikem_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_standard_kyslik_kapacita_celkem"), 0).Value;
                        r.Standard_luzka_s_kyslikem_covid = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_standard_kyslik_kapacita_volna_covid_pozitivni"), 0).Value;
                        r.Standard_luzka_s_kyslikem_necovid = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("luzka_standard_kyslik_kapacita_volna_covid_negativni"), 0).Value;

                        //r.Lekari_AROJIP_celkem = 0;
                        //r.Lekari_AROJIP_dostupni = 0;

                        //r.Sestry_AROJIP_celkem = 0;
                        //r.Sestry_AROJIP_dostupni = 0;
                        //r.Standard_luzka_celkem = 0;
                        //r.Standard_luzka_s_monitor_celkem = 0;

                        r.Ventilatory_prenosne_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("ventilatory_prenosne_kapacita_celkem"), 0).Value;
                        r.Ventilatory_operacnisal_celkem = Devmasters.TextUtil.ConvertToInt(csv.GetField<string>("ventilatory_operacni_sal_kapacita_celkem"), 0).Value;


                        raw.Add(r);

                    }
                }
            }
            Devmasters.Logging.Logger.Root.Info("Saving");
            var dates = raw.Select(m => m.lastModified).Distinct();
            var kraje = raw.Select(m => m.region).Distinct();
            foreach (var dt in dates.Where(m => m > mindate))
            {
                Console.WriteLine(dt);

                NemocniceData nem = null; //new NemocniceData();
                var id = "id_" + dt.ToString("yyyy-MM-dd");
                List<NemocniceData.Region> nr = new List<NemocniceData.Region>();
                foreach (var kr in kraje)
                {
                    var lines = raw.Where(m => m.lastModified == dt && m.region == kr).ToArray();
                    var r = NemocniceData.Aggregate(lines);
                    r.region = kr;
                    nr.Add(r);
                }

                try
                {
                    nem = ds.GetItem(id); // new NemocniceData();
                }
                catch (Exception)
                {
                }
                if (nem == null)
                {
                    nem = new NemocniceData();
                }
                nem.id = id;
                nem.regions = nr;
                nem.lastUpdated = dt;
                ds.AddOrUpdateItem(nem, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);


            }
            //ds.AddOrUpdateItem(nd, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);



        }


        private static void GetExcelFromUzisZIP_Old()
        {
            string fn = GetExecutingDirectoryName() + $"\\dip-report-kraje-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";

            string fnTemp = System.IO.Path.GetTempFileName();
            //nejnovejsi ZIP
            for (int i = 0; i < 7; i++)
            {
                DateTime dt = DateTime.Now.Date.AddDays(-1 * i);
                string zipUrl = $"https://share.uzis.cz/s/fbCgFKagS6fCrzc/download?path=%2F{dt.Year}-{dt.ToString("MM")}%20({dt.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("cs"))}%20{dt.Year})&files={dt:yyyy-MM-dd}-dostupnost-kapacit.zip";     //$"https://share.uzis.cz/s/fbCgFKagS6fCrzc/download?path=%2F&files={dt:yyyy-MM-dd}-dostupnost-kapacit.zip";
                Devmasters.Logging.Logger.Root.Info($"Getting ZIP url {zipUrl}");

                using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(zipUrl))
                {
                    try
                    {
                        System.IO.File.WriteAllBytes(fnTemp, net.GetBinary().Binary);
                        break;
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            try
            {


                Devmasters.Logging.Logger.Root.Info("Getting Excel from ZIP");
                //get xlsx from ZIP
                using (ZipArchive archive = ZipFile.OpenRead(fnTemp))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.ExtractToFile(fn);
                        }
                    }
                }

                if (false) //download xls from web
                {
                    //find xls url
                    string openDataPage = "https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19";
                    Uri xlsUrl = null;
                    Devmasters.Logging.Logger.Root.Info("Getting URL of XLS from " + openDataPage);
                    using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(openDataPage))
                    {
                        Devmasters.Logging.Logger.Root.Info("Getting Excel URL");
                        var html = net.GetContent().Text;

                        Devmasters.XPath xp = new Devmasters.XPath(html);
                        var node = xp.GetNode("//a[contains(@href,'dip-report-kraje.xlsx')]");
                        if (node != null)
                        {
                            xlsUrl = new Uri("https://onemocneni-aktualne.mzcr.cz" + node.Attributes["href"].Value);
                        }
                    }

                    if (xlsUrl == null)
                    {
                        Devmasters.Logging.Logger.Root.Fatal("No URL to download");
                        return;
                    }

                    using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(xlsUrl.AbsoluteUri))
                    {
                        Devmasters.Logging.Logger.Root.Info("Getting Excel");
                        System.IO.File.WriteAllBytes(fn, net.GetBinary().Binary);
                    }
                }

                //debug
                //fn = @"c:\!!\ONLINE_DISPECINK_IP_dostupne_kapacity_20201014_05-50.xlsx";

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage(new System.IO.FileInfo(fn)))
                {
                    ExcelWorksheet ws = p.Workbook.Worksheets[1];

                    //find date
                    //Analýza provedena z exportu 01.10.2020

                    for (int row = 1; row < 100000; row++)
                    {
                        Console.Write(".");
                        var txt = ws.Cells[row, 1].GetValue<string>();
                        if (txt != null && txt.StartsWith("Stav k datu:"))
                        {
                            string head = txt.Replace("Stav k datu: ", "");
                            string sdate = Devmasters.RegexUtil.GetRegexGroupValue(head, @" \s* (?<dt>\d{1,2}\s*\.\s*\d{1,2}\s*\.\s*\d{4} )", "dt");
                            DateTime dt = Devmasters.DT.Util.ToDate(sdate).Value;
                            string id = "id_" + dt.ToString("yyyy-MM-dd");
                            NemocniceData nd = null;
                            try
                            {
                                nd = ds.GetItem(id); // new NemocniceData();
                            }
                            catch (Exception)
                            {
                            }
                            if (nd == null)
                            {
                                nd = new NemocniceData();
                                nd.regions = new List<NemocniceData.Region>();
                            }
                            nd.lastUpdated = dt;

                            nd.id = id;

                            Console.WriteLine(".");
                            Devmasters.Logging.Logger.Root.Info(nd.lastUpdated.ToString());

                            row = row + 4;

                            List<NemocniceData.Region> finalRegs = new List<NemocniceData.Region>();

                            for (int regs = 0; regs < 14; regs++)
                            {
                                string region = ws.Cells[row + regs, 1].GetValue<string>();
                                NemocniceData.Region r = nd.regions.FirstOrDefault(m => m.region == region); //new NemocniceData.Region();
                                if (r == null)
                                {
                                    r = new NemocniceData.Region();
                                }
                                r.lastModified = nd.lastUpdated;
                                r.region = region;

                                r.UPV_celkem = ws.Cells[row + regs, 4].GetValue<int>();
                                r.UPV_volna = ws.Cells[row + regs, 5].GetValue<int>();

                                r.ECMO_celkem = ws.Cells[row + regs, 5].GetValue<int>();
                                r.ECMO_volna = ws.Cells[row + regs, 6].GetValue<int>();

                                r.CRRT_celkem = ws.Cells[row + regs, 8].GetValue<int>();
                                r.CRRT_volna = ws.Cells[row + regs, 9].GetValue<int>();

                                r.IHD_celkem = ws.Cells[row + regs, 11].GetValue<int>();
                                r.IHD_volna = ws.Cells[row + regs, 12].GetValue<int>();

                                r.AROJIP_luzka_celkem = ws.Cells[row + regs, 14].GetValue<int>();
                                r.AROJIP_luzka_covid = ws.Cells[row + regs, 15].GetValue<int>();
                                r.AROJIP_luzka_necovid = ws.Cells[row + regs, 16].GetValue<int>();

                                r.Standard_luzka_s_kyslikem_celkem = ws.Cells[row + regs, 18].GetValue<int>();
                                r.Standard_luzka_s_kyslikem_covid = ws.Cells[row + regs, 19].GetValue<int>();
                                r.Standard_luzka_s_kyslikem_necovid = ws.Cells[row + regs, 20].GetValue<int>();

                                r.Lekari_AROJIP_celkem = ws.Cells[row + regs, 22].GetValue<int>();
                                r.Lekari_AROJIP_dostupni = ws.Cells[row + regs, 23].GetValue<int>();

                                r.Sestry_AROJIP_celkem = ws.Cells[row + regs, 25].GetValue<int>();
                                r.Sestry_AROJIP_dostupni = ws.Cells[row + regs, 26].GetValue<int>();

                                r.Ventilatory_prenosne_celkem = ws.Cells[row + regs, 28].GetValue<int>();
                                r.Ventilatory_operacnisal_celkem = ws.Cells[row + regs, 29].GetValue<int>();

                                r.Standard_luzka_celkem = ws.Cells[row + regs, 30].GetValue<int>();
                                r.Standard_luzka_s_monitor_celkem = ws.Cells[row + regs, 31].GetValue<int>();

                                finalRegs.Add(r);
                            }
                            nd.regions = finalRegs;
                            row = row + 16;

                            Devmasters.Logging.Logger.Root.Info("Saving");

                            ds.AddOrUpdateItem(nd, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }

                    }


                }
            }
            catch (Exception e)
            {
                Devmasters.Logging.Logger.Root.Error("Processing ZIP XLS error", e);
                SendMail("michal@michalblaha.cz", "Selhalo zpracovani dat z UZIS", e.ToString(), "");
            }
        }

        static void CreateDataset(Dictionary<string, string> args)
        {

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(NemocniceData)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
                "Online dispečink intenzivní péče – volné kapacity", "kapacity-nemocnic", "https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19", "", "Všechna data jsou přehledně zobrazena na https://www.hlidacstatu.cz/kapacitanemocnic",
                genJsonSchema, betaversion: false, allowWriteAccess: false,

                searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template("",
                @"<br/>
<br/>
<br/>
<br/>
<p style=""font-size:160%;font-weight:bold;"">
Všechna data jsou přehledně zobrazena na <a href=""https://www.hlidacstatu.cz/kapacitanemocnic?utm_source=dataset&utm_medium=link&utm_campaign=kapacitanemocnic"">https://www.hlidacstatu.cz/kapacitanemocnic</a>
</p>
<br/>

<br/>

Zde jsou uložena v databázi a jsou tak snadno dostupná pro veřejnost přes <a href=""https://www.hlidacstatu.cz/api/v2/swagger/index#/Datasety"">API, aktuální a ve strojově čitelné podobě</a>.
<br/>
<br/>

Originální data v Excelu jsou ke stažení na <a href=""https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19"">https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19</a>, část ""<i>COVID‑19: Online dispečink intenzivní péče – volné kapacity
</i>""
<br/>
"),
                detailTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template("",
                @"<br/>
<br/>
<br/>
<br/>
<p style=""font-size:160%;font-weight:bold;"">
Všechna data jsou přehledně zobrazena na <a href=""https://www.hlidacstatu.cz/kapacitanemocnic?utm_source=dataset&utm_medium=link&utm_campaign=kapacitanemocnic"">https://www.hlidacstatu.cz/kapacitanemocnic</a>
</p>
<br/>

<br/>

Zde jsou uložena v databázi a jsou tak snadno dostupná pro veřejnost přes <a href=""https://www.hlidacstatu.cz/api/v2/swagger/index#/Datasety"">API, aktuální a ve strojově čitelné podobě</a>.
<br/>
<br/>

Originální data v Excelu jsou ke stažení na <a href=""https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19"">https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19</a>, část ""<i>COVID‑19: Online dispečink intenzivní péče – volné kapacity
</i>""
<br/>
")
                );

            try
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData>.OpenDataset(args["/apikey"], "kapacity-nemocnic");
                if (ds == null)
                    ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData>.CreateDataset(args["/apikey"], reg);

            }
            catch (ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<NemocniceData>.CreateDataset(args["/apikey"], reg);

            }
            catch (Exception e)
            {
                throw;
            }


        }

        public static void SendMail(string to, string subject, string body, string replyTo)
        {
            string from = "kapacitanemocnic_app@hlidacstatu.cz";

            using (var smtp = new System.Net.Mail.SmtpClient())
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(from, to);
                msg.Subject = subject;
                if (!string.IsNullOrEmpty(replyTo) && Devmasters.TextUtil.IsValidEmail(replyTo))
                    msg.ReplyToList.Add(new System.Net.Mail.MailAddress(replyTo));
                msg.IsBodyHtml = false;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = body;

                smtp.Send(msg);
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
                    if (match.Groups[groupname].Captures.Count > 1)
                        return match.Groups[groupname].Captures[0].Value;
                    else
                        return match.Groups[groupname].Value;
                }
            }
            return string.Empty;
        }

    }
}
