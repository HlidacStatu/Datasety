using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrijemciDotaci
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Prijemci dotaci");
			Console.WriteLine("---------------");

			var token = args.FirstOrDefault(a => a.StartsWith("--token="))?.Substring(8);
			if (token == null)
			{
				Help();
				return;
			}

			var yearArg = args.FirstOrDefault(a => a.StartsWith("--rok="))?.Substring(6);
			var year = 2017;
			if (yearArg == null || !int.TryParse(yearArg, out year))
			{
				Help();
				return;
			}

			var dataset = new Dataset(token);
			var apiClient = new ApiClient(token);

			//dataset.Recreate().Wait();

			var csvFile = args.FirstOrDefault(a => a.StartsWith("--csv="))?.Substring(6);
			if (csvFile != null)
			{
				var parser = new CsvParser(dataset);
				parser.Execute(year, csvFile);
			}
			else
			{
				var handler = new Handler(dataset, apiClient);
				handler.Execute(year).Wait();
			}

			Console.WriteLine();
			Console.WriteLine("Done");
			Console.ReadKey();
		}

		private static void Help()
		{
			Console.WriteLine("Parametry spusteni:");
			Console.WriteLine("  --rok=<rok> - rok, pro ktery se stahuji prijemci dotaci");
			Console.WriteLine("  --token=<token> - autorizacni token do Hlidace Statu");
			Console.WriteLine("  --csv=<cesta k souboru> - nepovinny, cesta k souboru CSV s importovanymi data (primarne pro data z dotacni-parazit.cz)");
			Console.WriteLine();
			Console.WriteLine("Ukazka volani:");
			Console.WriteLine("  RejstrikTrestuPravnickychOsob --rok=2017 --token=0123456789ABCDEF");
			Console.WriteLine();
		}
	}
}
