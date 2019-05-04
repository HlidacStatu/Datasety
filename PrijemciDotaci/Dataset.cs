using HlidacStatu.Api.Dataset.Connector;
using System;
using System.Threading.Tasks;

namespace PrijemciDotaci
{
	class Dataset
	{
		private readonly IDatasetConnector Connector;

		public Dataset(string token)
		{
			Connector = new DatasetConnector(token);
		}

		public async Task<string> Recreate()
		{
			var datasetExists = await Connector.DatasetExists(Definition);
			if (datasetExists)
			{
				await Connector.DeleteDataset(Definition);
			}

			return await Connector.CreateDataset(Definition);
		}

		public async Task<string> Add(PrijemceDotace item)
		{
			return await Connector.AddItemToDataset(Definition, item);
		}

		private readonly Dataset<PrijemceDotace> Definition = new Dataset<PrijemceDotace>(
			"Příjemci dotací",
			"PrijemciDotaci",
			"http://www.szif.cz/cs/seznam-prijemcu-dotaci",
			"Seznam příjemců dotací z fondů EU a ze státního rozpočtu ČR prostřednictvím administrace Státního zemědělského intervenčního fondu (SZIF)",
			"https://github.com/HlidacStatu/Datasety",
			true,
			false,
			new string[,] { { "Rok", "Rok" }, { "Název", "Nazev" }, { "Fond", "Fond" }, { "Zdroje celkem", "ZdrojeCelkem" } },
			new Template { Body = @"<table class=""table table-hover"" >
    <thead>
        <tr>
            <th></th>
            <th>IČO</th>
            <th>Název</th>
			<th>Vztahy se státem</th>
            <th>Rok</th>
            <th>Fond</th>
			<th style=""text-align:right"">Celková částka</th>
        </tr>
    </thead>
    <tbody>
                        
{{for item in model.Result}}
           <tr>
                <td style=""white-space: nowrap;"">
                    <a href=""{{fn_DatasetItemUrl item.Id}}"" >Detail</a>
                </td>
                <td style=""white-space: nowrap;"" >
                    {{item.ICO}}
                </td>
                <td style=""white-space: nowrap;"" >
                    {{ if fn_IsNullOrEmpty item.ICO }}
						{{ item.Jmeno }}
					{{ else }}
						{{ fn_RenderCompanyWithLink item.ICO }}
					{{ end }}
                </td>
			    <td style=""white-space: nowrap;"" >
					{{ if fn_IsNullOrEmpty item.ICO }}
					{{ else }}
					{{ fn_RenderCompanyStatistic item.ICO }}
					{{ end }}      
				</td>
                <td style=""white-space: nowrap;"" >
                    {{item.Rok}}
                </td>
                <td style=""white-space: nowrap;"" >
                    {{item.Fond}}
                </td>
				<td style=""text-align:right"">
					{{fn_FormatPrice item.ZdrojeCelkem}}
				</td>
            </tr>
{{end}}
</tbody></table>" },
			new Template { Body = @"{{this.item = model }}
<table class=""table table-hover"" >
        <tbody>
            <tr>
                <td>IČO</td>
                <td>{{item.ICO}}</td>
            </tr>
            <tr>
                <td>Název</td>
				<td>
                    {{ if fn_IsNullOrEmpty item.ICO }}
						{{ item.Jmeno }}
					{{ else }}
						{{ fn_RenderCompanyWithLink item.ICO }}
					{{ end }}
				</td>
            </tr>
			<tr>                
				<td>Vztahy se státem            
				</td>
				<td>
					{{ if fn_IsNullOrEmpty item.ICO }}
					{{ else }}
						{{ fn_RenderCompanyStatistic item.ICO }}
					{{ end }}                
				</td>
			</tr>
            <tr>
                <td>Adresa</td>
                <td>{{item.Adresa}}</td>
            </tr>
            <tr>
                <td>Rok</td>
                <td>{{item.Rok}}</td>
            </tr>
            <tr>
                <td>Fond</td>
                <td>{{item.Fond}}</td>
            </tr>
            <tr>
                <td>Opatření</td>
                <td>{{item.Opatreni}}</td>
            </tr>
            <tr>
                <td>Zdroje ČR</td>
                <td>{{fn_FormatPrice item.ZdrojeCr}}</td>
            </tr>
            <tr>
                <td>Zdroje EU</td>
                <td>{{fn_FormatPrice item.ZdrojeEu}}</td>
            </tr>
            <tr>
                <td>Celkem</td>
                <td>{{fn_FormatPrice item.ZdrojeCelkem}}</td>
            </tr>
            <tr>
                <td>Zdroj SZIF</td>
                <td><pre><a href=""{{item.Url}}"">{{item.Url}}</a></pre></td>
            </tr>
        </tbody>
    </table>" });
	}

	public class PrijemceDotace : IDatasetItem
	{
		public string Id
		{
			get
			{
				return Utils.CalculateMD5Hash($"{ICO}-{Jmeno}-{Rok}-{Fond}-{ZdrojeCelkem}");
			}
		}
		public string ICO { get; set; }
		public string Jmeno { get; set; }
		public string Adresa { get; set; }
		public int Rok { get; set; }
		public string Fond { get; set; }
		public string Opatreni { get; set; }
		public double ZdrojeCr { get; set; }
		public double ZdrojeEu { get; set; }
		public double ZdrojeCelkem { get; set; }
		public string Url { get; set; }
	}
}
