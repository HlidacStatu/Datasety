using System.Linq;
using System.Text;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Threading;

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
		private readonly ConcurrentQueue<CsvRecord> Queue = new ConcurrentQueue<CsvRecord>();

		public CsvParser(Dataset datasetConnector)
		{
			Connector = datasetConnector;
		}

		public void Execute(int year, string path)
		{
			foreach (var item in Parser.ReadFromFile(path, Encoding.UTF8).Where(r => r.IsValid))
			{
				Queue.Enqueue(item.Result);
			}

			var total = Queue.Count();
			Logger.Info($"Nacteno {total} zaznamu");
			var start = DateTime.Now;
			var processed = 0;

			for (int i = 0; i < 10; i++)
			{
				var t = new Thread(delegate () {
					var threadId = i;
					Logger.Info($"Startuje zpracovani vlakna {threadId}");
					CsvRecord item = null;
					while (Queue.TryDequeue(out item))
					{
						if (string.IsNullOrEmpty(item.ICO + item.Jmeno + item.Obec + item.Okres + item.Zdroj))
						{
							continue;
						}
						Save(year, item);
						var current = Interlocked.Increment(ref processed);
						if (current % 100 == 0)
						{
							var duration = DateTime.Now - start;
							Logger.Info($"[{threadId}] Ulozeno {processed} zaznamu ({duration}) -> {TimeSpan.FromSeconds(duration.TotalSeconds * total / current)}");
						}
					}
					Logger.Info($"Konci zpracovani vlakna {threadId}");
				});
				t.Start();
			}
		}

		private void Save(int year, CsvRecord r)
		{
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
			Connector.Add(item).Wait();
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
