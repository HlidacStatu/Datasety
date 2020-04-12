using HtmlAgilityPack;
using System;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinyCsvParser;
using System.Collections.Generic;

namespace RejstrikTrestuPravnickychOsob
{
	class Handler
	{
		private const string Url = "https://eservice-po.rejtr.justice.cz/public/odsouzeni_csv";

		private readonly Dataset DatasetConnector;
		private readonly Regex DateRegex = new Regex(@"Dat. rozhodnutí: (\d{2}.\d{2}.\d{4})", RegexOptions.Compiled);

		public Handler(Dataset datasetConnector)
		{
			DatasetConnector = datasetConnector;
		}


		public async Task Execute()
		{
			var client = new HttpClient();
			var content = await client.GetStringAsync(Url);
			var id = 0;


			var csvParser = new CsvParser<Trest>(new CsvParserOptions(true, ','), new TrestMapping());
			var data = csvParser.ReadFromString(new CsvReaderOptions(new[] { "\n" }), content.Trim());

			foreach (var item in data.Select(d => d.Result).ToArray())
			{
				var match = DateRegex.Match(item.TextOdsouzeni);
				if (match.Success)
				{
					item.DatumRozhodnuti = DateTime.ParseExact(match.Groups[1].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
				}
				item.Id =  $"{item.ICO}-{item.DatumRozhodnuti?.ToString("yyyyMMdd") ?? "00010101"}";
				await DatasetConnector.Add(item);

				Console.WriteLine($" - {item.ICO};{item.ObchodniJmeno}");
			}
		}
	}
}
