using GoFile_DL.Entities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Helpers
{
	public class DownloadHelper 
	{
		public static async Task DownloadFile(string path, string filename, string url, HttpClient client, Config config, ProgressTask task)
		{
			try
			{
				using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();
					task.MaxValue(response.Content.Headers.ContentLength ?? 0);
					task.StartTask();

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

							task.Increment(read);

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
	}
}
