using GoFile_DL.Entities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Helpers
{
	public class DownloadHelper 
	{
		public static async Task DownloadFile(string path, string filename, string url, HttpClient client, ProgressTask progressTask)
		{
			try
			{
				using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();

					using (var contentStream = await response.Content.ReadAsStreamAsync())
					using (var fileStream = new FileStream(path + "/" + filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
					{
						var buffer = new byte[8192];
						while (true)
						{
							var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
							if (read == 0)
							{
								break;
							}

							progressTask.Increment(read);

							await fileStream.WriteAsync(buffer, 0, read);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public static string CalculateMD5(string filePath)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = System.IO.File.OpenRead(filePath))
				{
					byte[] hash = md5.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLower();
				}
			}
		}

		public static bool VerifyMD5(string filePath, string expectedMD5)
		{
			string fileMD5 = CalculateMD5(filePath);
			return fileMD5.Equals(expectedMD5, StringComparison.OrdinalIgnoreCase);
		}
	}
}
