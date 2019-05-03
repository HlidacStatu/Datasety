using System.Linq;
using System.Text;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using System.Threading.Tasks;
using System;

namespace PrijemciDotaci
{
	/// <summary>
	/// Parser CSV souboru z webu dotacni-parazit.cz (https://dotacni-parazit.cz/opendata#downloads)
	/// Parseruje a uklada data z SZIF SPD souboru
	/// </summary>
	class CsvParser
	{
		private static readonly CsvParserOptions CsvParserOptions = new CsvParserOptions(true, ',');
		private static readonly CsvRecordMapping CsvMapper = new CsvRecordMapping();
		private readonly CsvParser<CsvRecord> Parser = new CsvParser<CsvRecord>(CsvParserOptions, CsvMapper);
		private readonly Dataset Connector;
		private readonly Logger Logger = new Logger(nameof(CsvParser)); 

		public CsvParser(Dataset datasetConnector)
		{
			Connector = datasetConnector;
		}

		public async Task Execute(int year, string path)
		{
			var data = Parser.ReadFromFile(path, Encoding.UTF8).ToList().AsParallel();
			Logger.Info($"Nacteno {data.Count()} zaznamu");
			var count = 0;
			var start = DateTime.Now;
			foreach (var row in data)
			{
				if (row.IsValid)
				{
					var r = row.Result;
					var item = new PrijemceDotace
					{
						ICO = r.ICO,
						Jmeno = r.Jmeno,
						Adresa = r.Obec + ", okres " + r.Okres,
						Rok = year,
						Fond = r.Zdroj,
						Opatreni = r.Opatreni,
						ZdrojeCr = ToDouble(r.CastkaCr),
						ZdrojeEu = ToDouble(r.CastkaEu),
						ZdrojeCelkem = ToDouble(r.CastkaCelkem),
						Url = "https://dotacni-parazit.cz/program-rozvoje-venkova/ico/" + r.ICO
					};
					await Connector.Add(item);

					if (++count % 100 == 0)
					{
						var duration = DateTime.Now - start;
						Logger.Info($"Ulozeno {count} zaznamu ({duration}) -> {TimeSpan.FromSeconds(duration.TotalSeconds * data.Count() / count)}");
					}
				}
			}
		}

		private double ToDouble(string value)
		{
			return Convert.ToDouble(value.Replace(" CZK", string.Empty).Replace(",", string.Empty).Replace(".", ","));
		}
	}

	class CsvRecord
	{
		// IČ(pouze u PO),Jméno,Obec,Okres,Zdroj,Opatření,Částka dotace část ČR,Částka dotace část EU,Částka celkem
		public string ICO { get; set; }
		public string Jmeno { get; set; }
		public string Obec { get; set; }
		public string Okres { get; set; }
		public string Zdroj { get; set; }
		public string Opatreni { get; set; }
		public string CastkaCr { get; set; }
		public string CastkaEu { get; set; }
		public string CastkaCelkem { get; set; }
	}

	class CsvRecordMapping : CsvMapping<CsvRecord>
	{
		public CsvRecordMapping()
			: base()
		{
			MapProperty(0, x => x.ICO);
			MapProperty(1, x => x.Jmeno);
			MapProperty(2, x => x.Obec);
			MapProperty(3, x => x.Okres);
			MapProperty(4, x => x.Zdroj);
			MapProperty(5, x => x.Opatreni);
			MapProperty(6, x => x.CastkaCr);
			MapProperty(7, x => x.CastkaEu);
			MapProperty(8, x => x.CastkaCelkem);
		}
	}
}
