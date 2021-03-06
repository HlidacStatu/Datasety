﻿using HtmlAgilityPack;
using System;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinyCsvParser;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace RejstrikTrestuPravnickychOsob
{
	class Handler
	{
		private const string Url = "https://eservice-po.rejtr.justice.cz/public/odsouzeni_xml";

		private readonly Dataset DatasetConnector;
		private readonly Regex DateRegex = new Regex(@"Dat. rozhodnutí: (\d{2}.\d{2}.\d{4})", RegexOptions.Compiled);

		public Handler(Dataset datasetConnector)
		{
			DatasetConnector = datasetConnector;
		}


		public void Execute()
		{
			var client = new HttpClient();
			var content = client.GetStringAsync("https://eservice-po.rejtr.justice.cz/public/odsouzeni_xml").Result;

			//XmlDocument doc = new XmlDocument();
			//doc.LoadXml(content);
			VypisXML.vypisList data = null;
			XmlSerializer serializer = new XmlSerializer(typeof(VypisXML.vypisList));
			using (TextReader reader = new StringReader(content))
			{
				data = (VypisXML.vypisList)serializer.Deserialize(reader);
			}
			List<Trest> tresty = new List<Trest>();
			foreach (var item in data.vypis)
			{
				var jd = new Trest(item);
				tresty.Add(jd);
			}
			foreach (var item in tresty)
			{
				DatasetConnector.Add(item).Wait();

				Console.WriteLine($" - {item.ICO};{item.NazevFirmy}");
			}
		}
	}
}
