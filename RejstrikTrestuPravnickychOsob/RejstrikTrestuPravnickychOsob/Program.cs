using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Serialization;

using Devmasters.Log;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

using Serilog;

namespace RejstrikTrestuPravnickychOsob
{
	class Program
	{

        public static Devmasters.Log.Logger logger = Devmasters.Log.Logger.CreateLogger("RejstriktrestuPravnickychOsob",
            Devmasters.Log.Logger.DefaultConfiguration()
            .Enrich.WithProperty("codeversion", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString())
            .AddFileLoggerFilePerLevel("/Data/Logs/rejstrikTrestuPravnickychOsob/", "slog.txt",
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

			Console.WriteLine("Rejstrik trestu pravnickych osob");
			Console.WriteLine("--------------------------------");

			var token = args.FirstOrDefault(a => a.StartsWith("--token="))?.Substring(8);
			if (token == null)
			{
				Help();
				return;
			}

			if (System.Diagnostics.Debugger.IsAttached )
				System.Net.WebRequest.DefaultWebProxy = new System.Net.WebProxy("127.0.0.1", 8888);

			var dataset = new Dataset(token);

			//if (System.Diagnostics.Debugger.IsAttached)
			//	dataset.Connector.Api.Configuration.BasePath = "https://local.hlidacstatu.cz";

            if (args.Any(a=>a.ToLower()=="--create"))
                dataset.Recreate().Wait();

			var jsonGen = new JSchemaGenerator
			{
				DefaultRequired = Required.Default
			};
			var genJsonSchema = jsonGen.Generate(typeof(Trest)).ToString();


			var handler = new Handler(dataset);
			handler.Execute();
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
