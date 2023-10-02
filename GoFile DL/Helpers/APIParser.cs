using GoFile_DL.Entities;
using Spectre.Console;
using System;
using File = GoFile_DL.Entities.File;

namespace GoFile_DL.Helpers
{
	public class APIParser : IAPIParser
	{
		private static readonly IAPIHelper aPIHelper;

		static APIParser()
		{
			aPIHelper = new APIHelper();
		}
		public async Task<Folder> ParseApiResponse(GetContentResponse response, Config config)
		{
			if(response != null && response.data != null && response.data.contents != null)
			{
				Folder? rootFolder = new Folder
				{
					Id = response.data.id,
					Name = response.data.name,
					Folders = new List<Folder>(),
					Files = new List<File>()
				};

				await ParseContents(response.data.contents, rootFolder.Folders, rootFolder.Files, config);

				return rootFolder;
			}
			return new Folder();
		}

		public async Task ParseContents(Dictionary<string, GetContentResponse.Content> contents, List<Folder> folders, List<File> files, Config config)
		{
			foreach (var content in contents.Values)
			{
				if (content.type == "folder")
				{
					Folder folder = new Folder
					{
						Id = content.id,
						Name = content.name,
						Folders = new List<Folder>(),
						Files = new List<File>()
					};
					folders.Add(folder);

					if(content != null && content.name != null && content.id != null)
					{
						GetContentResponse? folderContentResponse = await FetchFolderContentAsync(content.name, content.id, config);
						if (folderContentResponse != null && folderContentResponse.data != null && folderContentResponse.data.contents != null)
						{
							await ParseContents(folderContentResponse.data.contents, folder.Folders, folder.Files, config);
						}
					}
				}
				else if (content.type == "file")
				{
					File file = new File
					{
						Id = content.id,
						Name = content.name,
						DownloadLink = content.link,
						Size = content.size,
						MD5Hash = content.md5
					};
					files.Add(file);
				}
			}
		}

		private async Task<GetContentResponse?> FetchFolderContentAsync(string folderName, string folderId, Config config)
		{
			GetContentResponse? getContentResponse = await aPIHelper.GetContent(folderId, config);
			if (getContentResponse != null && getContentResponse.status == "ok")
			{
				return getContentResponse;	
			}
			else if (getContentResponse != null && getContentResponse.status == "error-passwordRequired")
			{
				AnsiConsole.Markup($"[red]Folder {folderName} is password protected\n[/]");
				while (true)
				{
					string inputPassword = AnsiConsole.Prompt(new TextPrompt<string>("[red]Enter Password: [/]"));
					string hashedPassword = PasswordHelper.ComputeSHA256Hash(inputPassword);
					GetContentResponse? getContentResponseWithPassword = await aPIHelper.GetContentWithPassword(folderId, hashedPassword, config);
					if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "ok")
					{
						return getContentResponseWithPassword;
					}
					else if (getContentResponseWithPassword != null && getContentResponseWithPassword.status == "error-passwordWrong")
					{
						AnsiConsole.Markup("[red]Password is incorrect, please try again\n[/]");
						continue;
					}
				}
			}
			return null;
		}
	}
}
