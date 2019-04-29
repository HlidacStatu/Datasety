using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrijemciDotaci
{
	class ApiClient
	{
		private readonly HttpClient Client;

		public ApiClient(string token)
		{
			Client = new HttpClient();
			Client.DefaultRequestHeaders.Add("Authorization", token);
		}

		public async Task<GetIcoByNameResult> GetIcoByName(string name)
		{
			var content = await Client.GetStringAsync("https://www.hlidacstatu.cz/api/v1/FindCompanyId?companyName=" + name);
			var result = JsonConvert.DeserializeObject<GetIcoByNameResult>(content);

			if (result.Jmeno != name)
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write($" !!!!!!!!!!! ");
				Console.ForegroundColor = color;

				result.ICO = result.ICO;
				result.Nalezeno = false;
				return result;
			}

			Console.Write($" ({result.ICO}) ");

			result.Nalezeno = true;
			return result;
		}
	}

	class GetIcoByNameResult
	{
		public string ICO { get; set; }
		public string Jmeno { get; set; }
		public bool Nalezeno { get; set; }
	}
}
