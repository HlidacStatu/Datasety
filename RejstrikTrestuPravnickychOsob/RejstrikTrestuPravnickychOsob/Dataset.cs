
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TinyCsvParser.Mapping;

namespace RejstrikTrestuPravnickychOsob
{
    class Dataset
    {
        public readonly HlidacStatu.Api.V2.Dataset.Typed.Dataset<Trest> Connector;

        public Dataset(string token)
        {
            try
            {
                Connector = HlidacStatu.Api.V2.Dataset.Typed.Dataset<Trest>.OpenDataset(token, "rejstrik-trestu-pravnickych-osob");
            }
            catch (Exception e)
            {
                throw;
            }

        }
        public async Task<string> Recreate()
        {
            //var datasetExists = await Connector.open(Definition);
            //if (datasetExists)
            //{
            //	await Connector.DeleteDataset(Definition);
            //}

            //return await Connector.CreateDataset(Definition);
            return "";
        }

        public async Task<string> Add(Trest item)
        {
            return Connector.AddOrUpdateItem(item, HlidacStatu.Api.V2.Dataset.Typed.ItemInsertMode.rewrite);
        }

        //		private readonly HlidacStatu.Api.V2.CoreApi.Model.Registration Definition = new HlidacStatu.Api.V2.CoreApi.Model.Registration(
        //			"Rejstřík trestů právnických osob", 
        //			"rejstrik-trestu-pravnickych-osob",
        //			"https://eservice-po.rejtr.justice.cz/public/odsouzeni",
        //            "V souvislosti s přijetím zákona č. 418/2011 Sb., o trestní odpovědnosti právnických osob a řízení proti nim a zákona č. 420/2011 Sb., o změně některých zákonů v souvislosti s přijetím zákona o trestní odpovědnosti právnických osob a řízení proti nim vznikla na Ministerstvu spravedlnosti evidence Rejstříku trestů právnické osoby.",
        //			"https://github.com/HlidacStatu/Datasety",
        //			false,
        //			false,
        //			new string[,] { { "DatumRozhodnuti", "DatumRozhodnuti" } },
        //			new Template { Body = @"<table class=""table table-hover"" >
        //  <thead>        
        //    <tr>
        //      <th>
        //      </th>
        //      <th>IČO
        //      </th>
        //      <th>Obchodní jméno
        //      </th>
        //      <th>Vztahy se státem
        //      </th>
        //      <th>Rozhodnutí
        //      </th>
        //    </tr>
        //  </thead>
        //  <tbody>
        //    {{for item in model.Result | array.sort ""DatumRozhodnuti"" | array.reverse}}           
        //    <tr>
        //      <td style=""white-space: nowrap;"">                    
        //        <a href=""{{fn_DatasetItemUrl item.Id}}"" >Detail
        //        </a>
        //      </td>
        //      <td style=""white-space: nowrap;"" >
        //        {{item.ICO}}                
        //      </td>
        //      <td style=""white-space: nowrap;"" >
        //        {{fn_RenderCompanyWithLink item.ICO}}
        //      </td>
        //      <td style=""white-space: nowrap;"" >
        //        {{ fn_RenderCompanyStatistic item.ICO }}                
        //      </td>
        //      <td>
        //        {{fn_FormatDate item.DatumPravniMoci }}    
        //      </td>
        //    </tr>
        //{{end}}
        //  </tbody>
        //</table>" }, 
        //			new Template { Body = @"{{this.item = model }}
        //<table class=""table table-hover"" >        
        //  <tbody>
        //    <tr>                
        //      <td>IČO
        //      </td>
        //      <td>{{item.ICO}}
        //      </td>
        //    </tr>
        //    <tr>                
        //      <td>Obchodní jméno
        //      </td>
        //      <td>{{fn_RenderCompanyWithLink item.ICO}}
        //      </td>
        //    </tr>
        //    <tr>                
        //      <td>Vztahy se státem
        //      </td>
        //      <td>{{fn_RenderCompanyStatistic item.ICO}}
        //      </td>
        //    </tr>

        //    <tr>                
        //      <td>Sídlo
        //      </td>
        //      <td>{{item.Sidlo}}
        //      </td>
        //    </tr>
        //    <tr>                
        //      <td>Datum odsouzení
        //      </td>
        //      <td>
        //        {{fn_FormatDate item.DatumPravniMoci }}        
        //      </td>
        //    </tr>
        //    <tr>                
        //      <td>Odsouzení
        //      </td>
        //      <td>
        //{{ if item.Odsouzeni}}
        //    {{ item.Odsouzeni.PrvniInstance.DruhRozhodnuti }} - {{ item.Odsouzeni.PrvniInstance.Jmeno}}, dne {{ fn_FormatDate item.Odsouzeni.PrvniInstance.DatumRozhodnuti}}, č.j. {{ item.Odsouzeni.PrvniInstance.SpisovaZnacka}}

        //    {{ if (item.Odsouzeni.OdvolaciSoud.empty? == false) }}
        //        <p>Odvolání: 
        //        {{ item.Odsouzeni.OdvolaciSoud.DruhRozhodnuti }} - {{ item.Odsouzeni.OdvolaciSoud.Jmeno}}, dne {{ fn_FormatDate item.Odsouzeni.OdvolaciSoud.DatumRozhodnuti}}, č.j. {{ item.Odsouzeni.OdvolaciSoud.SpisovaZnacka}}
        //        </p>
        //    {{end}}


        //{{end}}
        //      </td>
        //    </tr>

        //{{ if item.Tresty}}
        //    <tr>                
        //      <td>Trest
        //      </td>
        //      <td> {{ for t in item.Tresty}}{{ t.DruhText }}, {{end}}
        //      </td>
        //    </tr>
        //{{ end }}

        //{{ if item.Paragrafy}}
        //    <tr>                
        //      <td>Překročené zákony
        //      </td>
        //      <td><ul>
        //         {{ for p in item.Paragrafy}}  
        //             <li>{{p.Zavineni }}
        //                 <a href='https://www.zakonyprolidi.cz/cs/{{p.Zakon.Rok}}-{{p.Zakon.ZakonCislo}}#p{{p.Zakon.ParagrafCislo}}'>{{p.ZakonPopis}} §{{p.Zakon.ParagrafCislo}}
        //{{if p.Zakon.OdstavecPismeno }}odst. {{p.Zakon.OdstavecPismeno}} {{end}}</a>
        //{{if (fn_IsNullOrEmpty p.Doplneni) == false}}, <i>{{p.Doplneni}}</i>{{end}}
        //</li>
        //         {{end }}
        //         </ul>
        //      </td>
        //    </tr>
        //{{ end }}

        //  </tbody>
        //</table>

        //" });
        //	

    }
}
