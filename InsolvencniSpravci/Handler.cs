using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace InsolvencniSpravci
{
	class Handler
	{
		private readonly HttpClient Client;
		private Queue<InsolvencyAdministrator> Administrators = new Queue<InsolvencyAdministrator>();

		private const string BaseUrl = "https://isir.justice.cz/InsSpravci";

		public Handler()
		{
			Client = new HttpClient();
		}

		public void Execute()
		{
			LoadListOfInsolvencyAdministrators("O"); // Obecná - oddlužení i konkurs
			LoadListOfInsolvencyAdministrators("H"); // Hostující
			LoadListOfInsolvencyAdministrators("Z"); // Zvláštní

			Console.WriteLine($"Nacteno {Administrators.Count} insolvencnich spravcu");

			using (var adm = File.CreateText("spravci.csv"))
			using (var comp = File.CreateText("spolecnici.csv"))
			using (var branch = File.CreateText("pobocky.csv"))
			{
				adm.WriteLine($"Id;Jmeno;ICO;Typ;StaraFiremniJmena;KrestniJmeno;Prijmeni;PredeslaPrijmeni;DatumNarozeni;Oddluzeni;Konkurs;Ulice;PSC;CP;Mesto;Okres;DatumVzniku;DatumZvlZkousky;DatumPozastaveni;DuvodPozastaveni;DatumZaniku;DuvodZaniku;OdkazNaDetail");
				comp.WriteLine($"IdSpravce;KrestniJmeno;Prijmeni;DatumNarozeni;Oddluzeni;Konkurs;DatumZvlZkousky");
				branch.WriteLine($"IdSpravce;Adresa;Okres;Kraj;KonkursniProvozovna");

				foreach (var item in Administrators)
				{
					Console.WriteLine(item.Name);
					LoadInsolvencyAdministratorDetail(item);

					adm.WriteLine($"{item.Id};{item.Name};{item.ICO};{item.Type};{item.OldNames};{item.FirstName};{item.SureName};{item.OldSureNames};{item.BirthDate?.ToShortDateString() ?? ""};{(item.CanDebtRelief ? "ANO" : "NE")};{(item.CanKonkurs ? "ANO" : "NE")};{item.Street};{item.Zip};{item.Number};{item.City};{item.District};{item.DateOfEstablishment?.ToShortDateString() ?? ""};{item.DateOfExam?.ToShortDateString() ?? ""};{item.DateOfSuspension?.ToShortDateString() ?? ""};{item.ReasonOfSuspension};{item.DateOfTermination?.ToShortDateString() ?? ""};{item.ReasonOfTermination};{item.LinkDetail}");

					foreach (var c in item.Companions)
					{
						comp.WriteLine($"{c.AdministratorId};{c.Name};{c.Surename};{c.BirthDate?.ToShortDateString() ?? ""};{(c.CanDebtRelief ? "ANO" : "NE")};{(c.CanKonkurs ? "ANO" : "NE")};{c.DateOfSpecialExamination?.ToShortDateString() ?? ""}");
					}

					foreach (var b in item.BranchOffices)
					{
						branch.WriteLine($"{b.AdministratorId};{b.Address};{b.District};{b.Region};{(b.IsKonkursBranch ? "ANO" : "NE")}");
					}

					adm.Flush();
					comp.Flush();
					branch.Flush();
				}
			}
		}

		private void LoadListOfInsolvencyAdministrators(string type)
		{
			var resp = Client.PostAsync(BaseUrl + "/public/seznamAction.do", new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("filtrCast", type),
				new KeyValuePair<string, string>("lastSize", "1000"),
				new KeyValuePair<string, string>("size", "1000"),
				new KeyValuePair<string, string>("size", "1000"),
				new KeyValuePair<string, string>("path", "/public/seznamAction.do"),
			})).Result;
			var content = resp.Content.ReadAsStringAsync().Result;

			var doc = new HtmlDocument();
			doc.LoadHtml(content);

			var rows = doc.DocumentNode.SelectNodes("//table[@border='2']/tr");
			foreach (var row in rows.Skip(1))
			{
				var relativeLink = row.ChildNodes[1].FirstChild.ChildAttributes("href").FirstOrDefault().Value.Replace("..", "");
				var t = "unknown";
				if (relativeLink.Contains("detailFyzOs")) t = "FO";
				else if (relativeLink.Contains("detailVerObchSpol")) t = "VOS";
				else if (relativeLink.Contains("detailHostSpr")) t = "HOST";
				else throw new ArgumentException("Neznamy typ - " + relativeLink);
				var administrator = new InsolvencyAdministrator
				{
					Name = row.ChildNodes[1].InnerText,
					LinkDetail = BaseUrl + relativeLink,
					Type = t,
					Id = Convert.ToInt32(relativeLink.Split(new[] { '=', '&' })[1])
				};
				Administrators.Enqueue(administrator);
			}
		}

		private void LoadInsolvencyAdministratorDetail(InsolvencyAdministrator item)
		{
			var resp = Client.PostAsync(BaseUrl + "/public/osobaDetailAction.do", new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("ukazVsechnyProvozovny", "true"),
				new KeyValuePair<string, string>("typOsoby", item.Type),
				new KeyValuePair<string, string>("idOsoby", item.Id.ToString()),
			})).Result;
			var content = resp.Content.ReadAsStringAsync().Result;

			var doc = new HtmlDocument();
			doc.LoadHtml(content);


			if (item.Type == "VOS")
			{
				item.ICO = GetInputValue(doc, "ico");

				item.OldNames = GetInputValue(doc, "predesleNazvy");
			}
			else if (item.Type == "FO")
			{
				item.SureName = GetInputValue(doc, "prijmeni");
				item.FirstName = GetInputValue(doc, "jmeno");
				item.OldSureNames = GetInputValue(doc, "predeslaPrijmeni");
				item.BirthDate = GetInputDate(doc, "datumNarozeni");

				item.ICO = GetInputValue(doc, "ic");

				item.CanDebtRelief = IsInputChecked(doc, "oddluzeni");
				item.CanKonkurs = IsInputChecked(doc, "konkurs");
			}

			item.Street = GetInputValue(doc, "ulice_S");
			item.Zip = GetInputValue(doc, "psc_S");
			item.Number = GetInputValue(doc, "cp_S");
			item.City = GetInputValue(doc, "mesto_S");
			item.District = GetInputValue(doc, "okres_S");

			item.DateOfEstablishment = GetInputDate(doc, "denVzniku");
			item.DateOfExam = GetInputDate(doc, "datumVykonaniZvlZk");
			item.DateOfSuspension = GetInputDate(doc, "denPozastaveni");
			item.ReasonOfSuspension = GetInputValue(doc, "duvodPozastaveni");
			item.DateOfTermination = GetInputDate(doc, "denZaniku");
			item.ReasonOfTermination = GetInputValue(doc, "duvodZaniku");

			if (item.Type == "VOS")
			{
				item.Companions = doc.DocumentNode.SelectNodes("//table[4]/tr").Skip(1).Select(c => new Companion
				{
					AdministratorId = item.Id,
					Name = c.ChildNodes[1].InnerText.Trim(),
					Surename = c.ChildNodes[3].InnerText.Trim(),
					BirthDate = GetDate(c.ChildNodes[5].InnerText.Trim()),
					DateOfSpecialExamination = GetDate(c.ChildNodes.Count > 9 ? c.ChildNodes[9].InnerText.Trim() : ""),
					CanDebtRelief = c.ChildNodes[7].SelectSingleNode("//input[@id='oddluzeni']").Attributes.Any(a => a.Name == "checked" || a.Name == "checked/"),
					CanKonkurs = c.ChildNodes[7].SelectSingleNode("//input[@id='konkurs']").Attributes.Any(a => a.Name == "checked" || a.Name == "checked/"),
				}).ToList();
			}

			item.BranchOffices = doc.DocumentNode.SelectNodes("//table[@width='100%']")
				.Where(c => c.Attributes.Count == 1)
				.Select(c => new BranchOffice {
					AdministratorId = item.Id,
					Address = c.ChildNodes[3].ChildNodes[3].InnerText.Trim(),
					District = c.ChildNodes[5].ChildNodes[3].InnerText.Trim(),
					Region = c.ChildNodes[7].ChildNodes[3].InnerText.Trim(),
					IsKonkursBranch = c.ChildNodes[9].ChildNodes[3].InnerText.Trim() == "Ano"
				}).ToList();
		}

		private string GetInputValue(HtmlDocument doc, string name)
		{
			return doc.DocumentNode.SelectSingleNode($"//input[@id='{name}']").GetAttributeValue("value", "").Trim();
		}

		private DateTime? GetDate(string value)
		{
			return string.IsNullOrEmpty(value) ? new DateTime?() : DateTime.Parse(value);
		}

		private DateTime? GetInputDate(HtmlDocument doc, string name)
		{
			return GetDate(GetInputValue(doc, name));
		}

		private bool IsInputChecked(HtmlDocument doc, string name)
		{
			return doc.DocumentNode.SelectSingleNode($"//input[@id='{name}']").Attributes.Any(a => a.Name == "checked" || a.Name == "checked/");
		}
	}
}
