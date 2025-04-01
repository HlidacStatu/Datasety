using HlidacStatu.Api.V2.Dataset.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{
    internal static class UpdateAll
    {
        static System.Net.Http.HttpClient httpCl = null;
        static HlidacStatu.Api.V2.swaggerClient apicl = null;


        public class IdItem
        {
            public string id { get; set; }
            public DateTime lastUpdate { get; set; }
        }

        static UpdateAll()
        {
            httpCl = new System.Net.Http.HttpClient() { Timeout = TimeSpan.FromMinutes(2) };
            httpCl.DefaultRequestHeaders.Add("Authorization", Program.apikey);

            apicl = new HlidacStatu.Api.V2.swaggerClient("https://api.hlidacstatu.cz", httpCl);
        }

        public static void Go()
        {
            IdItem[] allIds = Array.Empty<IdItem>();

            var url = $"https://api.hlidacstatu.cz/api/v2/dumpItems/dataset.stenozaznamy-psp";
            using (var net = new Devmasters.Net.HttpClient.URLContent(url))
            {
                net.RequestParams.Headers.Add("Authorization", Program.apikey);
                net.Method = Devmasters.Net.HttpClient.MethodEnum.GET;
                net.Timeout = 60 * 1000;
                string sosoba = "";
                var resp = net.GetContent();
                if (resp != null)
                {
                    allIds = Newtonsoft.Json.JsonConvert.DeserializeObject<IdItem[]>(resp.Text);
                }

            }
            if (System.Diagnostics.Debugger.IsAttached)
                allIds = new IdItem[]
                        {
                            new IdItem(){ id="2002_13_00063" },
                            new IdItem(){ id="2021_98_00630" },
                        };

            System.IO.File.WriteAllLines("ids.txt", allIds.Select(i => i.id));
            System.Collections.Concurrent.ConcurrentBag<string> done = new System.Collections.Concurrent.ConcurrentBag<string>();
            Devmasters.Batch.Manager.DoActionForAll(allIds, (iid) =>
            {
            string id = iid.id;
            bool changed = false;

            var item = Program.dsc.GetItemSafe(id);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.politickaStrana) && Program.rewrite == false)
                    return new Devmasters.Batch.ActionOutputData();

                    if (string.IsNullOrEmpty(item.OsobaId) && !string.IsNullOrEmpty(item.celeJmeno))
                    {
                        (string osobaid, string politickaStrana)? osoba = Program.findInHS(item.celeJmeno, item.funkce);
                        if (osoba != null)
                        {
                            item.OsobaId = osoba.Value.osobaid;
                            item.politickaStrana = osoba.Value.politickaStrana;
                            var osob = apicl.OsobyAsync(item.OsobaId).Result;
                            item.celeJmeno = osob.Jmeno + " " + osob.Prijmeni;
                            changed = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.OsobaId))
                    {
                        try
                        {
                            if (item.OsobaId == "hana-marvanova")
                            {
                                item.OsobaId = "hana-kordova-marvanova";
                                changed = true;
                            }
                            var osob = apicl.OsobyAsync(item.OsobaId).Result;
                            if (osob != null)
                            {
                                item.politickaStrana = osob.PolitickaStrana;
                                changed = true;
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Not found " + item.OsobaId);
                        }
                    }
                    if (changed)
                    {

                        done.Add(id);
                        Program.SaveItem(item, false);
                    }
                }

                return new Devmasters.Batch.ActionOutputData();
            },
            null, new Devmasters.Batch.ActionProgressWriter(0.1f).Writer,
            !System.Diagnostics.Debugger.IsAttached,5
            );

            System.IO.File.WriteAllLines("done.txt", done);

        }

    }
}
