using CsvHelper;
using HlidacStatu.Api.Dataset.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{

    class Program
    {
        static string apikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];
        static HlidacStatu.Api.Dataset.Connector.DatasetConnector dsc;
        internal static Random rnd = new Random();

        static void Help()
        {
            Console.WriteLine(@"

Zpracování steno záznamů:
StenozaznamyPSP /apikey=hlidac-Api-Key /rok=volebni-rok [/schuze=cislo-schuze] [/rewrite] 

");

        }

        static void Main(string[] args)
        {
            string argValue = string.Empty;
            if (args.Count() == 0)
            {
                Help(); return;
            }

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments = args
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);


            if (!arguments.TryGetValue("/apikey", out apikey))
            {
                Help(); return;
            }

            int rok = 0;
            if (arguments.TryGetValue("/rok", out argValue))
                rok = Convert.ToInt32(argValue);
            else
            {
                Help(); return;
            }

            bool rewrite = false;
            if (arguments.TryGetValue("/rewrite", out argValue))
                rewrite = true;

            int? schuze = null;
            if (arguments.TryGetValue("/schuze", out argValue))
                schuze = Convert.ToInt32(argValue);


            dsc = new HlidacStatu.Api.Dataset.Connector.DatasetConnector(apikey);

            //create dataset
            var dsDef = Ds();

            string datasetid = dsDef.DatasetId;
            if (apikey != "csv")
            {
                if (!dsc.DatasetExists<Steno>(dsDef).Result)
                    datasetid = dsc.CreateDataset<Steno>(dsDef).Result;
                else
                {
                    //dsc.UpdateDataset<Steno>(dsDef).Result;
                }
            }

            //var data = ParsePSPWeb.ParseSchuze(2010, 5).ToArray();
            //System.Diagnostics.Debugger.Break();

            StreamWriter reader = null;
            CsvWriter csv = null;

            HashSet<string> jmena2Check = new HashSet<string>();


            var pocetSchuzi = ParsePSPWeb.PocetSchuzi(rok);

            //find latest item already in DB

            var lastSchuzeInDb = 1;

            if (rewrite == false)
            {
                try
                {
                    var last = dsc.SearchItemsInDataset<Steno>(dsDef.DatasetId, $"obdobi:{rok}", 1, "schuze", true)
                        .Result.results.FirstOrDefault();
                    lastSchuzeInDb = last?.schuze ?? 1;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                }
            }

            if (schuze.HasValue)
            {
                lastSchuzeInDb = schuze.Value;
                pocetSchuzi = schuze.Value;
            }

            List<int> schuzeToParse = new List<int>();
            for (int s = lastSchuzeInDb; s <= pocetSchuzi; s++)
                schuzeToParse.Add(s);

            Devmasters.Core.Batch.Manager.DoActionForAll<int>(schuzeToParse,
                s =>
            {
                foreach (var item in ParsePSPWeb.ParseSchuze(rok, s))
                {
                    try
                    {
                        if (rewrite == false)
                        {
                            var exists = dsc.GetItemFromDataset<Steno>(dsDef.DatasetId, item.Id).Result;
                            continue; //exists, skip
                        }
                    }
                    catch (Exception) //doesnt exists
                    {
                    }

                    if (item.celeJmeno?.Split(' ')?.Count() > 2)
                        if (!jmena2Check.Contains(item.celeJmeno))
                            jmena2Check.Add(item.celeJmeno);

                    using (var net = new Devmasters.Net.Web.URLContent($"https://www.hlidacstatu.cz/api/v1/PoliticiFromText?Authorization={apikey}"))
                    {
                        net.Method = Devmasters.Net.Web.MethodEnum.POST;
                        net.RequestParams.Form.Add("text", item.text);
                        net.Timeout = 60 * 1000;
                        var sosoby = net.GetContent().Text;
                        var osoby = Newtonsoft.Json.Linq.JArray.Parse(sosoby);
                        if (osoby != null && osoby.Count > 0)
                        {
                            item.politiciZminky = osoby
                                .Select(ja => ja.Value<string>("osobaid"))
                                .Where(o => !string.IsNullOrWhiteSpace(o))
                                .ToArray();
                        }
                    }


                    if (apikey == "csv")
                    {
                        csv.WriteRecord<Steno>(item);
                        csv.NextRecord();
                        if (item.poradi % 10 == 0)
                            csv.Flush();
                    }
                    else
                        SaveItem(dsDef, item, true);
                }

                return new Devmasters.Core.Batch.ActionOutputData();
            }, true);

            if (apikey == "csv")
            {
                csv.Flush();
                csv.Dispose();
                reader.Close();
            }


            Console.WriteLine();
            Console.WriteLine("Podezrela jmena:");
            foreach (var k in jmena2Check)
                Console.WriteLine(k);

            return;


            //download, parse and save data into dataset
            //GetData(dsDef, datasetid, fn);
        }
        public static string GetExecutingDirectoryName(bool endsWithBackslash)
        {
            var location = new Uri(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase);
            var dir = new FileInfo(location.AbsolutePath).Directory.FullName + (endsWithBackslash ? @"\" : "");
            return dir;
        }



        [Obsolete()]
        static void _getData(Dataset<Steno> ds, string datasetId, string fn, bool rewrite = false)
        {



            int num = 0;
            int skip = 0;
            using (var reader = new StreamReader(fn))
            {
                using (var csv = new CsvReader(reader,
                    new CsvHelper.Configuration.Configuration()
                    {
                        HasHeaderRecord = true,
                        Delimiter = ","
                    }
                        )
                    )
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        //csv rok,datum,schuze,fn,autor,funkce,tema,text
                        var item = new Steno()
                        {
                            obdobi = csv.GetField<int>("rok"),
                            datum = null,
                            schuze = csv.GetField<int>("schuze"),
                            url = csv.GetField<string>("fn"),
                            celeJmeno = csv.GetField<string>("autor"),
                            funkce = csv.GetField<string>("funkce"),
                            tema = csv.GetField<string>("tema"),
                            text = csv.GetField<string>("text"),
                        };

                        if (rewrite == false && num == 0) //first line
                        {
                            //find latest item already in DB
                            var last = dsc.SearchItemsInDataset<Steno>(ds.DatasetId, $"rok_zahajeni_vo:{item.obdobi}", 1, "DbCreated", true)
                                .Result.results.FirstOrDefault();

                            skip = last?.poradi ?? 0;

                        }
                        num++;
                        if (num < skip - 10)
                            continue;


                        var osobaId = findInHS(item.celeJmeno, item.funkce);

                        item.OsobaId = osobaId;

                        //if (num % 50 == 0)
                        //    Console.WriteLine($"{fn} - {num} records");

                        int tries = 0;
                    AddAgain:
                        try
                        {
                            tries++;
                            var id = dsc.AddItemToDataset<Steno>(ds, item, DatasetConnector.AddItemMode.Rewrite).Result;
                            Console.WriteLine(id);

                        }
                        catch (Exception e)
                        {
                            if (tries < 300)
                            {
                                Console.Write(".");
                                System.Threading.Thread.Sleep(10 * 1000);
                                goto AddAgain;
                            }
                            else
                                Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }

        static void SaveItem(Dataset<Steno> ds, Steno item, bool loadOsobaId)
        {

            if (string.IsNullOrEmpty(item.OsobaId) && loadOsobaId)
            {
                var osobaId = findInHS(item.celeJmeno, item.funkce);

                item.OsobaId = osobaId;
            }

            int tries = 0;
        AddAgain:
            try
            {
                tries++;
                var id = dsc.AddItemToDataset<Steno>(ds, item, DatasetConnector.AddItemMode.Rewrite).Result;
                Console.Write("s");

            }
            catch (Exception e)
            {
                if (tries < 300)
                {
                    Console.Write("S");
                    System.Threading.Thread.Sleep(10 * 1000);
                    goto AddAgain;
                }
                else
                    Console.WriteLine(e.Message);
            }

        }

        public static string findInHS(string fullname, string fce)
        {
            //using (var net = new System.Net.WebClient())
            //{
            //    net.Encoding = System.Text.Encoding.UTF8;
            //    string url = $"https://www.hlidacstatu.cz/api/v1/FindOsobaId?Authorization={apikey}&"
            //        + $"celejmeno={System.Net.WebUtility.UrlEncode(fullname)}&funkce={System.Net.WebUtility.UrlEncode(fce)}";
            //    var json = net.DownloadString(url);
            //    return Newtonsoft.Json.Linq.JObject.Parse(json).Value<string>("OsobaId");
            //}
            using (var net = new Devmasters.Net.Web.URLContent($"https://www.hlidacstatu.cz/api/v1/PolitikFromText?Authorization={apikey}"))
            {
                net.Method = Devmasters.Net.Web.MethodEnum.POST;
                net.RequestParams.Form.Add("text", $"{fullname} {fce}");
                net.Timeout = 60 * 1000;
                var sosoba = net.GetContent().Text;
                var osoba = Newtonsoft.Json.Linq.JObject.Parse(sosoba);
                return osoba.Value<string>("osobaid");
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
                    return match.Groups[groupname].Value;
                }
            }
            return string.Empty;
        }


        private static Dataset<Steno> Ds()
        {
            return new HlidacStatu.Api.Dataset.Connector.Dataset<Steno>(
                    "Stenozáznamy Poslanecké sněmovny Parlamentu ČR", "stenozaznamy-psp", "http://www.psp.cz", "Stenozáznamy (těsnopisecké záznamy) jednání Poslanecké sněmovny a jejích orgánů. S využitím Open dat knihovny Ondřeje Kokeše https://github.com/kokes/od/tree/master/data/psp/steno",
                    "https://github.com/HlidacStatu/Datasety/tree/master/StenozaznamyPSP",
                    true, true,
                    new string[,] {
                        { "Podle data konání", "datum" },
                        { "Podle osoby", "celeJmeno.keyword" },
                        { "Podle délky projevu", "pocetSlov" },
                        { "Podle pořadí projevu při schůzi", "Id.keyword" },
                    },
                    new Template()
                    {
                        Body = @"
<!-- scriban {{ date.now }} -->   
  <table class=""table table-hover"">                                                                                                                                                                                                                                
    <thead>      
      <tr>                                                                                                                                                                                                                                                                                                                                                                                                                
        <th>Id        
        </th>
        <th>Datum        
        </th>
        <th>Schůze        
        </th>
        <th>Osoba                
        </th>
        <th>Téma                
        </th>
        <th>Délka projevu                
        </th>
        <th>Politici zmínění v projevu                
        </th>
      </tr>
    </thead>
    <tbody>
      {{ for item in model.Result }}                                                                                                                                                                                                                                                                                                      
      <tr>                
        <td >                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
          <a href=""{{ fn_DatasetItemUrl item.Id }}"">Detail                    
          </a>
        </td>
        <td >{{ fn_FormatDate item.datum }}                                                                                                                                                                                                                                                                                                                                                                                                        
        </td>
        <td>                                                                                                                                                                                                                                                                                                                                                                        
          <a href=""/data/Hledat/stenozaznamy-psp?Q=obdobi%3A{{ item.obdobi }}%20AND%20schuze%3A{{ item.schuze}}&order=Id.keyword%20asc"">Schůze
            {{ item.schuze }}/{{ item.obdobi}}                                                                                                                                                                                                                                                                                                                                                                        
          </a>
        </td>
        <td style=""white-space: nowrap"">{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno """" }}                                                                                                                        
        </td>
        <td >{{ fn_ShortenText item.tema 50 }}                                                                                                                                                                                                                                
        </td>
        <td>{{  string.to_long item.pocetSlov | math.divided_by 200 | fn_Pluralize ""do minuty"" ""minuta"" ""{0} minuty"" ""{0} minut"" }}                                                                                                                                                                                                                
        </td>
        <td>
          {{ if (item.politiciZminky && item.politiciZminky.size > 0) }}
          {{ fn_RenderPersonNoLink item.politiciZminky[0] }}
          {{ if item.politiciZminky.size > 1 }}{{ fn_Pluralize (item.politiciZminky.size-1) """" (""a "" + (fn_RenderPersonNoLink item.politiciZminky[1])) ""a {0} další politiky"" ""a {0} dalších politiků""  }}
          {{ end }}
          {{ end }}                                                                                                                                        
        </td>
      </tr>
      {{ end }}                                                                
    </tbody>
  </table>


"
                    },
                    new Template()
                    {
                        Body = @"
<!-- scriban {{ date.now }} -->
  {{this.item = model}}                                      
  <h2> Schůze
    {{ item.schuze }}/{{ item.obdobi}}                                                                  
    <small>{{ fn_FormatDate item.datum }} 
    </small>
  </h2>
  <h4>Téma                                                             
    <i>{{ item.tema }}
    </i>
  </h4>
  <div class=""panel panel-default"">                                                  
    <div class=""panel-heading"">
      <h3 class=""panel-title"">{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno """" }}
        {{if item.funkce }}({{ item.funkce }} )
        {{end}}(                                                                                        
        <i>délka {{  string.to_long item.pocetSlov | math.divided_by 200 | fn_Pluralize ""do minuty"" ""minuta"" ""{0} minuty"" ""{0} minut"" }}                
        </i>
)      
      </h3>
    </div>
    <div class=""panel-body"">                                                                             
      <pre style=""font-size:90%;background:none;line-height:1.6em;"">
        {{ fn_HighlightText highlightingData item.text ""text"" }}                                                                                                                                                                                                                                                                        
      </pre>
    </div>
    <div class=""panel-footer"">
      {{ if item.poradi > 1 }}                                                                                                                                                                                                            
      <a href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.minus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" >&lt;&lt; Předchozí projev                  
      </a>
      {{else}}&nbsp;{{ end }}                                      
      <a style=""padding-left:30px;"" href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.plus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" >Následující projev  &gt;&gt;                                                                                                                                                                                                                                               
      </a>
&nbsp;&nbsp;-&nbsp;&nbsp;                                          
      <a href=""/data/Hledat/stenozaznamy-psp?Q=obdobi%3A{{ item.obdobi }}%20AND%20schuze%3A{{ item.schuze}}&order=Id.keyword%20asc"">Všechny projevy na schůzi
        {{ item.schuze }}/{{ item.obdobi}}                                                                                                                                                                                                                                                                                                                                  
      </a>
    </div>
  </div>
  {{ if (item.politiciZminky && item.politiciZminky.size > 0) }}                      
  <p>V projevu zmínění politici:            
    <ul>{{for p in item.politiciZminky}}                                                                  
      <li>{{ fn_RenderPersonWithLink2 p }}       
(najít další zmínky                 
        <a href='/data/hledat/stenozaznamy-psp?Q=OsobaId.keyword%3A{{item.OsobaId}}+AND+politiciZminky.keyword%3A{{p}}'>
          {{ fn_RenderPersonNoLink item.OsobaId }} o
          {{ fn_RenderPersonNoLink p }}
        </a>
 a 
        <a href='/data/hledat/stenozaznamy-psp?Q=OsobaId.keyword%3A{{p}}+AND+politiciZminky.keyword%3A{{item.OsobaId}}'>naopak                
        </a>
)                                    
      </li>
      {{end}}                            
    </ul>
  </p>
  {{end}}            
  <p class=""text-muted"">Zdroj:                                                                                                                                                                 
    <a href=""{{ item.url }}"" target=""_blank"">{{ item.url }}                                                                                                                                                                                                    
    </a>
  </p>

"
                    }

                    );
        }


    }
}
