using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Entities
{
	public class File
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public string? DownloadLink { get; set; }
		public long Size { get; set; }
		public string? MD5Hash { get; set; }
	}
}
