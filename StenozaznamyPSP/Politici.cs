using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{

    public class Politici
    {
        public static List<Tuple<string, string[]>> politiciStems = new List<Tuple<string, string[]>>();
        static HashSet<string> slova = new HashSet<string>();

        static string[] prefixes = ("kolega poslanec předseda místopředseda prezident"
            + " kolegyně poslankyně předsedkyně místopředsedkyně prezidentka")
            .Split(' ');
        static string[] blacklist = { "poslanec celý" };

        static Politici()
        {
            politiciStems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tuple<string, string[]>>>(
        System.IO.File.ReadAllText(@"pol.json")
        );

        }

        public static string[] FindCitations(string text)
        {
            var stopw = new Devmasters.Core.StopWatchEx();
            stopw.Start();
            string[] sText = Stems(text);
            stopw.Stop();
            //Console.WriteLine($"stemmer {stopw.ExactElapsedMiliseconds} ");
            stopw.Restart();
            List<string> found = new List<string>();
            foreach (var kv in politiciStems)
            {
                string zkratka = kv.Item1;
                string[] politik = kv.Item2;

                for (int i = 0; i < sText.Length - (politik.Length - 1); i++)
                {
                    bool same = true;
                    for (int j = 0; j < politik.Length; j++)
                    {
                        if (sText[i + j] == politik[j])
                            same = same & true;
                        else
                        {
                            same = false;
                            break;
                        }
                    }
                    if (same)
                    {
                        if (!found.Contains(zkratka))
                            found.Add(zkratka);
                        break;
                    }

                }

            }
            stopw.Stop();
            //Console.WriteLine($"location {stopw.ExactElapsedMiliseconds} ");
            return found.ToArray();

        }


        public static void InitPol()
        {
            foreach (var s in System.IO.File.ReadAllLines(@"Czech.3-2-5.dic"))
            {
                //var s1 = s.ToLower();
                //if (slova.Contains())
                slova.Add(s);
            }
            string[] p = System.IO.File.ReadAllLines("politici.tsv");
            foreach (var l in p)
            {
                var cols = l.Split('\t');
                var key = cols[0];
                List<string> variants = new List<string>();

                List<string> names = new List<string>();
                for (int i = 1; i < cols.Length; i++)
                {
                    string jmeno = cols[1];
                    string prijmeni = cols.Skip(2).Take(i).Aggregate((f, s) => f + " " + s);
                    names.Add(jmeno + " " + prijmeni);
                    if (!slova.Contains(prijmeni))
                        names.Add(prijmeni);
                }

                foreach (var n in names)
                    foreach (var pref in prefixes)
                    {
                        if (!blacklist.Contains(pref + " " + n))
                            politiciStems.Add(new Tuple<string, string[]>(key, (pref + " " + n).Split(' ')));
                    }
            }
            System.IO.File.WriteAllText(@"c:\!\pol.json", Newtonsoft.Json.JsonConvert.SerializeObject(politiciStems));

        }

        private static Random Rnd = new Random();

        public static string[] Stems(string text)
        {
            //HttpClientHandler httpClientHandler = new HttpClientHandler()
            //{
            //    Proxy = new WebProxy(string.Format("{0}:{1}", "127.0.0.1", 8888), false)
            //};
            var wc = new System.Net.Http.HttpClient();
            try
            {

                var data = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.ToString(text));
                var res = wc.PostAsync(classificationBaseUrl() + "/text_stemmer?ngrams=1", data).Result;

                return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(res.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                wc.Dispose();
            }

        }

        private static string classificationBaseUrl()
        {
            string[] baseUrl = Devmasters.Core.Util.Config.GetConfigValue("Classification.Service.Url")
                .Split(',', ';');
            //Dictionary<string, DateTime> liveEndpoints = new Dictionary<string, DateTime>();

            return baseUrl[Rnd.Next(baseUrl.Length)];

        }
    }
}
