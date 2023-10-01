using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Entities
{
	public class Folder
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public List<Folder> Folders { get; set; }
		public List<File> Files { get; set; }
		public string DownloadPath { get; set; }
		public Folder()
		{
			Folders = new List<Folder>();
			Files = new List<File>();
		}

		public async Task IterateFoldersAsync(Func<Folder, string, Task> folderActionAsync, string currentPath)
		{
			string folderPath = Path.Combine(currentPath, Name);

			await folderActionAsync(this, folderPath);

			foreach (var subfolder in Folders)
			{
				await subfolder.IterateFoldersAsync(folderActionAsync, folderPath);
			}
		}
	}
}
