using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Kapacita1Nemocnice
{
    class Program
    {

        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<Nemocnice> ds = null;
        const int everyMins = 58;
        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new System.IO.FileInfo(location.AbsolutePath).Directory.FullName;
        }
        static void Main(string[] args)
        {

            var DArgs = args
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            CreateDataset(DArgs);

            //find xls url
            string openDataPage = "https://dip.mzcr.cz/api/v1/kapacity-intenzivni-pece-zdravotnicke-zarizeni.csv";
            Uri xlsUrl = null;
            Devmasters.Logging.Logger.Root.Info("Getting URL of csv from " + openDataPage);
            using (Devmasters.Net.HttpClient.URLContent net = new Devmasters.Net.HttpClient.URLContent(openDataPage))
            {
                Devmasters.Logging.Logger.Root.Info("Getting csv");
                var fn = "kapacity-intenzivni-pece-zdravotnicke-zarizeni.csv";
                System.IO.File.WriteAllBytes(fn, net.GetBinary().Binary);

                using (var reader = new StreamReader(fn) )//new StringReader(html))
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

                        List<Nemocnice> recs = new List<Nemocnice>();
                        if (true)
                        {
                            csv.Read();
                            csv.ReadHeader();
                            while (csv.Read())
                            {
                                var record = new Nemocnice
                                {
                                    crrt_kapacita_celkem = csv.GetField<int>("crrt_kapacita_celkem"),
                                    crrt_kapacita_volna = csv.GetField<int>("crrt_kapacita_volna"),
                                    ecmo_kapacita_celkem = csv.GetField<int>("ecmo_kapacita_celkem"),
                                    ecmo_kapacita_volna = csv.GetField<int>("ecmo_kapacita_volna"),
                                    ihd_kapacita_celkem = csv.GetField<int>("ihd_kapacita_celkem"),
                                    ihd_kapacita_volna = csv.GetField<int>("ihd_kapacita_volna"),
                                    kraj_nazev = csv.GetField<string>("kraj_nazev"),
                                    kraj_nuts_kod = csv.GetField<string>("kraj_nuts_kod"),
                                    luzka_aro_jip_kapacita_celkem = csv.GetField<int>("luzka_aro_jip_kapacita_celkem"),
                                    luzka_aro_jip_kapacita_volna_covid_negativni = csv.GetField<int>("luzka_aro_jip_kapacita_volna_covid_negativni"),
                                    luzka_aro_jip_kapacita_volna_covid_pozitivni = csv.GetField<int>("luzka_aro_jip_kapacita_volna_covid_pozitivni"),
                                    luzka_standard_kyslik_kapacita_celkem = csv.GetField<int>("luzka_standard_kyslik_kapacita_celkem"),
                                    luzka_standard_kyslik_kapacita_volna_covid_negativni = csv.GetField<int>("luzka_standard_kyslik_kapacita_volna_covid_negativni"),
                                    luzka_standard_kyslik_kapacita_volna_covid_pozitivni = csv.GetField<int>("luzka_standard_kyslik_kapacita_volna_covid_pozitivni"),
                                    reprofilizovana_kapacita_luzka_aro_jip_kapacita_celkem = csv.GetField<int?>("reprofilizovana_kapacita_luzka_aro_jip_kapacita_celkem") ?? 0,
                                    reprofilizovana_kapacita_luzka_aro_jip_kapacita_planovana = csv.GetField<int?>("reprofilizovana_kapacita_luzka_aro_jip_kapacita_planovana") ?? 0,
                                    reprofilizovana_kapacita_luzka_aro_jip_kapacita_volna = csv.GetField<int?>("reprofilizovana_kapacita_luzka_aro_jip_kapacita_volna") ?? 0,
                                    reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_celkem = csv.GetField<int?>("reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_celkem") ?? 0,
                                    reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_planovana = csv.GetField<int?>("reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_planovana") ?? 0,
                                    reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_volna = csv.GetField<int?>("reprofilizovana_kapacita_luzka_standard_kyslik_kapacita_volna") ?? 0,
                                    upv_kapacita_celkem = csv.GetField<int>("upv_kapacita_celkem"),
                                    upv_kapacita_volna = csv.GetField<int>("upv_kapacita_volna"),
                                    ventilatory_operacni_sal_kapacita_celkem = csv.GetField<int>("ventilatory_operacni_sal_kapacita_celkem"),
                                    ventilatory_operacni_sal_kapacita_volna = csv.GetField<int>("ventilatory_operacni_sal_kapacita_volna"),
                                    ventilatory_prenosne_kapacita_celkem = csv.GetField<int>("ventilatory_prenosne_kapacita_celkem"),
                                    ventilatory_prenosne_kapacita_volna = csv.GetField<int>("ventilatory_prenosne_kapacita_volna"),
                                    zz_kod = csv.GetField<string>("zz_kod"),
                                    zz_nazev = csv.GetField<string>("zz_nazev"),
                                    datum = DateTime.ParseExact(csv.GetField<string>("datum"), "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture)
                                };
                                var iis = record.id;
                                recs.Add(record);
                            }
                        }
                        //ds.AddOrRewriteItems(recs);
                        Devmasters.Batch.Manager.DoActionForAll<Nemocnice>(recs.OrderByDescending(o=>o.datum),
                            n =>
                            {
                                Console.Write(".");
                                ds.AddOrUpdateItem(n, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.skip);

                                return new Devmasters.Batch.ActionOutputData();
                            },false);

                    }
                }
            }
        }

        static void CreateDataset(Dictionary<string, string> args)
        {

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(Nemocnice)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
                "Online dispečink intenzivní péče – volné kapacity nemocnic", "kapacity-jedn-nemocnic", "https://onemocneni-aktualne.mzcr.cz/api/v2/covid-19", "", "Všechna data jsou přehledně zobrazena na https://www.hlidacstatu.cz/kapacitanemocnic",
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
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Nemocnice>.OpenDataset(args["/apikey"], "kapacity-jedn-nemocnic");
                if (ds == null)
                    ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Nemocnice>.CreateDataset(args["/apikey"], reg);

            }
            catch (ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Nemocnice>.CreateDataset(args["/apikey"], reg);

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
