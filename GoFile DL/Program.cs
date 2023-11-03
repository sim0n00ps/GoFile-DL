using GoFile_DL.Entities;
using GoFile_DL.Helpers;
using Newtonsoft.Json;
using Spectre.Console;
using System;
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
		private static readonly IAPIParser aPIParser;

		static Program()
		{
			aPIHelper = new APIHelper();
			aPIParser = new APIParser();
		}
		public static async Task Main()
		{
			if (!System.IO.File.Exists("Config.json"))
			{
				string configString = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
                System.IO.File.WriteAllText("Config.json", configString);
			}
			Config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("Config.json"));

			if (string.IsNullOrEmpty(Config?.Token) && Config != null)
			{
				Config.Token = await aPIHelper.CreateAccount();
				UpdateConfig();
			}

			string? siteToken = await aPIHelper.GetSiteToken();
			if(Config != null && Config.Token != siteToken)
			{
				Config.SiteToken = siteToken;
				UpdateConfig();
			}

			do
			{
				var mainMenuOptions = GetMainMenuOptions();

				var mainMenuSelection = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[red]What would you like to do? | Download From Single URL - Download content from a single GoFile link | Batch Download URLS - Download all URLS in links.txt[/]")
						.AddChoices(mainMenuOptions)
				);

				switch(mainMenuSelection)
				{
					case "[red]Download From Single URL[/]":
						await SingleURLDownload();
						break;
					case "[red]Batch Download URLS[/]":
						await BatchURLDownload();
						break;
					case "[red]Exit[/]":
						Environment.Exit(0);
						break;
				}
			} while (true);
		}

		public static async Task SingleURLDownload()
		{
			try
			{
				if(Config != null)
				{
					string inputUrl = AnsiConsole.Prompt(
						new TextPrompt<string>("[red]Please enter a GoFile URL: [/]")
							.ValidationErrorMessage("[red]Please enter a valid GoFile link[/]")
							.Validate(url =>
							{
								Regex regex = new Regex("https://gofile\\.io/d/([A-Za-z0-9]+)", RegexOptions.IgnoreCase);
								if (regex.IsMatch(url))
								{
									return ValidationResult.Success();
								}
								return ValidationResult.Error("[red]Please enter a valid GoFile link[/]");
							}));

					GetContentResponse? getContentResponse = await aPIHelper.GetContent(GetCode(inputUrl), Config);
					if (getContentResponse != null && getContentResponse.status == "ok" && Config != null && !string.IsNullOrEmpty(Config.Token) && !string.IsNullOrEmpty(Config.SiteToken))
					{
						Folder rootFolder = await aPIParser.ParseApiResponse(getContentResponse, Config);
						if (!Path.Exists("Downloads"))
						{
							Directory.CreateDirectory("Downloads");
						}

						HttpClient client = CreateHttpClient(Config.Token);

						await rootFolder.IterateFoldersAsync(async (folder, folderPath) =>
						{
							if (!Directory.Exists(folderPath))
							{
								Directory.CreateDirectory(folderPath);
							}

							if (folder.Files.Count > 0 && folder.Name != null)
							{
								AnsiConsole.Markup($"[red]Downloading Content for Folder - {Markup.Escape(folder.Name)}[/]");
							}

							foreach (Entities.File file in folder.Files)
							{
								if(file != null && file.Name != null && file.DownloadLink != null && file.MD5Hash != null)
								{
									await AnsiConsole.Progress()
									.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
									.StartAsync(async ctx =>
									{
										var downloadTask = ctx.AddTask($"[red]{Markup.Escape(folderPath.Replace("\\", "/"))}/{Markup.Escape(file.Name)}[/]");
										downloadTask.MaxValue = file.Size;

										bool downloadSuccessful = false;

										do
										{
											await DownloadHelper.DownloadFile(folderPath, file.Name, file.DownloadLink, client, downloadTask);

											bool isMD5Valid = DownloadHelper.VerifyMD5(folderPath + "/" + file.Name, file.MD5Hash);

											if (isMD5Valid)
											{
												downloadSuccessful = true;
											}
											else
											{
												AnsiConsole.Markup($"MD5 hash check failed for {Markup.Escape(file.Name)}. Retrying download...");
												System.IO.File.Delete(folderPath + "/" + file.Name);
											}

										} while (!downloadSuccessful);
									});
								}
							}
						}, "Downloads");
					}
					else if (getContentResponse != null && getContentResponse.status == "error-passwordRequired" && Config != null && !string.IsNullOrEmpty(Config.Token))
					{
						AnsiConsole.Markup($"[red]Base Folder is password protected\n[/]");
						while (true)
						{
							string inputPassword = AnsiConsole.Prompt(new TextPrompt<string>("[red]Enter Password: [/]"));
							string hashedPassword = PasswordHelper.ComputeSHA256Hash(inputPassword);
							GetContentResponse? getContentResponseWithPassword = await aPIHelper.GetContentWithPassword(GetCode(inputUrl), hashedPassword, Config);
							if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "ok")
							{
								Folder rootFolder = await aPIParser.ParseApiResponse(getContentResponseWithPassword, Config);
								if (!Path.Exists("Downloads"))
								{
									Directory.CreateDirectory("Downloads");
								}

								HttpClient client = CreateHttpClient(Config.Token);

								await rootFolder.IterateFoldersAsync(async (folder, folderPath) =>
								{
									if (!Directory.Exists(folderPath))
									{
										Directory.CreateDirectory(folderPath);
									}

									foreach (Entities.File file in folder.Files)
									{
										if (file != null && file.Name != null && file.DownloadLink != null && file.MD5Hash != null)
										{
											await AnsiConsole.Progress()
												.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
												.StartAsync(async ctx =>
												{
													var downloadTask = ctx.AddTask($"[red]{Markup.Escape(folderPath.Replace("\\", "/"))}/{Markup.Escape(file.Name)}[/]");
													downloadTask.MaxValue = file.Size;

													bool downloadSuccessful = false;

													do
													{
														await DownloadHelper.DownloadFile(folderPath, file.Name, file.DownloadLink, client, downloadTask);

														bool isMD5Valid = DownloadHelper.VerifyMD5(folderPath + "/" + file.Name, file.MD5Hash);

														if (isMD5Valid)
														{
															downloadSuccessful = true;
														}
														else
														{
															AnsiConsole.Markup($"MD5 hash check failed for {Markup.Escape(file.Name)}. Retrying download...");
															System.IO.File.Delete(folderPath + "/" + file.Name);
														}

													} while (!downloadSuccessful);
												});
										}
									}
								}, "Downloads");
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
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.Message, ex.StackTrace);

				if (ex.InnerException != null)
				{
					Console.WriteLine("\nInner Exception:");
					Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.InnerException.Message, ex.InnerException.StackTrace);
				}
			}
		}
		public static async Task BatchURLDownload()
		{
			try
			{
				if(Config != null && !string.IsNullOrEmpty(Config.Token))
				{
					using (StreamReader sr = new StreamReader("Links.txt"))
					{
						string? line;

						while ((line = sr.ReadLine()) != null)
						{
							Regex regex = new Regex("https://gofile\\.io/d/([A-Za-z]+)", RegexOptions.IgnoreCase);
							if (regex.IsMatch(line))
							{
								GetContentResponse? getContentResponse = await aPIHelper.GetContent(GetCode(line), Config);
								if (getContentResponse != null && getContentResponse.status == "ok")
								{
									Folder rootFolder = await aPIParser.ParseApiResponse(getContentResponse, Config);
									if (!Path.Exists("Downloads"))
									{
										Directory.CreateDirectory("Downloads");
									}

									HttpClient client = CreateHttpClient(Config.Token);

									await rootFolder.IterateFoldersAsync(async (folder, folderPath) =>
									{
										if (!Directory.Exists(folderPath))
										{
											Directory.CreateDirectory(folderPath);
										}

										if (folder.Files.Count > 0 && folder.Name != null)
										{
											AnsiConsole.Markup($"[red]Downloading Content for Folder - {Markup.Escape(folder.Name)}[/]");
										}

										foreach (Entities.File file in folder.Files)
										{
											if (file != null && file.Name != null && file.DownloadLink != null && file.MD5Hash != null)
											{
												await AnsiConsole.Progress()
													.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
													.StartAsync(async ctx =>
													{
														var downloadTask = ctx.AddTask($"[red]{Markup.Escape(folderPath.Replace("\\", "/"))}/{Markup.Escape(file.Name)}[/]");
														downloadTask.MaxValue = file.Size;

														bool downloadSuccessful = false;

														do
														{
															await DownloadHelper.DownloadFile(folderPath, file.Name, file.DownloadLink, client, downloadTask);

															bool isMD5Valid = DownloadHelper.VerifyMD5(folderPath + "/" + file.Name, file.MD5Hash);

															if (isMD5Valid)
															{
																downloadSuccessful = true;
															}
															else
															{
																AnsiConsole.Markup($"MD5 hash check failed for {Markup.Escape(file.Name)}. Retrying download...");
																System.IO.File.Delete(folderPath + "/" + file.Name);
															}

														} while (!downloadSuccessful);
													});
											}
										}
									}, "Downloads");
								}
								else if (getContentResponse != null && getContentResponse.status == "error-passwordRequired")
								{
									AnsiConsole.Markup($"[red]Base Folder is password protected\n[/]");
									while (true)
									{
										string inputPassword = AnsiConsole.Prompt(new TextPrompt<string>("[red]Enter Password: [/]"));
										string hashedPassword = PasswordHelper.ComputeSHA256Hash(inputPassword);
										GetContentResponse? getContentResponseWithPassword = await aPIHelper.GetContentWithPassword(GetCode(line), hashedPassword, Config);
										if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "ok")
										{
											Folder rootFolder = await aPIParser.ParseApiResponse(getContentResponseWithPassword, Config);
											if (!Path.Exists("Downloads"))
											{
												Directory.CreateDirectory("Downloads");
											}

											HttpClient client = CreateHttpClient(Config.Token);

											await rootFolder.IterateFoldersAsync(async (folder, folderPath) =>
											{
												if (!Directory.Exists(folderPath))
												{
													Directory.CreateDirectory(folderPath);
												}

												foreach (Entities.File file in folder.Files)
												{
													if (file != null && file.Name != null && file.DownloadLink != null && file.MD5Hash != null)
													{
														await AnsiConsole.Progress()
															.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new DownloadedColumn(), new RemainingTimeColumn())
															.StartAsync(async ctx =>
															{
																var downloadTask = ctx.AddTask($"[red]{Markup.Escape(folderPath.Replace("\\", "/"))}/{Markup.Escape(file.Name)}[/]");
																downloadTask.MaxValue = file.Size;

																bool downloadSuccessful = false;

																do
																{
																	await DownloadHelper.DownloadFile(folderPath, file.Name, file.DownloadLink, client, downloadTask);

																	bool isMD5Valid = DownloadHelper.VerifyMD5(folderPath + "/" + file.Name, file.MD5Hash);

																	if (isMD5Valid)
																	{
																		downloadSuccessful = true;
																	}
																	else
																	{
																		AnsiConsole.Markup($"MD5 hash check failed for {Markup.Escape(file.Name)}. Retrying download...");
																		System.IO.File.Delete(folderPath + "/" + file.Name);
																	}

																} while (!downloadSuccessful);
															});
													}
												}
											}, "Downloads");
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
							else
							{
								AnsiConsole.Markup($"[red]{line} is not a valid GoFile URL\n[/]");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.Message, ex.StackTrace);

				if (ex.InnerException != null)
				{
					Console.WriteLine("\nInner Exception:");
					Console.WriteLine("Exception caught: {0}\n\nStackTrace: {1}", ex.InnerException.Message, ex.InnerException.StackTrace);
				}
			}
		}

		private static void UpdateConfig()
		{
			string configString = JsonConvert.SerializeObject(Config, Formatting.Indented);
			System.IO.File.WriteAllText("Config.json", configString);
			Config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("Config.json"));
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
		public static List<string> GetMainMenuOptions()
		{
			return new List<string>
			{
				"[red]Download From Single URL[/]",
				"[red]Batch Download URLS[/]",
				"[red]Exit[/]"
			};
		}
	}
}