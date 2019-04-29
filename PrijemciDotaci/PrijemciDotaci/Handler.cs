using HtmlAgilityPack;
using System;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrijemciDotaci
{
	class Handler
	{
		private const string RootUrl = "http://www.szif.cz/cs/seznam-prijemcu-dotaci";
		private const string Url = RootUrl + "?asc=asc&year={0}&sortby=%2FBIC%2FZC_F201&ino=0&page={1}";

		private readonly Dataset DatasetConnector;
		private readonly ApiClient ApiClient;

		public Handler(Dataset datasetConnector, ApiClient apiClient)
		{
			DatasetConnector = datasetConnector;
			ApiClient = apiClient;
		}

		public async Task Execute(int year)
		{
			var client = new HttpClient();
			var page = 1;
			var rows = 0;

			await DatasetConnector.Recreate();

			do
			{
				rows = await ProcessPage(client, year, page++);
			} while (rows > 0);
		}

		private async Task<int> ProcessPage(HttpClient client, int year, int page)
		{
			Console.WriteLine();
			Console.WriteLine($"Strana {page}");
			Console.WriteLine();
			var content = await client.GetStringAsync(string.Format(Url, year, page));
			var doc = new HtmlDocument();
			doc.LoadHtml(content);

			var rows = doc.DocumentNode.SelectNodes("//div[@class='container-table']/table/tbody/tr");
			foreach (var row in rows)
			{
				var detailUrl = RootUrl + row.ChildNodes[1].ChildNodes[1].ChildNodes[0].GetAttributeValue("href", string.Empty);
				var companyName = row.ChildNodes[1].ChildNodes[1].InnerText;

				Console.Write(companyName + " ");

				var address = row.ChildNodes[1].ChildNodes[3].InnerText;
				var icoResult = CompanyNameToIco.GetIco(companyName);
				if (!icoResult.Nalezeno)
				{
					icoResult = await ApiClient.GetIcoByName(companyName);
				}
				var normalizedCompanyName = Normalize(companyName);

				var detailContent = await client.GetStringAsync(detailUrl);

				var detail = new HtmlDocument();
				detail.LoadHtml(detailContent);

				var tableRows = detail.DocumentNode.SelectNodes("//div[@class='container-table']/table/tbody/tr").ToArray();
				var rowIndex = 0;
				foreach (var detailRow in tableRows)
				{
					var cells = detailRow.Descendants("td").Select(c => c.InnerText.Trim()).ToArray();
					var isEu = cells.Length == 6;

					var result = await DatasetConnector.Add(new PrijemceDotace
					{
						Id = $"{normalizedCompanyName}-{cells[0]}-{rowIndex++}",
						ICO = icoResult.Nalezeno ? icoResult.ICO : string.Empty,
						Jmeno = companyName,
						Adresa = address,
						Url = detailUrl,
						Rok = Convert.ToInt32(cells[0]),
						Fond = cells[1],
						Opatreni = cells[2],
						ZdrojeCr = ParsePrice(cells[3]),
						ZdrojeEu = isEu ? ParsePrice(cells[4]) : 0,
						ZdrojeCelkem = isEu ? ParsePrice(cells[5]) : ParsePrice(cells[3])
					});

					Console.Write(isEu ? "E" : "C");
				}

				Console.WriteLine(" - done");
			}

			return page;
		}

		private Regex NormalizePattern = new Regex("[^a-zA-Z0-9-]", RegexOptions.Compiled);

		private string Normalize(string value)
		{
			return NormalizePattern.Replace(value, "_");
		}

		private double ParsePrice(string value)
		{
			return Convert.ToDouble(value.Replace("&nbsp;", ""));
		}
	}
}
