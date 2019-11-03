using CsvHelper;
using HlidacStatu.Api.Dataset.Connector;
using Newtonsoft.Json.Linq;
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

            //Parse.net.Encoding = System.Text.Encoding.GetEncoding("windows-1250");

            //read poslanci jmena
            jmena = poslanci
               .Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
               .Select(m => m.Split('\t'))
               .Where(m => m.Length == 3)
               .Select(m => new string[] { m[0].Trim() + " " + m[1].Trim(), m[2].Trim() });



            if (args.Length < 2)
            {
                Console.WriteLine("StenozaznamyPSP {csv/API_KEY} {rok} [rewrite]")
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
                new Template() { 
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
        <td>{{  string.to_long item.pocetSlov | math.divided_by 200 | fn_Pluralize ""pár sekund"" ""minuta"" ""{0} minuty"" ""{0} minut"" }}                                                                                        
        </td>
        <td >{{ fn_ShortenText item.tema 50 }}                                                                                                        
        </td>
      </tr>
      {{ end }}                                        
    </tbody>
  </table>
" },
                new Template() { 
                Body= @"
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
          <a href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.minus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" target=""_blank"">&lt;&lt; Předchozí projev
          </a>
{{else}}
&nbsp;{{ end }}          
          <a style=""padding-left:30px;"" href=""{{ item.obdobi + ""_""+item.schuze + ""_"" + (string.to_long item.poradi | math.plus 1 | object.format ""00000"" ) | fn_DatasetItemUrl  }}"" target=""_blank"">Následující projev  &gt;&gt;                                                                   
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
"}

                //new ClassicTemplate.ClassicSearchResultTemplate()
                //    .AddColumn("Id", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}</a>")
                //    .AddColumn("Datum", "{{ fn_FormatDate item.datum }}")
                //    .AddColumn("Osoba", "{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno \"\" }}")
                //    .AddColumn("Téma", "{{ fn_ShortenText item.tema 50 }}")
                //,
                //new ClassicTemplate.ClassicDetailTemplate()
                //    .AddColumn("Id", @"<a href=""{{ fn_DatasetItemUrl item.Id }}"">{{ item.Id }}</a>")
                //    .AddColumn("Datum", "{{ fn_FormatDate item.datum }}")
                //    .AddColumn("Osoba", "{{ fn_RenderPersonWithLink item.OsobaId item.celeJmeno \"\" }}")
                //    .AddColumn("Funkce", "{{ item.funkce }}")
                //    .AddColumn("Téma", "{{ item.tema }}")
                //    .AddColumn("Vystoupení", "{{ fn_HighlightText highlightingData item.text \"text\" }}")
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
            { try
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

            for (int s = lastSchuzeInDb; s <= pocetSchuzi; s++)
            {
                foreach (var item in ParsePSPWeb.ParseSchuze(rok, s))
                {
                    if (item.celeJmeno?.Split(' ')?.Count() > 2)
                        if (!jmena2Check.Contains(item.celeJmeno))
                            jmena2Check.Add(item.celeJmeno);

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


        static void GetData(Dataset<Steno> ds, string datasetId, string fn, bool rewrite = false)
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

            var found = jmena.Where(m => m[0] == fullname.Trim()).FirstOrDefault();
            if (found == null)
                return null;
            return found[1];
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
        static string poslanci = @"
Adam	Rykala	adam-rykala
Adam	Vojtěch	adam-vojtech-1
Adolf	Beznoska	adolf-beznoska
Alena	Hanáková	alena-hanakova
Alena	Nohavová	alena-nohavova
Aleš	Rádl	ales-radl
Aleš	Řebíček	ales-rebicek
Aleš	Rozehnal	ales-rozehnal
Aleš	Roztočil	ales-roztocil
Alexander	Černý	alexander-cerny
Alfréd	Michalík	alfred-michalik
Andrej	Babiš	andrej-babis
Anna	Putnová	anna-putnova
Antonín	Seďa	antonin-seda
Augustin Karel	Andrle Sylor	augustin-karel-andrle-sylor
Bohuslav	Chalupa	bohuslav-chalupa
Bohuslav	Sobotka	bohuslav-sobotka
Bohuslav	Svoboda	bohuslav-svoboda
Boris	Šťastný	boris-stastny
Bořivoj	Šarapatka	borivoj-sarapatka
Břetislav	Petr	bretislav-petr
Bronislav	Schwarz	bronislav-schwarz
Cyril	Svoboda	cyril-svoboda
Cyril	Zapletal	cyril-zapletal
Dagmar	Molendová	dagmar-molendova
Dagmar	Navrátilová	dagmar-navratilova
Dalibor	Matulka	dalibor-matulka
Dan	Ťok	dan-tok
Dana	Filipi	dana-filipi
Dana	Váhalová	dana-vahalova
Daniel	Herman	daniel-herman
Daniel	Korte	daniel-korte
Daniel	Petruška	daniel-petruska
Daniel	Reisiegel	daniel-reisiegel
Daniel	Rovan	daniel-rovan
David	Kádner	david-kadner
David	Kafka	david-kafka
David	Kasal	david-kasal
David	Rath	david-rath
David	Šeich	david-seich
David	Vodrážka	david-vodrazka
Dominik	Feri	dominik-feri
Evžen	Snítilý	evzen-snitily
František	Adámek	frantisek-adamek
František	Bublan	frantisek-bublan
František	Dědič	frantisek-dedic
František	Koníček	frantisek-konicek
František	Laudát	frantisek-laudat
František	Novosad	frantisek-novosad
František	Pejřil	frantisek-pejril
František	Pelc	frantisek-pelc
František	Petrtýl	frantisek-petrtyl
František	Sivera	frantisek-sivera
František	Vácha	frantisek-vacha
Gabriela	Hubáčková	gabriela-hubackova
Gabriela	Kalábková	gabriela-kalabkova
Gabriela	Pecková	gabriela-peckova
Hana	Aulická Jírovcová	hana-aulicka-jirovcova
Hana	Orgoníková	hana-orgonikova
Helena	Langšádlová	helena-langsadlova
Helena	Válková	helena-valkova
Herbert	Pavera	herbert-pavera
Hynek	Fajmon	hynek-fajmon
Igor	Jakubčík	igor-jakubcik
Igor	Nykl	igor-nykl
Igor	Svoják	igor-svojak
Ivan	Adamec	ivan-adamec
Ivan	Fuksa	ivan-fuksa
Ivan	Gabal	ivan-gabal
Ivan	Langer	ivan-langer
Ivan	Ohlídal	ivan-ohlidal
Ivan	Pilný	ivan-pilny
Ivana	Dobešová	ivana-dobesova
Ivana	Levá	ivana-leva
Ivana	Řápková	ivana-rapkova
Ivana	Weberová	ivana-weberova
Ivo	Pojezný	ivo-pojezny
Jan	Babor	jan-babor
Jan	Bartošek	jan-bartosek
Jan	Bauer	jan-bauer
Jan	Birke	jan-birke
Jan	Bureš	jan-bures
Jan	Bürgermeister	jan-burgermeister
Jan	Čechlovský	jan-cechlovsky
Jan	Chvojka	jan-chvojka
Jan	Farský	jan-farsky
Jan	Florián	jan-florian
Jan	Hamáček	jan-hamacek
Jan	Husák	jan-husak
Jan	Kalvoda	jan-kalvoda
Jan	Kasal	jan-kasal
Jan	Klán	jan-klan
Jan	Kostrhun	jan-kostrhun
Jan	Kubata	jan-kubata
Jan	Látka	jan-latka
Jan	Mládek	jan-mladek
Jan	Morava	jan-morava
Jan	Pajer	jan-pajer
Jan	Sedláček	jan-sedlacek
Jan	Skopeček	jan-skopecek
Jan	Smutný	jan-smutny
Jan	Sobotka	jan-sobotka
Jan	Špika	jan-spika
Jan	Vidím	jan-vidim
Jan	Volný	jan-volny
Jan	Zahradil	jan-zahradil
Jan	Zahradník	jan-zahradnik
Jana	Černochová	jana-cernochova
Jana	Drastichová	jana-drastichova
Jana	Fischerová	jana-fischerova
Jana	Hnyková	jana-hnykova
Jana	Kaslová	jana-kaslova
Jana	Lorencová	jana-lorencova
Jana	Pastuchová	jana-pastuchova
Jana	Rybínová	jana-rybinova
Jana	Suchá	jana-sucha
Jaromír	Chalupa	jaromir-chalupa
Jaromír	Drábek	jaromir-drabek
Jaromír	Kohlíček	jaromir-kohlicek
Jaromír	Schling	jaromir-schling
Jaromír	Talíř	jaromir-talir
Jaroslav	Borka	jaroslav-borka
Jaroslav	Eček	jaroslav-ecek
Jaroslav	Faltýnek	jaroslav-faltynek
Jaroslav	Fiala	jaroslav-fiala
Jaroslav	Foldyna	jaroslav-foldyna
Jaroslav	Holík	jaroslav-holik
Jaroslav	Klaška	jaroslav-klaska
Jaroslav	Krákora	jaroslav-krakora
Jaroslav	Krupka	jaroslav-krupka
Jaroslav	Lobkowicz	jaroslav-lobkowicz
Jaroslav	Martinů	jaroslav-martinu
Jaroslav	Palas	jaroslav-palas
Jaroslav	Plachý	jaroslav-plachy
Jaroslav	Škárka	jaroslav-skarka
Jaroslav	Tvrdík	jaroslav-tvrdik
Jaroslav	Vandas	jaroslav-vandas
Jaroslav	Zavadil	jaroslav-zavadil
Jaroslav	Zvěřina	jaroslav-zverina
Jaroslava	Pokorná Jermanová	jaroslava-pokorna-jermanova-1
Jaroslava	Jermanová	jaroslava-pokorna-jermanova-1
Jaroslava	Schejbalová	jaroslava-schejbalova
Jaroslava	Wenigerová	jaroslava-wenigerova
Jeroným	Tejc	jeronym-tejc
Jiří	Běhounek	jiri-behounek
Jiří	Besser	jiri-besser
Jiří	Carbol	jiri-carbol
Jiří	Dienstbier	jiri-dienstbier
Jiří	Dolejš	jiri-dolejs
Jiří	Hlavatý	jiri-hlavaty-23
Jiří	Holeček	jiri-holecek
Jiří	Janeček	jiri-janecek
Jiří	Junek	jiri-junek
Jiří	Koskuba	jiri-koskuba
Jiří	Koubek	jiri-koubek
Jiří	Krátký	jiri-kratky
Jiří	Maštálka	jiri-mastalka
Jiří	Mihola	jiri-mihola
Jiří	Oliva	jiri-oliva
Jiří	Papež	jiri-papez
Jiří	Paroubek	jiri-paroubek
Jiří	Payne	jiri-payne
Jiří	Petrů	jiri-petru
Jiří	Pospíšil	jiri-pospisil-1
Jiří	Rusnok	jiri-rusnok
Jiří	Rusnok	jiri-rusnok-1
Jiří	Skalický	jiri-skalicky-1
Jiří	Šlégr	jiri-slegr
Jiří	Štětina	jiri-stetina
Jiří	Šulc	jiri-sulc
Jiří	Valenta	jiri-valenta
Jiří	Zemánek	jiri-zemanek
Jiří	Zimola	jiri-zimola
Jiří	Zlatuška	jiri-zlatuska
Jiřina	Fialová	jirina-fialova-4
Jitka	Chalánková	jitka-chalankova
Josef	Cogan	josef-cogan
Josef	Dobeš	josef-dobes
Josef	Hájek	josef-hajek
Josef	Kott	josef-kott
Josef	Nekl	josef-nekl
Josef	Novotný Ml.	josef-novotny-ml.
Josef	Novotný St.	josef-novotny-st.
Josef	Rihák	josef-rihak-7
Josef	Šenfeld	josef-senfeld
Josef	Smýkal	josef-smykal
Josef	Tancoš	josef-tancos
Josef	Uhlík	josef-uhlik
Josef	Vondrášek	josef-vondrasek
Josef	Vondruška	josef-vondruska
Josef	Vozdecký	josef-vozdecky
Josef	Zahradníček	josef-zahradnicek
Jozef	Kochan	jozef-kochan
Karel	Černý	karel-cerny
Karel	Fiedler	karel-fiedler
Karel	Korytář	karel-korytar
Karel	Kratochvíle	karel-kratochvile
Karel	Pražák	karel-prazak
Karel	Rais	karel-rais
Karel	Schwarzenberg	karel-schwarzenberg
Karel	Šidlo	karel-sidlo
Karel	Tureček	karel-turecek
Karla	Šlechtová	karla-slechtova
Karolína	Peake	karolina-peake
Kateřina	Klasnová	
Kateřina	Konečná	katerina-konecna
Klára	Dostálová	klara-dostalova
Kristýna	Kočí	kristyna-koci
Kristýna	Zelienková	kristyna-zelienkova
Květa	Matušovská	kveta-matusovska
Ladislav	Jirků	ladislav-jirku
Ladislav	Okleštěk	ladislav-oklestek
Ladislav	Šincl	ladislav-sincl
Ladislav	Skopal	ladislav-skopal
Ladislav	Velebný	ladislav-velebny
Lenka	Andrýsová	lenka-andrysova
Lenka	Kohoutová	lenka-kohoutova
Lenka	Mazuchová	lenka-mazuchova
Leo	Luzar	leo-luzar
Leoš	Heger	leos-heger
Libor	Ambrozek	libor-ambrozek
Libor	Nowak	libor-nowak
Libor	Rouček	libor-roucek
Lubomír	Toufar	lubomir-toufar
Lubomír	Zaorálek	lubomir-zaoralek
Luděk	Jeništa	ludek-jenista
Ludmila	Bubeníková	ludmila-bubenikova
Ludmila	Müllerová	ludmila-mullerova
Ludmila	Navrátilová	ludmila-navratilova
Ludvík	Hovorka	ludvik-hovorka
Lukáš	Pleticha	lukas-pleticha
Marek	Benda	marek-benda
Marek	Černoch	marek-cernoch
Marek	Šnajdr	marek-snajdr
Marek	Ženíšek	marek-zenisek
Margita	Balaštíková	margita-balastikova
Marian	Jurečka	marian-jurecka
Marie	Benešová	marie-benesova
Marie	Nedvědová	marie-nedvedova
Marie	Pěnčíková	marie-pencikova
Marie	Rusová	marie-rusova
Markéta	Adamová	marketa-adamova
Markéta	Pekarová Adamová	marketa-adamova
Markéta	Wernerová	marketa-wernerova
Marta	Bayerová	marta-bayerova
Marta	Semelová	marta-semelova
Martin	Gregora	martin-gregora
Martin	Kocourek	martin-kocourek
Martin	Kolovratník	martin-kolovratnik
Martin	Komárek	martin-komarek
Martin	Lank	martin-lank
Martin	Mečíř	martin-mecir
Martin	Novotný	martin-novotny
Martin	Pecina	martin-pecina
Martin	Plíšek	martin-plisek
Martin	Říman	martin-riman
Martin	Sedlář	martin-sedlar
Martin	Starec	martin-starec
Martin	Stropnický	martin-stropnicky
Martin	Tesařík	martin-tesarik
Martin	Vacek	martin-vacek
Martina	Berdychová	martina-berdychova
Matěj	Fichtner	matej-fichtner
Michael	Hrbata	michael-hrbata
Michaela	Šojdrová	michaela-sojdrova
Michal	Babák	michal-babak
Michal	Doktor	michal-doktor
Michal	Hašek	michal-hasek
Michal	Janek	michal-janek
Michal	Kraus	michal-kraus
Michal	Kučera	michal-kucera
Michal	Pohanka	michal-pohanka
Milada	Emmerová	milada-emmerova
Milada	Halíková	milada-halikova
Milan	Brázdil	milan-brazdil
Milan	Cabrnoch	milan-cabrnoch
Milan	Chovanec	milan-chovanec
Milan	Šarapatka	milan-sarapatka
Milan	Šťovíček	milan-stovicek
Milan	Urban	milan-urban
Miloš	Babiš	milos-babis
Miloš	Kužvart	milos-kuzvart
Miloš	Melčák	milos-melcak
Miloš	Patera	milos-patera
Miloš	Petera	milos-petera
Miloš	Zeman	milos-zeman
Miloslav	Bačiak	miloslav-baciak
Miloslav	Janulík	miloslav-janulik
Miloslav	Kala	miloslav-kala
Miloslav	Ransdorf	
Miloslav	Vlček	miloslav-vlcek
Miloslava	Rutová	miloslava-rutova
Miloslava	Vostrá	miloslava-vostra
Mirek	Topolánek	mirek-topolanek
Miroslav	Bernášek	miroslav-bernasek
Miroslav	Grebeníček	miroslav-grebenicek
Miroslav	Jeník	miroslav-jenik
Miroslav	Kalousek	miroslav-kalousek
Miroslav	Opálka	miroslav-opalka
Miroslav	Ouzký	miroslav-ouzky
Miroslav	Petráň	miroslav-petran
Miroslav	Svoboda	miroslav-svoboda
Miroslav	Váňa	miroslav-vana-2
Miroslava	Němcová	miroslava-nemcova
Miroslava	Strnadlová	miroslava-strnadlova
Nina	Nováková	nina-novakova
Oldřich	Vojíř	oldrich-vojir
Olga	Havlová	olga-havlova
Ondrej	Benešík	ondrej-benesik-1
Ondřej	Liška	ondrej-liska
Otto	Chaloupka	otto-chaloupka
Patricie	Kotalíková	patricie-kotalikova
Pavel	Antonín	pavel-antonin
Pavel	Bělobrádek	pavel-belobradek
Pavel	Bém	pavel-bem
Pavel	Blažek	pavel-blazek
Pavel	Bohatec	pavel-bohatec
Pavel	Čihák	pavel-cihak
Pavel	Drobil	pavel-drobil
Pavel	Havíř	pavel-havir
Pavel	Hojda	pavel-hojda
Pavel	Holík	pavel-holik
Pavel	Kováčik	pavel-kovacik
Pavel	Němec	pavel-nemec
Pavel	Ploc	pavel-ploc
Pavel	Plzák	pavel-plzak
Pavel	Severa	pavel-severa
Pavel	Šrámek	pavel-sramek
Pavel	Staněk	pavel-stanek
Pavel	Suchánek	pavel-suchanek
Pavel	Svoboda	pavel-svoboda-1
Pavel	Volčík	pavel-volcik
Pavla	Golasowská	pavla-golasowska
Pavlína	Nytrová	pavlina-nytrova
Pavol	Lukša	pavol-luksa
Petr	Adam	petr-adam
Petr	Benda	petr-benda
Petr	Bendl	petr-bendl
Petr	Braný	petr-brany
Petr	Bratský	petr-bratsky
Petr	Fiala	petr-fiala
Petr	Gandalovič	petr-gandalovic
Petr	Gazdík	petr-gazdik
Petr	Holeček	petr-holecek
Petr	Hulinský	petr-hulinsky
Petr	Jalowiczor	petr-jalowiczor
Petr	Kořenek	petr-korenek
Petr	Kudela	petr-kudela
Petr	Lachnit	petr-lachnit
Petr	Nečas	petr-necas
Petr	Rafaj	petr-rafaj
Petr	Šimůnek	petr-simunek
Petr	Skokan	petr-skokan
Petr	Sunkovský	petr-sunkovsky
Petr	Tluchoř	petr-tluchor
Petr	Zgarba	petr-zgarba
Přemysl	Rabas	premysl-rabas
Radek	John	radek-john
Radek	Vondráček	radek-vondracek
Radim	Chytka	radim-chytka
Radim	Fiala	radim-fiala
Radim	Holeček	radim-holecek
Radim	Jirout	radim-jirout
Radim	Vysloužil	radim-vyslouzil
Radka	Maxová	radka-maxova
Radko	Martínek	radko-martinek
Renáta	Witoszová	renata-witoszova
René	Číp	rene-cip
Richard	Brabec	richard-brabec
Richard	Dolejš	richard-dolejs
Robert	Dušek	robert-dusek
Robert	Pelikán	robert-pelikan
Robin	Böhnisch	robin-bohnisch
Rom	Kostřica	rom-kostrica
Roman	Kubíček	roman-kubicek
Roman	Pekárek	roman-pekarek
Roman	Procházka	roman-prochazka
Roman	Sklenák	roman-sklenak
Roman	Váňa	roman-vana
Rostislav	Vyzula	rostislav-vyzula
Rudolf	Chlad	rudolf-chlad
Simeon	Karamazov	simeon-karamazov
Soňa	Marková	sona-markova
Stanislav	Berkovec	stanislav-berkovec
Stanislav	Grospič	stanislav-grospic
Stanislav	Gross	stanislav-gross
Stanislav	Huml	stanislav-huml
Stanislav	Křeček	stanislav-krecek
Stanislav	Mackovík	stanislav-mackovik
Stanislav	Pfléger	stanislav-pfleger
Stanislav	Polčák	stanislav-polcak
Štěpán	Stupčuk	stepan-stupcuk
Štěpánka	Fraňková	stepanka-frankova
Svatomír	Recman	svatomir-recman
Táňa	Fischerová	tana-fischerova
Tomáš	Chalupa	tomas-chalupa
Tomáš	Hasil	tomas-hasil
Tomáš	Kladívko	tomas-kladivko
Tomáš	Úlehla	tomas-ulehla
Tomáš Jan	Podivínský	tomas-jan-podivinsky
Tomio	Okamura	tomio-okamura
Uhde	Milan	
Václav	Baštýř	vaclav-bastyr
Václav	Cempírek	vaclav-cempirek
Václav	Horáček	vaclav-horacek
Václav	Klaus	vaclav-klaus
Václav	Klaus	vaclav-klaus
Václav	Klučka	vaclav-klucka
Václav	Koubík	vaclav-koubik
Václav	Krása	vaclav-krasa
Václav	Kubata	vaclav-kubata
Václav	Mencl	vaclav-mencl
Václav	Neubauer	vaclav-neubauer
Václav	Šlajs	vaclav-slajs
Václav	Snopek	vaclav-snopek-1
Václav	Votava	vaclav-votava
Václav	Zemek	vaclav-zemek
Věra	Jakubková	vera-jakubkova
Věra	Jourová	vera-jourova
Věra	Kovářová	vera-kovarova
Viktor	Paggio	viktor-paggio
Vít	Bárta	vit-barta
Vít	Kaňkovský	vit-kankovsky
Vít	Němeček	vit-nemecek
Vítězslav	Jandák	vitezslav-jandak
Vladimír	Dlouhý	vladimir-dlouhy
Vladimír	Koníček	vladimir-konicek
Vladimír	Špidla	vladimir-spidla
Vladimíra	Lesenská	vladimira-lesenska
Vladislav	Vilímec	vladislav-vilimec
Vlasta	Bohdalová	vlasta-bohdalova
Vlasta	Parkanová	vlasta-parkanova
Vlastimil	Gabrhel	vlastimil-gabrhel
Vlastimil	Tlustý	vlastimil-tlusty
Vlastimil	Vozka	vlastimil-vozka
Vojtěch	Adam	vojtech-adam
Vojtěch	Filip	vojtech-filip
Walter	Bartoš	walter-bartos
Yvona	Kubjátová	yvona-kubjatova
Zbyněk	Stanjura	zbynek-stanjura
Zdeněk	Besta	zdenek-besta
Zdeněk	Bezecný	zdenek-bezecny
Zdeněk	Boháč	zdenek-bohac
Zdeněk	Mach	zdenek-mach
Zdeněk	Ondráček	zdenek-ondracek
Zdeněk	Škromach	zdenek-skromach
Zdeněk	Soukup	zdenek-soukup
Zdeněk	Syblík	zdenek-syblik
Zdeňka	Horníková	zdenka-hornikova
Zuzana	Brzobohatá	zuzana-brzobohata
Zuzana	Kailová	zuzana-kailova
Zuzana	Šánová	zuzana-sanova
Zuzka	Bebarová-Rujbrová	zuzka-bebarova-rujbrova
Zuzka	Bebarová Rujbrová	zuzka-bebarova-rujbrova
Jana	Mračková Vildumetzová	jana-vildumetzova
Jana	Vildumetzová	jana-vildumetzova
Zuzana	Majerová Zahradníková	zuzana-majerova-zahradnikova
Hana	Kordová Marvanová	hana-marvanova
Hana	Marvanová	hana-marvanova";
    }
}
