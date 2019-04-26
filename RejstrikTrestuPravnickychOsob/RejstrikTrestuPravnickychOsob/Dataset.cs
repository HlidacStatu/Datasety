using HlidacStatu.Api.Dataset.Connector;
using System;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace RejstrikTrestuPravnickychOsob
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

		public async Task<string> Add(Trest item)
		{
			return await Connector.AddItemToDataset(Definition, item);
		}

		private readonly Dataset<Trest> Definition = new Dataset<Trest>(
			"Rejstřík trestů právnických osob", 
			"RejstrikTrestuPravnickychOsob",
			"https://eservice-po.rejtr.justice.cz/public/odsouzeni",
			"",
			"https://github.com/HlidacStatu/Datasety",
			false,
			false,
			new string[,] { { "DatumRozhodnuti", "DatumRozhodnuti" } },
			new Template { Body = @"<table class=""table table-hover"" >
    <thead>
        <tr>
            <th></th>
            <th>IČO</th>
            <th>Obchodní jméno</th>
            <th>Sídlo</th>
            <th>Stát</th>
			<th>Rozhodnutí</th>
        </tr>
    </thead>
    <tbody>
                        
{{for item in model.Result | array.sort ""DatumRozhodnuti"" | array.reverse}}
           <tr>
                <td style=""white-space: nowrap;"">
                    <a href=""{{fn_DatasetItemUrl item.Id}}"" >Detail</a>
                </td>
                <td style=""white-space: nowrap;"" >
                    {{item.ICO}}
                </td>
                <td style=""white-space: nowrap;"" >
                    {{fn_RenderCompanyWithLink item.ICO}}
                </td>
                <td style=""white-space: nowrap;"" >
                    {{item.Sidlo}}
                </td>
                <td style=""white-space: nowrap;"" >
                    {{item.Stat}}
                </td>
				<td>
					{{fn_FormatDate item.DatumRozhodnuti}}
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
                <td>Obchodní jméno</td>
                <td>{{fn_RenderCompanyWithLink item.ICO}}</td>
            </tr>
            <tr>
                <td>Sídlo</td>
                <td>{{item.Sidlo}}</td>
            </tr>
            <tr>
                <td>Stát</td>
                <td>{{item.Stat}}</td>
            </tr>
            <tr>
                <td>Text odsouzení</td>
                <td><pre>{{item.TextOdsouzeni}}</pre></td>
            </tr>
        </tbody>
    </table>" });
	}

	public class Trest : IDatasetItem
	{
		public string Id { get; set; }
		public string ICO { get; set; }
		public string ObchodniJmeno { get; set; }
		public string Sidlo { get; set; }
		public string Stat { get; set; }
		public string TextOdsouzeni { get; set; }
		public DateTime? DatumRozhodnuti { get; set; }
	}

	public class TrestMapping : CsvMapping<Trest>
	{
		public TrestMapping() : base()
		{
			MapProperty(0, x => x.ICO);
			MapProperty(1, x => x.ObchodniJmeno);
			MapProperty(2, x => x.Sidlo);
			MapProperty(3, x => x.Stat);
			MapProperty(4, x => x.TextOdsouzeni);
		}
	}
}
