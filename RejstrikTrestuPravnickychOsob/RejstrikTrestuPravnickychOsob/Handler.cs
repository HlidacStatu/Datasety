using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RejstrikTrestuPravnickychOsob
{
	class Handler
	{
		private const string RootUrl = "https://eservice-po.rejtr.justice.cz/public";

		private readonly Dataset DatasetConnector;
		private int id = 0;

		public Handler(Dataset datasetConnector)
		{
			DatasetConnector = datasetConnector;
		}

		public async Task Execute()
		{
			var client = new HttpClient();
			var link = RootUrl + "/odsouzeni";
			var index = 1;
			id = 0;

			await DatasetConnector.Recreate();

			do
			{
				Console.WriteLine($"PAGE {index++}");

				var mainPage = await client.GetAsync(link);
				var content = await mainPage.Content.ReadAsStringAsync();

				var doc = new HtmlDocument();
				doc.LoadHtml(content);

				await ProcessPage(client, doc);

				link = PrepareLink(doc.DocumentNode.SelectSingleNode("//a[@class='next']")?.GetAttributeValue("href", string.Empty));
				Console.WriteLine();
			} while (link != null);
		}

		private static string PrepareLink(string link)
		{
			return string.IsNullOrEmpty(link)
				? null
				: RootUrl + link.Substring(1);
		}

		private async Task ProcessPage(HttpClient client, HtmlDocument doc)
		{
			var rows = doc.DocumentNode.SelectNodes("//tbody/tr");

			foreach (var row in rows)
			{
				var ico = row.ChildNodes[1].InnerText.Trim();
				var name = row.ChildNodes[2].InnerText.Trim();
				var address = row.ChildNodes[3].InnerText.Trim();
				var country = row.ChildNodes[4].InnerText.Trim();
				var link = row.ChildNodes[5].ChildNodes[1].ChildNodes[0].GetAttributeValue("href", string.Empty);
				var convictionText = string.Empty;

				if (!string.IsNullOrEmpty(link))
				{
					link = PrepareLink(link);

					var detailPage = await client.GetAsync(link);
					var detailContent = await detailPage.Content.ReadAsStringAsync();

					var detailDoc = new HtmlDocument();
					detailDoc.LoadHtml(detailContent);
					var p = detailDoc.DocumentNode.SelectSingleNode("//td/span/p");
					convictionText = p.InnerText;
				}

				var item = new Trest
				{
					Id = (++id).ToString(),
					ICO = ico,
					ObchodniJmeno = name,
					Sidlo = address,
					Stat = country,
					TextOdsouzeni = convictionText
				};
				await DatasetConnector.Add(item);

				Console.WriteLine($" - {ico};{name}");
			}
		}
	}
}
