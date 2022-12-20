using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using HlidacStatu.Api.V2.CoreApi.Client;

namespace Tiskove_konference_vlady_CR
{
    class Program
    {
        static HlidacStatu.Api.V2.Dataset.Typed.Dataset<zapis> dsc;
        public static Dictionary<string, string> args = new Dictionary<string, string>();
        public static string startUrl = "https://www.vlada.cz/scripts/detail.php?pgid=218&conn=1339&pg={0}";

        public static string apiKey = "";
        static void Main(string[] arguments)
        {

            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            if (args.ContainsKey("/?") || args.ContainsKey("/h") || args.ContainsKey("/apikey") == false)
            {
                Console.WriteLine("Tiskove konference vlady downloader");
                Console.WriteLine("[/h] [/daysback=] [/from=yyyy-MM-dd] /apikey=xxxyyy");
                return;
            }
            apiKey = args["/apikey"];

            if (args.ContainsKey("/debug"))
                Parse.parallel = false;

            DateTime? from = null;
            if (args.ContainsKey("/daysback"))
            {
                var db = int.Parse(args["/daysback"]);
                from = DateTime.Now.Date.AddDays(-1 * db);
            }
            if (args.ContainsKey("/from"))
                from = DateTime.ParseExact(args["/from"], "yyyy-MM-dd", System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.AssumeLocal);

            //create dataset
            var jsonGen = new JSchemaGenerator
            {
                DefaultRequired = Required.Default
            };
            var genJsonSchema = jsonGen.Generate(typeof(zapis)).ToString();
            HlidacStatu.Api.V2.CoreApi.Model.Registration dsDef = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
                "Tiskové konference vlády ČR", Parse.datasetname, startUrl, "Zápisy z tiskových konferencí vlády ČR.",
                "https://github.com/HlidacStatu/Datasety/tree/master/Tiskove-konference-vlady-CR",genJsonSchema,
                "michal@michalblaha.cz",DateTime.Now, false, false,false,
                new HlidacStatu.Api.V2.Dataset.ClassicTemplate.ClassicDetailTemplate() { Body = @"<table class='table table-hover' >
  <tbody>
    {{for item in model.Result }}           
    <tr>
      <td style='white-space: nowrap;'>                    
        <a href='{{fn_DatasetItemUrl item.Id}}' >Kompletní přepis
        </a>
      </td>
      <td style='white-space: nowrap;' >
        {{ fn_FormatDate item.datum 'd. MMMM yyy'}}                
      </td>
      <td  >
        {{item.nazev}}
      </td>
      <td  >
        {{item.nazev}}
      </td>
<td>{{fn_Pluralize item.vstupy 'přepis není dostupný' 'jeden vstup' '{0} vstupy' '{0} vstupů'  }}</td>
        <td>
            {{string.to_long item.celkovyPocetSlov | math.divided_by 200 | fn_Pluralize 'do minuty' 'minuta' '{0} minuty' '{0} minut' }}
        </td>
    </tr>
{{end}}
  </tbody>
</table>" },
                 new HlidacStatu.Api.V2.Dataset.ClassicTemplate.ClassicDetailTemplate() { Body = @"
{{this.item = model}}

Pripravuje se

" }, 
                 null, new string[,] { { "Datum jednání", "datum" } }

                );

            try
            {
                dsc = HlidacStatu.Api.V2.Dataset.Typed.Dataset<zapis>.OpenDataset(
                apiKey, Parse.datasetname
                    );

            }
            catch (ApiException e)
            {
                //api = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Record>.CreateDataset(apikey, Registration());

            }
            catch (Exception e)
            {
                throw;
            }


            Parse.DownloadAllData(dsc, from);
        }


    }
}
