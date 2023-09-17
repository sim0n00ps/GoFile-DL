using GoFile_DL.Entities;
using GoFile_DL.Helpers;
using Newtonsoft.Json;
using Spectre.Console;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GoFile_DL.Entities.GetContentResponse;

namespace GoFile_DL 
{
	public class Program
	{
		public static Config? Config { get; set; } = null;
		public static List<string> folders = new List<string>();
		private static readonly IAPIHelper aPIHelper;

		static Program()
		{
			aPIHelper = new APIHelper();
		}
		public static async Task Main()
		{
			if (!File.Exists("Config.json"))
			{
				string configString = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
				File.WriteAllText("Config.json", configString);
			}
			Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));

			if (string.IsNullOrEmpty(Config?.Token) && Config != null)
			{
				Config.Token = await aPIHelper.CreateAccount();
				UpdateConfig();
			}

			string siteToken = await aPIHelper.GetSiteToken();
			if(Config != null && Config.Token != siteToken)
			{
				Config.SiteToken = siteToken;
				UpdateConfig();
			}

			string inputUrl = AnsiConsole.Prompt(
				new TextPrompt<string>("[red]Please enter a GoFile URL: [/]")
					.ValidationErrorMessage("[red]Please enter a valid GoFile link[/]")
					.Validate(url =>
					{
						Regex regex = new Regex("https://gofile\\.io/d/([A-Za-z]+(\\d[A-Za-z]+)+)", RegexOptions.IgnoreCase);
						if (regex.IsMatch(url))
						{
							return ValidationResult.Success();
						}
						return ValidationResult.Error("[red]Please enter a valid GoFile link[/]");
					}));

			GetContentResponse? getContentResponse = await aPIHelper.GetContent(GetCode(inputUrl), Config);
			//If response if ok then download content
			if (getContentResponse != null && getContentResponse.status == "ok")
			{
				HttpClient client = CreateHttpClient(Config.Token);
				if (!Path.Exists("Downloads"))
				{
					Directory.CreateDirectory("Downloads");
				}

				string path = $"Downloads/{getContentResponse.data.name}";
				if (!Path.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				foreach (KeyValuePair<string, Content> kvp in getContentResponse.data.contents)
				{
					switch (kvp.Value.type)
					{
						case "file":
							await AnsiConsole.Progress()
								.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
								.StartAsync(async ctx =>
								{
									var downloadTask = ctx.AddTask(kvp.Value.name);
									downloadTask.MaxValue = kvp.Value.size;
									await DownloadHelper.DownloadFile(path, kvp.Value.name, kvp.Value.link, client, Config, downloadTask);
								});
							break;
					}
				}
			}
			//If password required
			else if (getContentResponse != null && getContentResponse.status == "error-passwordRequired")
			{
				AnsiConsole.Markup("[red]Folder is password protected\n[/]");
				while (true)
				{
					string inputPassword = AnsiConsole.Prompt(new TextPrompt<string>("[red]Enter Password: [/]"));
					string hashedPassword = PasswordHelper.ComputeSHA256Hash(inputPassword);
					GetContentResponse? getContentResponseWithPassword = await aPIHelper.GetContentWithPassword(GetCode(inputUrl), hashedPassword, Config);
					if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "ok")
					{
						HttpClient client = CreateHttpClient(Config.Token);
						if (!Path.Exists("Downloads"))
						{
							Directory.CreateDirectory("Downloads");
						}

						string path = $"Downloads/{getContentResponseWithPassword.data.name}";
						if (!Path.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						foreach (KeyValuePair<string, Content> kvp in getContentResponseWithPassword.data.contents)
						{
							switch (kvp.Value.type)
							{
								case "file":
									await AnsiConsole.Progress()
										.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
										.StartAsync(async ctx =>
										{
											var downloadTask = ctx.AddTask(kvp.Value.name);
											downloadTask.MaxValue = kvp.Value.size;
											await DownloadHelper.DownloadFile(path, kvp.Value.name, kvp.Value.link, client, Config, downloadTask);
										});
									break;
							}
						}
						break;
					}
					else if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "error-passwordWrong")
					{
						AnsiConsole.Markup("[red]Password is incorrect, please try again\n[/]");
						continue;
					}
				}
			}
		}

		private static void UpdateConfig()
		{
			string configString = JsonConvert.SerializeObject(Config, Formatting.Indented);
			File.WriteAllText("Config.json", configString);
			Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
		}

		private static HttpClient CreateHttpClient(string token)
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Cookie", $"accountToken={token}");

			return client;
		}

		private static string GetCode(string url)
		{
			return url.Split("/")[4];
		}
	}
}