using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SkutecniMajitele
{
    class Program
    {
        private const string DatasetNameId = "skutecni-majitele";
        public static Devmasters.Logging.Logger logger = new Devmasters.Logging.Logger("SkutecniMajitele");

        static Devmasters.Batch.MultiOutputWriter outputWriter =
    new Devmasters.Batch.MultiOutputWriter(
        Devmasters.Batch.Manager.DefaultOutputWriter,
        new Devmasters.Batch.LoggerWriter(logger, Devmasters.Logging.PriorityLevel.Debug).OutputWriter
    );

        static Devmasters.Batch.MultiProgressWriter progressWriter =
            new Devmasters.Batch.MultiProgressWriter(
                new Devmasters.Batch.ActionProgressWriter(1.0f,Devmasters.Batch.Manager.DefaultProgressWriter).Write,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Logging.PriorityLevel.Information).ProgressWriter).Write
            );


        //static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitel_flat> ds_flat = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele> ds = null;
        public static string apiKey = "";
        public static bool force = false;
        static void Main(string[] parameters)
        {
            var args = new Devmasters.Args(parameters);
            logger.Info($"Starting with args {string.Join(' ',parameters)}");

            apiKey = args["/apikey"];
            force = args.Exists("/force");

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(majitele)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
    "Skuteční majitelé firem", DatasetNameId,
    "https://esm.justice.cz/",
    "https://github.com/HlidacStatu/Datasety/tree/master/SkutecniMajitele",
    "Evidence skutečných majitelů firem podle zákona č. 37/2021 Sb.",
    genJsonSchema, betaversion: true, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle datumu zápisu", "datum_zapis" },
                    { "Podle IČ subjektu", "ico" },
    },
    defaultOrderBy: "datum_zapis desc",

    searchResultTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"
<!-- scriban {{ date.now }} --> 
<table class=""table table-hover"">
                        <thead>
                            <tr>
<th>Detail</th>
<th>Subjekt</th>
<th>Skutečný majitel</th>
</tr>
                        </thead>
                        <tbody>
{{ for item in model.Result }}
<tr>
<td ><a href=""{{ fn_DatasetItemUrl item.id }}"">{{item.id}}</a></td>
<td >{{fn_RenderCompanyWithLink item.ico}}</td>
<td >
{{ for sk in item.skutecni_majitele }}

    {{ if !(fn_IsNullOrEmpty sk.osobaId) }}
      {{fn_RenderPersonWithLink2 sk.osobaId}},
    {{else }}
      {{sk.osoba_jmeno}} {{sk.osoba_prijmeni}},

    {{ end }}
{{end }}
</td>
</tr>
{{ end }}

</tbody></table>
" },
    detailTemplate: new HlidacStatu.Api.V2.CoreApi.Model.Template() { Body = @"
<!-- scriban {{ date.now }} --> 
 {{this.item = model}}
<table class=""table table-hover""><tbody>
<tr><td>IČ</td><td ><a href=""{{ fn_DatasetItemUrl item.id }}"">{{item.id}}</a></td></tr>
<tr><td>Subjekt</td><td >{{fn_RenderCompanyWithLink item.ico}}<br/>
{{fn_RenderCompanyInformations item.ico 3}}</td></tr>
<tr><td>Skutečný majitel</td><td >
{{ for sk in item.skutecni_majitele }}
    <dl>
      <dt>
    {{ if !(fn_IsNullOrEmpty sk.osobaId) }}
      {{fn_RenderPersonWithLink2 sk.osobaId}}
    {{else }}
      {{sk.osoba_jmeno}} {{sk.osoba_prijmeni}}
    {{end}}

    ({{sk.udaj_typ_nazev}}) 
      </dt>
      <dd>
      {{if !(fn_IsNullOrEmpty sk.podil_na_prospechu_hodnota) }}
         Podíl na prospěchu ze společnosti: {{sk.podil_na_prospechu_hodnota}} 
         {{if sk.podil_na_prospechu_typ==""PROCENTA""}}%{{else}}({{sk.podil_na_prospechu_typ}}){{end}}
<br/>
      {{end}}
      {{if !(fn_IsNullOrEmpty sk.podil_na_hlasovani_hodnota) }}
         Podíl na hlasovacích právech: {{sk.podil_na_hlasovani_hodnota}} 
         {{if sk.podil_na_hlasovani_typ==""PROCENTA""}}%{{else}}({{sk.podil_na_hlasovani_typ}}){{end}}

<br/>
      {{end}}
      </dd>
    </dl>
{{end }}
</td></tr>
</table>

" }

    );


            try
            {
                if (args.Exists("/new"))
                {
                    Configuration configuration = new Configuration();
                    configuration.AddDefaultHeader("Authorization", apiKey);
                    HlidacStatu.Api.V2.CoreApi.DatasetyApi datasetyApi = new HlidacStatu.Api.V2.CoreApi.DatasetyApi(configuration);
                    datasetyApi.ApiV2DatasetyDelete(reg.DatasetId);
                }
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele>.OpenDataset(apiKey, DatasetNameId);

            }
            catch (HlidacStatu.Api.V2.CoreApi.Client.ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele>.CreateDataset(apiKey, reg);

            }
            catch (Exception e)
            {
                throw;
            }
            var wc = new System.Net.WebClient();

            var package_list = Newtonsoft.Json.Linq.JObject.Parse(
                wc.DownloadString("https://dataor.justice.cz/api/3/action/package_list")
                );

            var onlyCurrYears = package_list["result"]
                .ToArray()
                .Select(m => m.Value<string>())
                .Where(m => m.EndsWith($"-{DateTime.Now.Year}") && m.Contains("-full-"))
                //.Where(m => m == "as-full-ostrava-2021") //DEBUG
                ;

            Devmasters.Batch.Manager.DoActionForAll<string>(onlyCurrYears,
            name =>
            {
                ProcessXML(args, name);

                return new Devmasters.Batch.ActionOutputData();
            }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
            !System.Diagnostics.Debugger.IsAttached, 
            maxDegreeOfParallelism: 2, prefix: "Get XMLS ");
        }

        private static void ProcessXML(Devmasters.Args args, string name)
        {
            logger.Debug($"Starting {name}.xml");
            if (System.IO.File.Exists(name + ".xml"))
            {
                if (args.Exists("/uselocal"))
                {
                    //skip next, use local file
                }
                else if (force || (DateTime.Now - new System.IO.FileInfo(name + ".xml").LastWriteTime).TotalDays > 4)
                {
                    logger.Debug($"downloading new {name}.xml");
                    Console.WriteLine($"Downloading new {name}");
                    DownloadFile(name);
                }
            }
            else
            {
                logger.Debug($"downloading new {name}.xml");
                Console.WriteLine($"Downloading {name}");
                DownloadFile(name);
            }

            if (!System.IO.File.Exists(name + ".xml"))
                return;

            rawXML d = null;

            Console.WriteLine($"Deserializing {name}");
            logger.Debug($"Deserializing {name}.xml");
            using (var xmlReader = new System.IO.StreamReader(name + ".xml"))
            {
                var serializer = new XmlSerializer(typeof(rawXML));
                d = (rawXML)serializer.Deserialize(xmlReader);
            }
            Console.WriteLine($"{d.Subjekt?.Count()} subjects");



            Devmasters.Batch.Manager.DoActionForAll<xmlSubjekt>(d.Subjekt //.Where(m=>m.ico== "3493661")  //debug
                , subj =>
                {
                    majitele item = majitele.GetMajitele(subj);
                    if (item != null && item?.skutecni_majitele?.Count() > 0)
                    {
                        if (!ds.ItemExists(item.ico) || force)
                        {
                            item.UpdateOsobaId();
                            ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }
                        else
                        {
                            //check change
                            var old = ds.GetItem(item.ico);
                            if (old != null)
                            {
                                var same = true;
                                if (old.skutecni_majitele?.Count() != item.skutecni_majitele?.Count())
                                    same = false;
                                else if (item.skutecni_majitele?.Count() == old.skutecni_majitele?.Count() && item.skutecni_majitele?.Count()>0)
                                {
                                    foreach (var sm in item.skutecni_majitele)
                                    {
                                        same = same && old.skutecni_majitele.Any(m =>
                                            m.osoba_jmeno == sm.osoba_jmeno
                                            && m.osoba_prijmeni == sm.osoba_prijmeni
                                            && m.osoba_datum_narozeni == sm.osoba_datum_narozeni
                                            && m.osoba_titul_pred == sm.osoba_titul_pred
                                            && m.osoba_titul_za == sm.osoba_titul_za
                                            && m.adresa_cast_obce == sm.adresa_cast_obce
                                            && m.adresa_cislo_ev == sm.adresa_cislo_ev
                                            && m.adresa_cislo_or == sm.adresa_cislo_or
                                            && m.adresa_cislo_po == sm.adresa_cislo_po
                                            && m.adresa_obec == sm.adresa_obec
                                            && m.adresa_okres == sm.adresa_okres
                                            && m.adresa_psc == sm.adresa_psc
                                            && m.adresa_stat_nazev == sm.adresa_stat_nazev
                                            && m.adresa_text == sm.adresa_text
                                            && m.adresa_ulice == sm.adresa_ulice
                                            && m.slovni_vyjadreni == sm.slovni_vyjadreni
                                            && m.podil == sm.podil
                                            && m.postaveni == sm.postaveni
                                            && !string.IsNullOrEmpty(m.osobaId)
                                        );
                                    }
                                }
                                if (same == false)
                                {
                                    item.UpdateOsobaId();
                                    ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                                }
                            }
                        }
                    }
                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
            !System.Diagnostics.Debugger.IsAttached, 
            maxDegreeOfParallelism: 4, prefix: $"{name} ITEMS ");





        }

        private static void DownloadFile(string name)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");

            }
            catch (Exception e1)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000);
                    wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");
                }
                catch (Exception e2)
                {

                    Console.WriteLine($"Cannot download https://dataor.justice.cz/api/file/{name}.xml   ex:" + e2.Message);
                }
            }
        }
    }
}
