using HlidacStatu.Api.V2.Dataset.Client;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SkutecniMajitele
{
    class Program
    {
        public static Serilog.ILogger _logger = null;

        //static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitel_flat> ds_flat = null;
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele> ds = null;
        public static string apiKey = "";
        public static bool force = false;
        public static bool debug = false;
        public static string root = "";

        static void Main(string[] parameters)
        {
            var loggerBuilder = new LoggerConfiguration()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("HlidacStatu", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("Devmasters", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("HlidacStatu.Connectors.Manager", Serilog.Events.LogEventLevel.Error)
                //.MinimumLevel.Override("HlidacStatu.AI", Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(@"\data\logs\skutecni-majitele\log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(10),
                    retainedFileCountLimit: 30
                    //,outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}"
                );
            _logger = loggerBuilder.CreateLogger();

            var args = new Devmasters.Args(parameters);
            _logger.Information($"Starting with args {string.Join(' ', parameters)}");

            root = AppDomain.CurrentDomain.BaseDirectory;

            //System.Net.Http.HttpClient.DefaultProxy = new System.Net.WebProxy("127.0.0.1", 8888);

            apiKey = args["/apikey"];
            force = args.Exists("/force");
            debug = args.Exists("/debug");

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(majitele)).ToString();

            HlidacStatu.Api.V2.Dataset.Model.Registration reg = new HlidacStatu.Api.V2.Dataset.Model.Registration(
                "Skuteční majitelé firem", "skutecni-majitele",
                "https://esm.justice.cz/",
                "https://github.com/HlidacStatu/Datasety/tree/master/SkutecniMajitele",
                "Evidence skutečných majitelů firem podle zákona č. 37/2021 Sb.",
                genJsonSchema, betaversion: true, allowWriteAccess: false,
                orderList: new string[,]
                {
                    { "Podle datumu zápisu", "datum_zapis" },
                    { "Podle IČ subjektu", "ico" },
                },
                defaultOrderBy: "datum_zapis desc",
                searchResultTemplate: new HlidacStatu.Api.V2.Dataset.Model.Template()
                {
                    Body = @"
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
"
                },
                detailTemplate: new HlidacStatu.Api.V2.Dataset.Model.Template()
                {
                    Body = @"
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

"
                }
            );


            try
            {
                if (args.Exists("/new"))
                {
                    Configuration configuration = new Configuration();
                    configuration.AddApiKey("Authorization", apiKey);
                    HlidacStatu.Api.V2.Dataset.Api.DatasetyApi datasetyApi =
                        new HlidacStatu.Api.V2.Dataset.Api.DatasetyApi(configuration);
                    datasetyApi.ApiV2DatasetyDelete(reg.DatasetId);
                }

                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele>.OpenDataset(apiKey, "skutecni-majitele");
            }
            catch (HlidacStatu.Api.V2.Dataset.Client.ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<majitele>.CreateDataset(apiKey, reg);
            }
            catch (Exception e)
            {
                _logger.Error(e, "problem with dataset");
                throw;
            }

            var wc = new System.Net.WebClient();

            var package_list = Newtonsoft.Json.Linq.JObject.Parse(
                wc.DownloadString("https://dataor.justice.cz/api/3/action/package_list")
            );

            var onlyCurrYears = package_list["result"]
                .ToArray()
                .Select(m => m.Value<string>())
                .Where(m => m.EndsWith($"-{DateTime.Now.Year}") && m.Contains("-full-"));

            if (System.Diagnostics.Debugger.IsAttached)
                onlyCurrYears = onlyCurrYears.Where(m => m.Contains("as-full-praha")); //DEBUG


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
            _logger.Information($"Processing {name} started");
            var filename = $"{name}.xml";
            var filepath = Path.Combine(root, filename);
            
            
            if (System.IO.File.Exists(filepath))
            {
                if (args.Exists("/uselocal"))
                {
                    //skip next, use local file
                }
                else if (force ||
                         (DateTime.Now - new System.IO.FileInfo(root + name + ".xml").LastWriteTime).TotalDays > 4)
                {
                    Console.WriteLine($"Downloading new {name}");
                    DownloadFile(name, filepath);
                }
            }
            else
            {
                DownloadFile(name, filepath);
            }

            if (!System.IO.File.Exists(filepath))
            {
                _logger.Warning($"File [{filepath}] not found");
                return;
            }

            rawXML d = null;

            _logger.Information($"Deserializing {filename}");
            using (var xmlReader = new System.IO.StreamReader(filepath))
            {
                var serializer = new XmlSerializer(typeof(rawXML));
                d = (rawXML)serializer.Deserialize(xmlReader);
            }

            _logger.Information($"{filename} has {d.Subjekt?.Length} subjects");
            var subjs = d.Subjekt;
            if (System.Diagnostics.Debugger.IsAttached)
                subjs = d.Subjekt.Where(m => m.ico == "21167885").ToArray();

            Devmasters.Batch.Manager.DoActionForAll<xmlSubjekt>(subjs,
                subj =>
                {
                    try
                    {
                        CompareLogic cl = new CompareLogic();
                        cl.Config.IgnoreProperty<majitel_base>(m => m.osobaId);

                        var item = majitele.GetMajitele(subj);
                        bool sameAll = true;
                        if (item != null)
                        {
                            //check change
                            var old = ds.GetItemSafe(item.ico);
                            if (old == null)
                            {
                                sameAll = false;
                            }
                            else
                            {
                                if (old.skutecni_majitele?.Count() != item.skutecni_majitele?.Count())
                                    sameAll = false;
                                else if (item.skutecni_majitele?.Count() == old.skutecni_majitele?.Count() &&
                                         item.skutecni_majitele?.Count() > 0)
                                {
                                    var oldDict = old.skutecni_majitele //this was added for big performance boost
                                        .GroupBy(x => x.GetHashCode())
                                        .ToDictionary(g => g.Key, g => g.ToList());

                                    foreach (majitel_base sm in item.skutecni_majitele)
                                    {
                                        bool same = false;
                                        if (oldDict.TryGetValue(sm.GetHashCode(), out var oldSmList))
                                        {
                                            foreach (majitel_base oldSm in oldSmList)
                                            {
                                                ComparisonResult result = cl.Compare(oldSm, sm);
                                                bool areEq = result.AreEqual;

                                                if (areEq)
                                                {
                                                    same = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (same == false)
                                        {
                                            sameAll = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (sameAll == false)
                            {
                                if (debug)
                                {
                                    if (!System.IO.Directory.Exists("changes"))
                                        System.IO.Directory.CreateDirectory("changes");
                                    ComparisonResult result = cl.Compare(old, item);

                                    Console.WriteLine("writing debug object changes for " + item.ico);

                                    System.IO.File.WriteAllText(
                                        root + $"changes\\{item.ico}-{System.DateTime.Now:yyyy-MM-dd}-old.json",
                                        Newtonsoft.Json.JsonConvert.SerializeObject(old, Formatting.Indented));
                                    System.IO.File.WriteAllText(
                                        root + $"changes\\{item.ico}-{System.DateTime.Now:yyyy-MM-dd}-new.json",
                                        Newtonsoft.Json.JsonConvert.SerializeObject(item, Formatting.Indented));
                                    System.IO.File.WriteAllText(
                                        root + $"changes\\{item.ico}-{System.DateTime.Now:yyyy-MM-dd}-changes.json",
                                        Newtonsoft.Json.JsonConvert.SerializeObject(result.Differences,
                                            Formatting.Indented));
                                }

                                item.UpdateOsobaId();
                                ds.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Something went wrong when processing subject [{subj.ico}] from [{name}] file.");
                        throw;
                    }


                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached,
                maxDegreeOfParallelism: 6, prefix: $"{name} ITEMS ");
        }

        private static void DownloadFile(string name, string filepath)
        {
            _logger.Information($"Downloading {name}");
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", filepath);
            }
            catch (Exception e1)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000);
                    wc.DownloadFile($"https://dataor.justice.cz/api/file/{name}.xml", filepath);
                }
                catch (Exception e2)
                {
                    _logger.Error(e2, $"Cannot download https://dataor.justice.cz/api/file/{name}.xml");
                }
            }
        }
    }
}