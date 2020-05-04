using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HlidacStatu.Api.Dataset.Connector;

namespace Vybory_PSP
{
    class Program
    {
        static HlidacStatu.Api.Dataset.Connector.DatasetConnector dsc;
        public static Dictionary<string, string> args = new Dictionary<string, string>();

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

            int? from = null;
            if (args.ContainsKey("/from"))
                from = int.Parse(args["/from"]);

            //create dataset
            var dsDef = new HlidacStatu.Api.Dataset.Connector.Dataset<jednani>(
                "Jednání výborů PSP", Parse.datasetname, "https://www.psp.cz/sqw/hp.sqw?k=194", "Databáze neutajených jednání výborů Poslanecké sněmovny Parlamentu České republiky. Zápisy, audio záznamy, usnesení a další dokumenty, které nepodléhají režimu utajení.",
                "https://github.com/HlidacStatu/Datasety/tree/master/Vybory-PSP",
                true, true,
                new string[,] { { "Datum jednání", "datum" }, { "Výboru", "vybor" } },
                new Template() { Body= @"

<!-- scriban {{ date.now }} --> 
<table class='table table-hover'>
                        <thead>
                            <tr>
<th style='min-width:120px'>Jednání</th>
<th style='min-width:120px'>Výbor</th>
<th style='min-width:120px'>Datum jednání</th>
</tr>
</thead>
<tbody>
{{ for item in model.Result }}
<tr>
<td ><a href='{{ fn_DatasetItemUrl item.Id }}'>č. {{ item.cisloJednani }}</a></td>
<td >{{ item.vybor }}</td>
<td>{{ fn_FormatDate item.datum 'dd. MM. yyyy' }}</td>
</tr>
{{ end }}

</tbody></table>

" },
                new Template() { Body = @"
{{this.item = model}}

<table class=""table table-hover""><tbody>

<tr><td>Datum jednání</td><td >{{ item.datum }}</td></tr>
<tr><td>Bod jednání</td><td >{{ item.bod }}</a></td></tr>

{{ if !(fn_IsNullOrEmpty item.cisloJednaci)  }}
   <tr><td>Číslo jednací</td><td >{{ item.cisloJednaci }}</td></tr>
{{end }}

<tr><td>Obsah projednáváného bodu</td><td >{{ item.vec }}</td></tr>


{{ if item.dokumenty.size > 0  }}

   <tr><td>Projednávané dokumenty</td><td >
<ul>
{{ for doc in item.dokumenty }}

   <li> 
      {{ doc.jmeno }} - 
      {{fn_LinkTextDocumentWithHighlighting doc ""Vybory-PSP"" item.Id ""Obsah dokumentu"" highlightingData }}
   </li>

{{ end }}
</ul>
<div class=""text-muted small"">Vláda uveřejňuje dokumenty obvykle ve dvou formátech (DOC a PDF). Pro úplnost zpracováváme obě verze souboru, i když jsou obvykle shodné</div>
</td></tr>


{{end }}

{{ if item.souvisejici.size > 0  }}

   <tr><td>Související usnesení a dokumenty</td><td >
<ul>
{{ for doc in item.souvisejici }}

   <li> 

      Toto usnesení {{ doc.zmena }} 
      {{ if fn_IsNullOrEmpty doc.usneseniCislo }}
           {{ doc.usneseni }}
      {{ else }}
          <a href=""https://www.hlidacstatu.cz/data/Hledat/Vybory-PSP?Q=cisloJednaci.keyword:{{ string.replace doc.usneseniCislo ""/20"" ""%2F"" }}"">
          {{ doc.usneseni }}</a>
      {{ end }}
   </li>

{{ end }}
</ul>
</td></tr>


{{end }}


</table>

" }
                );

            //dsc.DeleteDataset(dsDef).Wait();
            if (!dsc.DatasetExists(dsDef).Result)
            {
                dsc.CreateDataset(dsDef).Wait();
            }

            //download vybory
            Parse.InitVybory();
            //int idVyboru = 500;
            foreach (var idVyboru in Parse.Vybory.Keys)
            {
                Parse.Vybor(dsc, idVyboru);
            }
        }


    }
}
