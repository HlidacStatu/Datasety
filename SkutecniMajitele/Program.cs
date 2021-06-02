using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

using System;
using System.Linq;
using System.Xml.Serialization;

namespace SkutecniMajitele
{
    class Program
    {
        //static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitel_flat> ds_flat = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele> ds = null;
        public static string apiKey = "";
        static void Main(string[] parameters)
        {
            var args = new Devmasters.Args(parameters);
            apiKey = args["/apikey"];
            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(majitele)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
    "Skuteční majitelé firem", "skutecni-majitele",
    "https://esm.justice.cz/",
    "https://github.com/HlidacStatu/Datasety/tree/master/SkutecniMajitele",
    "Evidence skutečných majitelů firem podle zákona č. 37/2021 Sb.",
    genJsonSchema, betaversion: true, allowWriteAccess: false,
    orderList: new string[,] {
                    { "Podle datumu zápisu", "datum_zapis" },
                    { "Podle IČ subjektu", "ico" },
    },
    defaultOrderBy: "datum_zapis desc",
    searchResultTemplate: new ClassicTemplate.ClassicSearchResultTemplate()
        .AddColumn("Detail", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{item.id}}</a>")
        .AddColumn("Subjekt", "{{fn_RenderCompanyWithLink item.ico}}")
        .AddColumn("Skutečný majitel", "{{item.osoba_jmeno}} {{item.osoba_prijmeni}}")
    ,
    detailTemplate: new ClassicTemplate.ClassicDetailTemplate()
        .AddColumn("Detail", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{item.id}}</a>")
        .AddColumn("Subjekt", "{{fn_RenderCompanyWithLink item.ico}}")
        .AddColumn("Skutečný majitel", "{{item.osoba_jmeno}} {{item.osoba_prijmeni}}")
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
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele>.OpenDataset(apiKey, "skutecni-majitele");

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
                .Where(m => m.EndsWith($"-{DateTime.Now.Year}"))
                ;


            foreach (var name in onlyCurrYears)
            {
                if (System.IO.File.Exists(name + ".xml"))
                {
                    if ((DateTime.Now - new System.IO.FileInfo(name + ".xml").CreationTime).TotalDays > 1)
                    {
                        Console.WriteLine($"Downloading new {name}");
                        wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");
                    }
                }
                else
                {
                    Console.WriteLine($"Downloading {name}");
                    wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", name + ".xml");
                }

                rawXML d = null;

                Console.WriteLine($"Deserializing {name}");
                using (var xmlReader = new System.IO.StreamReader(name + ".xml"))
                {
                    var serializer = new XmlSerializer(typeof(rawXML));
                    d = (rawXML)serializer.Deserialize(xmlReader);
                }
                Console.WriteLine($"{d.Subjekt?.Count()} subjects");



                Devmasters.Batch.Manager.DoActionForAll<xmlSubjekt>(d.Subjekt,
                subj =>
                {
                    var item = majitele.GetMajitele(subj);
                    if (item != null && item?.skutecni_majitele?.Count()>0)
                    {
                        if (!ds.ItemExists(item.ico))
                        {
                            ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }
                    }

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                true,//!System.Diagnostics.Debugger.IsAttached, 
                maxDegreeOfParallelism: 6);




            }

        }


    }
}
