using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{

    public class Politici
    {
        public static List<Tuple<string, string[]>> politiciStems = new List<Tuple<string, string[]>>();
        static HashSet<string> slova = new HashSet<string>();

        static string[] prefixes = ("pan kolega poslanec předseda místopředseda prezident"
            + "paní slečna kolegyně poslankyně předsedkyně místopředsedkyně prezidentka")
            .Split(' ');
        static string[] blacklist = { "poslanec celý" };

        static Politici()
        {
            politiciStems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tuple<string, string[]>>>(
        System.IO.File.ReadAllText(Program.GetExecutingDirectoryName(true)+ @"pol.json")
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

        public static string GetExecutingDirectoryName(bool endsWithBackslash)
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            var dir = new FileInfo(location.AbsolutePath).Directory.FullName + (endsWithBackslash ? @"\" : "");
            return dir;
        }

        public static void InitPol()
        {
            politiciStems = new List<Tuple<string, string[]>>();
            foreach (var s in System.IO.File.ReadAllLines(GetExecutingDirectoryName(true) + @"Czech.3-2-5.dic"))
            {
                slova.Add(s);
            }
            string[] p = System.IO.File.ReadAllLines(GetExecutingDirectoryName(true)+"politici.tsv");
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
                {
                    var fname = (n).Split(' ');
                    if (!blacklist.Contains(n) && fname.Length>1)
                        politiciStems.Add(new Tuple<string, string[]>(key, fname ));

                    foreach (var pref in prefixes)
                    {
                        if (!blacklist.Contains(pref + " " + n))
                        {
                            var nn = (pref + " " + n).Split(' ');
                            if (nn.Length>1)
                                politiciStems.Add(new Tuple<string, string[]>(key, nn));
                        }
                    }
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
