using System;
using System.Collections.Generic;
using System.Linq;

using HlidacStatu.Api.V2.CoreApi.Client;
using HlidacStatu.Api.V2.Dataset;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

namespace deMinimis
{
    class Program
    {
        public static Dictionary<string, string> args = new Dictionary<string, string>();
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<JednoduchaPodpora> ds = null;

        static void Main(string[] arguments)
        {
            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            int skip = 0;
            if (args.ContainsKey("/skip"))
                skip = int.Parse(args["/skip"]);

            int days = 30;
            if (args.ContainsKey("/days"))
                skip = int.Parse(args["/days"]);

            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(JednoduchaPodpora)).ToString();

            HlidacStatu.Api.V2.CoreApi.Model.Registration reg = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
                "Příjemci podpor z registru de minimis", "de-minimis",
                "http://eagri.cz/public/web/mze/dotace/verejna-podpora-a-de-minimis/registr-de-minimis/",
                "https://github.com/HlidacStatu/Datasety/tree/master/deMinimis/deMinimis",
                "Centrální registr podpor malého rozsahu (Registr de minimis) slouží od pro evidenci podpor de minimis poskytovaných na základě přímo použitelných předpisů EU. Data Ministerstva zemědělství, dostupná pouze přes komplikované API, poskytujeme v jednoduché formě po jednotlivých podporách.",
                genJsonSchema, betaversion: false, allowWriteAccess: false,
                orderList: new string[,] { 
                    { "Podle datumu poskytnutí podpory", "PodporaDatum" }, 
                    { "Podle výše podpory v CZK", "PodporaCzk" }, 
                    { "Podle výše podpory v EUR", "PodporaEur" }, 
                },
                defaultOrderBy:"PodporaDatum desc",
                searchResultTemplate: new ClassicTemplate.ClassicSearchResultTemplate()
                    .AddColumn("Podpora", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{item.Id}}</a>")
                    .AddColumn("Subjekt", "{{fn_RenderCompanyWithLink item.Ico}}")
                    .AddColumn("Podpora", "{{item.PodporaUcel}}")
                    .AddColumn("Výše", "{{fn_FormatPrice item.PodporaCzk }} / {{fn_FormatPrice item.PodporaEur \"EUR\" }}")
                    .AddColumn("Poskytnuta", "{{fn_FormatDate item.PodporaDatum \"dd.MM.yyyy\" }}")
                ,
                detailTemplate: new ClassicTemplate.ClassicDetailTemplate()
                    .AddColumn("Podpora", @"{{item.Id}}")
                    .AddColumn("Subjekt", "{{fn_RenderCompanyWithLink item.Ico}} {{ fn_RenderCompanyStatistic item.PrijemceJmeno }}")
                    .AddColumn("ID prijemce v registru", "{{item.Prijemce_SZRId }}")
                    .AddColumn("Podpora", "{{item.PodporaUcel}}")
                    .AddColumn("Forma podpory", "{{ item.PodporaFormaText }} ({{item.PodporaFormaKod}})")
                    .AddColumn("Účel podpory", "{{ item.PodporaUcel }}")
                    .AddColumn("ID projektu", "{{ item.ProjektId }}")
                    .AddColumn("Podporu poskytl", "{{ fn_RenderCompanyWithLink item.PoskytovatelIco item.PoskytovatelOjm }}")
                    .AddColumn("Právní aktu", "{{ item.PravniAktPoskytnutiText  }} ({{item.PravniAktPoskytnutiId}})")
                    .AddColumn("Výše podpory", "{{fn_FormatPrice item.PodporaCzk }} / {{fn_FormatPrice item.PodporaEur \"EUR\" }}")
                    .AddColumn("Poskytnuta", "{{fn_FormatDate item.PodporaDatum \"dd.MM.yyyy\" }}")
                );


            try
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<deMinimis.JednoduchaPodpora>.OpenDataset(args["/apikey"], "de-minimis");

            }
            catch (ApiException e)
            {
                ds = HlidacStatu.Api.V2.Dataset.Typed.Dataset<deMinimis.JednoduchaPodpora>.CreateDataset(args["/apikey"], reg);

            }
            catch (Exception e)
            {
                throw;
            }

            if (args.ContainsKey("/missing"))
            {
                AddMissingFromJsonDump();return;
            }


            int[] changes = null;
            if (args.ContainsKey("/fn"))
                changes = System.IO.File.ReadAllLines(args["/fn"]).Select(m => Convert.ToInt32(m)).ToArray();
            else
                changes = DeMinimisCalls.GetChanges(DateTime.Now.Date.AddDays(-1 * days))?.seznam_subjektid;


            if (changes != null && changes.Length > 0)
            {
                Devmasters.Batch.Manager.DoActionForAll<int>(changes,
                szrId =>
                {
                    var podporyApi = DeMinimisCalls.GetSubjPerSubjectId(szrId.ToString());

                    var podpory = JednoduchaPodpora.FromAPI(podporyApi);
                    if (podpory != null)
                    {
                        foreach (var p in podpory)
                        {
                            try
                            {
                                if (!ds.ItemExists(p.Id.ToString()))
                                {
                                    ds.AddOrUpdateItem(p, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                                }
                            }
                            catch (Exception e)
                            {
                                ds.AddOrUpdateItem(p, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                            }

                        }
                    }

                    return new Devmasters.Batch.ActionOutputData();
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: 6);

                //AddMissingFromJsonDump();

            }

            static void AddMissingFromJsonDump()
            {

                Devmasters.Batch.Manager.DoActionForAll<string>(System.IO.File.ReadAllLines(@"c:\!!\dataset.de-minimis.dump.json"),
                line =>
                {
                    string s = "";
                    var jp = JednoduchaPodpora.FromJson(line);
                    if (jp != null)
                    {
                        try
                        {
                            if (!ds.ItemExists(jp.Id.ToString()))
                            {
                                s = jp.Id.ToString();

                                ds.AddOrUpdateItem(jp, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                            }
                        }
                        catch (Exception e)
                        {
                            s = jp.Id.ToString();
                            ds.AddOrUpdateItem(jp, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
                        }

                    }
                    return new Devmasters.Batch.ActionOutputData() { Log = s };
                }, Devmasters.Batch.Manager.DefaultOutputWriter, Devmasters.Batch.Manager.DefaultProgressWriter,
                    !System.Diagnostics.Debugger.IsAttached, maxDegreeOfParallelism: 10);

            }
        }
    }
}