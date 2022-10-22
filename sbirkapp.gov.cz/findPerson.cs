using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sbirkapp.gov.cz
{

    public class findPerson
    {

        public static string GetOsobaId(string jmeno, string prijmeni, DateTime narozeni)
        {
            string url = $"https://www.hlidacstatu.cz/api/v2/osoby/hledat?jmeno={System.Net.WebUtility.UrlEncode(jmeno)}&prijmeni={System.Net.WebUtility.UrlEncode(prijmeni)}&datumNarozeni={narozeni:yyyy-MM-dd}";
            try
            {

                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.Headers.Add("Authorization", Program.apiKey);
                    var str = wc.DownloadString(url);
                    var persons = Newtonsoft.Json.JsonConvert.DeserializeObject<person[]>(str);
                    if (persons?.Count() > 0)
                        return persons.First().NameId;
                    else
                        return null;

                }
            }
            catch (Exception ex)
            {
                System.Threading.Thread.Sleep(200);
                try
                {
                    using (var net = new Devmasters.Net.HttpClient.URLContent(url))
                    {
                        net.TimeInMsBetweenTries = 500;
                        net.Tries = 5;
                        net.RequestParams.Headers.Add("Authorization", Program.apiKey);
                        var json = net.GetContent().Text;
                        var persons = Newtonsoft.Json.JsonConvert.DeserializeObject<person[]>(json);
                        if (persons?.Count() > 0)
                            return persons.First().NameId;
                        else
                            return null;
                    }
                }
                catch (Exception e)
                {
                    //sbirkapp.gov.cz.Program.logger.Error(url, e);
                }
                return null;
            }
        }

        public person[] persons { get; set; }
    }

    public class person
    {
        public string Jmeno { get; set; }
        public string Prijmeni { get; set; }
        public DateTime Narozeni { get; set; }
        public string NameId { get; set; }
        public string Profile { get; set; }
    }

}
