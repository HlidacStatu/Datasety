using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PrijemciDotaci
{
	static class Utils
	{
		private static Regex NormalizePattern = new Regex("[^a-zA-Z0-9-]", RegexOptions.Compiled);

		public static string Normalize(string value)
		{
			return NormalizePattern.Replace(value, "_");
		}

		public static string CreateKey(string normalizedCompanyName, string year, int index)
		{
			return $"{normalizedCompanyName}-{year}-{index}";
		}

		public static string CalculateMD5Hash(string input)
		{
			var md5 = MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(input);
			var hash = md5.ComputeHash(inputBytes);

			var sb = new StringBuilder();
			for (var i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
