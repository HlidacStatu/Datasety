using System;
using System.Linq;

namespace RejstrikTrestuPravnickychOsob
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Rejstrik trestu pravnickych osob");
			Console.WriteLine("--------------------------------");

			var token = args.FirstOrDefault(a => a.StartsWith("--token="))?.Substring(8);
			if (token == null)
			{
				Help();
				return;
			}

			var dataset = new Dataset(token);

			var handler = new Handler(dataset);
			handler.Execute().Wait();


			Console.ReadKey();
		}

		private static void Help()
		{
			Console.WriteLine("Parametry spusteni:");
			Console.WriteLine("  --token=<token> - autorizacni token do Hlidace Statu");
			Console.WriteLine();
			Console.WriteLine("Ukazka volani:");
			Console.WriteLine("  RejstrikTrestuPravnickychOsob --token=0123456789ABCDEF");
			Console.WriteLine();
		}
	}
}
