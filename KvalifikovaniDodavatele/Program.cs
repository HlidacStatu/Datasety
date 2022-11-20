using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KvalifikovaniDodavatele
{
	class Program
	{


        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("deMinimis",
            Devmasters.Log.Logger.DefaultConfiguration()
            .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
            .AddFileLoggerFilePerLevel("/Data/Logs/deMinimis/", "slog.txt",
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                              rollingInterval: RollingInterval.Day,
                              fileSizeLimitBytes: null,
                              retainedFileCountLimit: 9,
                              shared: true
                              )
            .WriteTo.Console()
           );

        public static Devmasters.Batch.MultiOutputWriter outputWriter =
             new Devmasters.Batch.MultiOutputWriter(
                Devmasters.Batch.Manager.DefaultOutputWriter,
                new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Debug).OutputWriter
             );

        public static Devmasters.Batch.MultiProgressWriter progressWriter =
            new Devmasters.Batch.MultiProgressWriter(
                new Devmasters.Batch.ActionProgressWriter(1.0f, Devmasters.Batch.Manager.DefaultProgressWriter).Write,
                new Devmasters.Batch.ActionProgressWriter(500, new Devmasters.Batch.LoggerWriter(logger, Devmasters.Log.PriorityLevel.Information).ProgressWriter).Write
            );

        static void Main(string[] args)
		{
			new Handler().Execute();
		}
	}

	class Handler
	{
		private readonly HttpClient Client;
		private Dictionary<string, City> Cities = new Dictionary<string, City>();

		private const string BaseUrl = "http://www.isvz.cz/ISVZ/Ciselniky/Ajax.ashx";

		public Handler()
		{
			Client = new HttpClient();
		}

		public void Execute()
		{
			LoadCities();
			Console.WriteLine($"Nacteno {Cities.Keys.Count} mest");

			using (var f = File.CreateText("dodavatele.csv"))
			{
				f.WriteLine($"ICO;Obchodní firma/Název dodavatele;Město;PSČ;Okres;NUTS;Země;Evidenční číslo;DT nabytí PM;Potvrzený člen SKD;Právní skutečnost;LegalId;Konvertován;Výpis");

				var responce = Client.PostAsync(BaseUrl, new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><selectorParams><firstItemIndex>0</firstItemIndex><pageSize>200</pageSize><sortBy>-1</sortBy><sortDirection>0</sortDirection><filterMode>2</filterMode></selectorParams><ajaxData>@type:\"/wECAQ ==,@vt:\"/wELKZEBQVNEU29mdC5JU1ZaLlB1YmxpYy5XZWJGb3JtLlR5cGVGaWx0ZXJFbnVtLCBBU0RTb2Z0LklTVlouUHVibGljLldlYkZvcm0sIFZlcnNpb249MS4wLjcwNjYuMTcxNTgsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49ZTVlYjkzODBlOGQwMDQ1NwE=</ajaxData><events>%@mh5oc%2fOzTFTwTsuA2%2b42rw%3d%3d</events><url>http://www.isvz.cz//ISVZ/Ciselniky/Seznam.aspx</url><queryString>?type=1&amp;vt=1&amp;data=</queryString><senderId>content_selectorList1</senderId><senderName>ctl00$content$selectorList1</senderName><reqId>1</reqId></root>")).Result;
				var content = responce.Content.ReadAsStringAsync().Result;
				var doc = new XmlDocument();
				doc.LoadXml(content);

				foreach (XmlNode item in doc.DocumentElement.SelectNodes("//al/a"))
				{
					var id = item.Attributes["name"].Value;
					var city = new City();
					Cities.TryGetValue(item.ChildNodes[5].FirstChild.InnerText, out city);
					var c = new Contractor
					{
						ICO = item.ChildNodes[0].FirstChild.InnerText,
						Name = System.Net.WebUtility.HtmlDecode(item.ChildNodes[1].FirstChild.InnerText).Replace("\"", ""),
						Country = System.Net.WebUtility.HtmlDecode(item.ChildNodes[2].FirstChild.InnerText),
						RegistrationNumber = System.Net.WebUtility.HtmlDecode(item.ChildNodes[3].FirstChild.InnerText),
						Zip = System.Net.WebUtility.HtmlDecode(item.ChildNodes[4].FirstChild.InnerText),
						City = city,
						DateOfEntryIntoForce = DateTime.Parse(item.ChildNodes[6].FirstChild.InnerText),
						ConfirmedMember = System.Net.WebUtility.HtmlDecode(item.ChildNodes[7].FirstChild.InnerText),
						LegalFact = System.Net.WebUtility.HtmlDecode(item.ChildNodes[8].FirstChild.InnerText),
						LegalId = System.Net.WebUtility.HtmlDecode(item.ChildNodes[9].FirstChild.InnerText),
						Converted = item.ChildNodes[10].FirstChild.InnerText.ToLowerInvariant() == "true",
						LinkToExtract = "http://www.isvz.cz/ISVZ/Download.ashx?type=1&id=" + id,
					};

					f.WriteLine($"{c.ICO};{c.Name};{c.City?.Name ?? ""};{c.Zip};{c.City?.District ?? ""};{c.City?.Nuts ?? ""};{c.Country};{c.RegistrationNumber};{c.DateOfEntryIntoForce.ToShortDateString()};{c.ConfirmedMember};{c.LegalFact};{c.LegalId};{c.Converted};{c.LinkToExtract}");
				}
			}
		}

		private void LoadCities()
		{
			var responce = Client.PostAsync(BaseUrl, new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><selectorParams><firstItemIndex>0</firstItemIndex><pageSize>20000</pageSize><sortBy>-1</sortBy><sortDirection>0</sortDirection><filterMode>2</filterMode></selectorParams><ajaxData>@type:\"/wECAQ==</ajaxData><events>%@%2bkBLuYozD9GmnPeklOVP2A%3d%3d</events><url>http://www.isvz.cz//ISVZ/Ciselniky/Ciselnik.aspx</url><queryString>?type=01</queryString><senderId>content_selectorList1</senderId><senderName>ctl00$content$selectorList1</senderName><reqId>1</reqId></root>")).Result;
			var content = responce.Content.ReadAsStringAsync().Result;
			var doc = new XmlDocument();
			doc.LoadXml(content);

			foreach (XmlNode item in doc.DocumentElement.SelectNodes("//al/a"))
			{
				var id = item.ChildNodes[0].FirstChild.InnerText;
				Cities.Add(id, new City
				{
					Id = id,
					Name = item.ChildNodes[1].FirstChild.InnerText,
					Nuts = item.ChildNodes[2].FirstChild.InnerText,
					District = item.ChildNodes[3].FirstChild.InnerText,
				});
			}
		}
	}

	class Contractor
	{
		public string ICO { get; set; }
		public string Name { get; set; }
		public string Country { get; set; }
		public string RegistrationNumber { get; set; }
		public string Zip { get; set; }
		public City City { get; set; }
		public DateTime DateOfEntryIntoForce { get; set; }
		public string ConfirmedMember { get; set; }
		public string LegalFact { get; set; }
		public string LegalId { get; set; }
		public bool Converted { get; set; }
		public string LinkToExtract { get; set; }
	}

	class City
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Nuts { get; set; }
		public string District { get; set; }
	}
}
