using HlidacStatu.Api.Dataset.Connector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tiskove_konference_vlady_CR
{
    class Program
    {
        static HlidacStatu.Api.Dataset.Connector.DatasetConnector dsc;
        public static Dictionary<string, string> args = new Dictionary<string, string>();
        public static string startUrl = "https://www.vlada.cz/scripts/detail.php?pgid=218&conn=1339&pg={0}";
        static void Main(string[] arguments)
        {
            dsc = new HlidacStatu.Api.Dataset.Connector.DatasetConnector(
                System.Configuration.ConfigurationManager.AppSettings["apikey"]
                );

            args = arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);

            if (args.ContainsKey("/debug"))
                Parse.parallel = false;

            DateTime? from = null;
            if (args.ContainsKey("/from"))
                from = DateTime.ParseExact(args["/from"], "yyyy-MM-dd", System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.AssumeLocal);

            //create dataset
            var dsDef = new HlidacStatu.Api.Dataset.Connector.Dataset<zapis>(
                "Tiskové konference vlády ČR", Parse.datasetname, startUrl, "Zápisy z tiskových konferencí vlády ČR.",
                "https://github.com/HlidacStatu/Datasety/tree/master/Tiskove-konference-vlady-CR",
                true, false,
                new string[,] { { "Datum jednání", "datum" } },
                new Template() { Body = @"<table class='table table-hover' >
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
                 new Template() { Body = @"
{{this.item = model}}

Pripravuje se

" }
                );

            //dsc.DeleteDataset(dsDef).Wait();
            if (!dsc.DatasetExists(dsDef).Result)
            {
                dsc.CreateDataset(dsDef).Wait();
            }

            Parse.DownloadAllData(dsc, from);
        }


    }
}
