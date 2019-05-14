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
            if (args.Any(a=>a.ToLower()=="--created"))
                dataset.Recreate().Wait();

            var handler = new Handler(dataset);
			handler.Execute().Wait();
		}

		private static void Help()
		{
			Console.WriteLine("Parametry spusteni:");
			Console.WriteLine("  --token=<token> - autorizacni token do Hlidace Statu");
            Console.WriteLine("  --create - smaze existujici a vytvori databazi znovu. Jinak provede pouze update dat");
            Console.WriteLine();
			Console.WriteLine("Ukazka volani:");
			Console.WriteLine("  RejstrikTrestuPravnickychOsob --token=0123456789ABCDEF");
			Console.WriteLine();
		}
	}
}
