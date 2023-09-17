using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Helpers
{
	public class PasswordHelper
	{
		public static string ComputeSHA256Hash(string input)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(input);
				byte[] hashBytes = sha256.ComputeHash(inputBytes);

				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					builder.Append(hashBytes[i].ToString("x2"));
				}

				return builder.ToString();
			}
		}
	}
}
