using GoFile_DL.Entities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace GoFile_DL.Helpers
{
	public class APIHelper : IAPIHelper
	{
		public async Task<string?> CreateAccount()
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("https://api.gofile.io/createAccount")
				};
				using (var response = await client.SendAsync(request))
				{
					response.EnsureSuccessStatusCode();
					string body = await response.Content.ReadAsStringAsync();
					CreateAccountResponse? account = JsonConvert.DeserializeObject<CreateAccountResponse?>(body);
					if (account != null && account.data != null && account.data.token != null && account.status == "ok")
					{
						return account.data.token;
					}
					else
					{
						throw new Exception("Error creating GoFile account");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

		public async Task<string?> GetSiteToken()
		{
			try
			{
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("https://gofile.io/dist/js/alljs.js")
				};
				using (var response = await client.SendAsync(request))
				{
					response.EnsureSuccessStatusCode();
					string body = await response.Content.ReadAsStringAsync();
					string pattern = @"fetchData\.websiteToken\s*=\s*""(.*?)""";
					Match match = Regex.Match(body, pattern);
					if (match.Success)
					{
						string websiteToken = match.Groups[1].Value;
						if (!string.IsNullOrEmpty(websiteToken))
						{
							return websiteToken;
						}
					}
					return null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

		public async Task<GetContentResponse?> GetContent(string contentId, Config config)
		{
			try
			{
				if(config != null && !string.IsNullOrEmpty(config.Token) && !string.IsNullOrEmpty(config.SiteToken))
				{
					Dictionary<string, string> getParams = new()
					{
						{ "contentId", contentId },
						{ "token", config.Token },
						{ "websiteToken", config.SiteToken }
					};
					string queryParams = "?" + string.Join("&", getParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
					HttpClient client = new HttpClient();
					HttpRequestMessage request = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri("https://api.gofile.io/getContent" + queryParams)
					};
					using (var response = await client.SendAsync(request))
					{
						response.EnsureSuccessStatusCode();
						string body = await response.Content.ReadAsStringAsync();
						GetContentResponse? content = JsonConvert.DeserializeObject<GetContentResponse?>(body);
						if (content != null)
						{
							return content;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}
		public async Task<GetContentResponse?> GetContentWithPassword(string contentId, string password, Config config)
		{
			try
			{
				if(config != null && !string.IsNullOrEmpty(config.Token) && !string.IsNullOrEmpty(config.SiteToken))
				{
					Dictionary<string, string> getParams = new()
					{
						{ "contentId", contentId },
						{ "token", config.Token },
						{ "websiteToken", config.SiteToken },
						{ "password", password }
					};
					string queryParams = "?" + string.Join("&", getParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
					HttpClient client = new HttpClient();
					HttpRequestMessage request = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri("https://api.gofile.io/getContent" + queryParams)
					};
					using (var response = await client.SendAsync(request))
					{
						response.EnsureSuccessStatusCode();
						string body = await response.Content.ReadAsStringAsync();
						GetContentResponse? content = JsonConvert.DeserializeObject<GetContentResponse?>(body);
						if (content != null)
						{
							return content;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}
	}
}
