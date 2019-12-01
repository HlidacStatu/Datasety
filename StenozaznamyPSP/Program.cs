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

        static void Main(string[] args)
        {
            //Politici.InitPol();

            //var jsonGen = new JSchemaGenerator
            //{
            //    DefaultRequired = Required.Default
            //};
            //var JsonSchema = jsonGen.Generate(typeof(Steno)).ToString();


            //Parse.net.Encoding = System.Text.Encoding.GetEncoding("windows-1250");

            //read poslanci jmena
            jmena = System.IO.File.ReadAllLines("politici.tsv")
               .Select(m => m.Split('\t'))
               .Where(m => m.Length > 2)
               .SelectMany(m => {
                    var variants = new List<string[]>();
                    for (int l = 2; l < m.Length; l++)
                    {
                        variants.Add(new string[] { m[0].Trim(), m.Skip(1).Take(l).Aggregate((f, s) => f + " " + s) });
                    }
                    return variants;
               });



            if (args.Length < 2)
            {
                Console.WriteLine("StenozaznamyPSP {csv/API_KEY} {rok} [rewrite]");
                return;
            }

            apikey = args[0];
            int rok = Convert.ToInt32(args[1]);
            bool rewrite = false;
            if (args.Length == 3 && args[2].ToLower() == "rewrite")
                rewrite = true;

            dsc = new HlidacStatu.Api.Dataset.Connector.DatasetConnector(apikey);

            //create dataset
            var dsDef = new HlidacStatu.Api.Dataset.Connector.Dataset<Steno>(
                "Stenozáznamy Poslanecké sněmovny Parlamentu ČR", "stenozaznamy-psp", "http://www.psp.cz", "Stenozáznamy (těsnopisecké záznamy) jednání Poslanecké sněmovny a jejích orgánů. S využitím Open dat knihovny Ondřeje Kokeše https://github.com/kokes/od/tree/master/data/psp/steno",
                "https://github.com/HlidacStatu/Datasety/tree/master/StenozaznamyPSP",
                true, true,
                new string[,] {
                    { "Podle konání", "Id.keyword" },
                    { "Podle volebního období", "období" },
                    { "Podle osoby", "celeJmeno" },
                    { "Podle délky projevu", "pocetSlov" },
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
        <th>Délka projevu        
        </th>
        <th>Téma        
        </th>
      </tr>
    </thead>
    <tbody>
      {{ for item in model.Result }}                                                                                                                                                
      <tr>        
        <td >                                                                                                                                                                                                                                                
          <a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}          
          </a>
        </td>
        <td >{{ fn_FormatDate item.datum }}                                                                                                                                                                                                
        </td>
        <td>                                                                                                              
          <a href=""/data/Hledat/stenozaznamy-psp?Q=obdobi%3A{{ item.obdobi }}%20AND%20schuze%3A{{ item.schuze}}&order=Id.keyword%20asc"">Schůze
            {{ item.schuze }}/{{ item.obdobi}}                                                                                                              
          </a>
        </td>
        <td >{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno """" }}                                                                                                                
        </td>
        <td>{{  string.to_long item.pocetSlov | math.divided_by 200 | fn_Pluralize ""do minuty"" ""minuta"" ""{0} minuty"" ""{0} minut"" }}                                                                                        
        </td>
        <td >{{ fn_ShortenText item.tema 50 }}                                                                                                        
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
  <table class=""table table-hover"">
    <tbody>                                                                                                                                                      
      <tr>
        <td>Id                                                                                                                                                                                                        
        </td>
        <td >                                                                                                                                                                                                                                                          
          <a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}
          </a>
        </td>
      </tr>
      <tr>
        <td>Datum                                                                                                                                                                                                        
        </td>
        <td >{{ fn_FormatDate item.datum }}                                                                                                                                                                                                        
        </td>
      </tr>
      <tr>
        <td>Schůze                                                                                
        </td>
        <td>                                                                                                    
          <a href=""/data/Hledat/stenozaznamy-psp?Q=obdobi%3A{{ item.obdobi }}%20AND%20schuze%3A{{ item.schuze}}&order=Id.keyword%20asc"">Schůze
            {{ item.schuze }}/{{ item.obdobi}}                                                                                                    
          </a>
        </td>
      </tr>
      <tr>                                                                                
        <td>Osoba                                                                                                                        
        </td>
        <td >{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno """" }}                                                                                                                        
        </td>
      </tr>
      <tr>                                                                                
        <td>Funkce                                                                                                                        
        </td>
        <td >{{ item.funkce }}                                                                                                                        
        </td>
      </tr>
      <tr>                                                                                
        <td>Téma                                                                                                                        
        </td>
        <td >{{ item.tema }}                                                                                                                        
        </td>
      </tr>
      <tr>                                                                                
        <td>Délka projevu                                                                                                                        
        </td>
        <td >{{  string.to_long item.pocetSlov | math.divided_by 200 | fn_Pluralize ""do minuty"" ""minuta"" ""{0} minuty"" ""{0} minut"" }} ({{ item.pocetSlov |fn_Pluralize ""{0} slov"" ""{1} slovo"" ""{0} slova"" ""{0} slov"" }})                                                                                                                        
        </td>
      </tr>
      <tr>                                                                                
        <td>Vystoupení                                                                                                                        
        </td>
        <td >                                                                                                                                            
          <pre>{{ fn_HighlightText highlightingData item.text ""text"" }}                                                                                                    
          </pre>
        </td>
      </tr>
      <tr>                        
        <td colspan=""2"">
          {{ if item.poradi > 1 }}                                          
          <a href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.minus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" >&lt;&lt; Předchozí projev
          </a>
{{else}}
&nbsp;{{ end }}                              
          <a style=""padding-left:30px;"" href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.plus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" >Následující projev  &gt;&gt;                                                                   
          </a>
        </td>
      </tr>
      <tr>                                                
        <td>Zdroj                                                                 
        </td>
        <td >                                                            
          <a href=""{{ item.url }}"" target=""_blank"">{{ item.url }}                                                            
          </a>
        </td>
      </tr>
    </table>
"
                }

                );

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

            //int roky = new int[] { 2002 };// 2006 , 2002, 1998, 1996, 1993};
            if (apikey == "csv")
            {
                reader = new StreamWriter($"{rok}.csv");
                csv = new CsvWriter(reader,
                    new CsvHelper.Configuration.Configuration()
                    {
                        HasHeaderRecord = true,
                        Delimiter = ","
                    });
                csv.WriteHeader<Steno>();
                csv.NextRecord();
            }

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

            if (args.Length == 4)
            {
                lastSchuzeInDb = Convert.ToInt32(args[3]);
                pocetSchuzi = lastSchuzeInDb;
            }

            for (int s = lastSchuzeInDb; s <= pocetSchuzi; s++)
            {
                foreach (var item in ParsePSPWeb.ParseSchuze(rok, s))
                {
                    if (item.celeJmeno?.Split(' ')?.Count() > 2)
                        if (!jmena2Check.Contains(item.celeJmeno))
                            jmena2Check.Add(item.celeJmeno);

                    var politiciZminky = Politici.FindCitations(item.text);
                    item.politiciZminky = politiciZminky;


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
            }

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


                        var osobaId = fromJmeno(item.celeJmeno);
                        if (string.IsNullOrEmpty(osobaId))
                            osobaId = findInHS(item.celeJmeno, item.funkce);

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
                var osobaId = fromJmeno(item.celeJmeno);
                if (string.IsNullOrEmpty(osobaId))
                    osobaId = findInHS(item.celeJmeno, item.funkce);

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
            using (var net = new System.Net.WebClient())
            {
                net.Encoding = System.Text.Encoding.UTF8;
                string url = $"https://www.hlidacstatu.cz/api/v1/FindOsobaId?Authorization={apikey}&"
                    + $"celejmeno={System.Net.WebUtility.UrlEncode(fullname)}&funkce={System.Net.WebUtility.UrlEncode(fce)}";
                var json = net.DownloadString(url);
                return Newtonsoft.Json.Linq.JObject.Parse(json).Value<string>("OsobaId");
            }

        }

        public static string fromJmeno(string fullname)
        {
            if (string.IsNullOrEmpty(fullname))
                return fullname;
            //using (var net = new System.Net.WebClient())
            //{
            //    net.Encoding = System.Text.Encoding.UTF8;
            //    string url = $"https://www.hlidacstatu.cz/api/v1/FindOsobaId?Authorization={apikey}&"
            //        + $"celejmeno={System.Net.WebUtility.UrlEncode(fullname)}&funkce={System.Net.WebUtility.UrlEncode(fce)}";
            //    var json = net.DownloadString(url);
            //    return Newtonsoft.Json.Linq.JObject.Parse(json).Value<string>("OsobaId");
            //}
            fullname = fullname.ToLower();
            var found = jmena.Where(m => m[1] == fullname.Trim()).FirstOrDefault();
            if (found == null)
                return null;
            return found[0];
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

        static IEnumerable<string[]> jmena = null;

    }
}
