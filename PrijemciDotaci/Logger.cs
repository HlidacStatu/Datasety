using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PrijemciDotaci
{
	class Logger
	{
		private readonly string Name;

		public Logger(string name)
		{
			Name = name;
		}

		public void Debug(string message)
		{ }

		public void Info(string message)
		{
			Console.WriteLine(message);
			WriteToFile(message, "INFO");
		}

		public void Warn(string message)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine(message);
			Console.ForegroundColor = color;
			WriteToFile(message, "WARN");
		}

		private static readonly object LockRoot = new object();

		private void WriteToFile(string message, string type)
		{
			lock (LockRoot)
			{
				File.AppendAllLines("app.log", new[] { $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {type}: {message}" });
			}
		}
	}
}
