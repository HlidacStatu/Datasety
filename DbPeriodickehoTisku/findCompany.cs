using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPeriodickehoTisku
{

    public class findCompany
    {

        public static string FindIco(string companyName)
        {
            string url = $"https://www.hlidacstatu.cz/api/v2/firmy/{System.Net.WebUtility.UrlEncode(companyName).Trim()}";
            try
            {

                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.Headers.Add("Authorization", Program.apiKey);
                    var str = wc.DownloadString(url);
                    var comp = Newtonsoft.Json.JsonConvert.DeserializeObject<company>(str);
                    if (comp != null)
                        return comp.ico;
                    else
                        return null;

                }
            }
            catch (System.Net.WebException wex)
            {
                var resp = wex.Response as System.Net.HttpWebResponse;
                if (resp?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                if (resp?.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return null;
                Program.logger.Error("FindIco error", wex);
                return null;
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
                        var comp = Newtonsoft.Json.JsonConvert.DeserializeObject<company>(json);
                        if (comp != null)
                            return comp.ico;
                        else
                            return null;
                    }
                }
                catch (Exception e)
                {
                    Program.logger.Error("FindICO error", e, url);
                    //DbPeriodickehoTisku.Program.logger.Error(url, e);
                }
                return null;
            }
        }

    }

    public class company
    {
        public string jmeno { get; set; }
        public string ico { get; set; }
        public string[] datoveSchranky { get; set; }
        public DateTime? zalozena { get; set; }
    }

}
