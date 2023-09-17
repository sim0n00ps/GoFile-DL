using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Entities
{
	public class GetContentResponse
	{
		public string status { get; set; }
		public Data data { get; set; }
		public class Data
		{
			public bool isOwner { get; set; }
			public string id { get; set; }
			public string type { get; set; }
			public string name { get; set; }
			public string parentFolder { get; set; }
			public string code { get; set; }
			public long createTime { get; set; }
			public bool @public { get; set; }
			public List<string> childs { get; set; }
			public int totalDownloadCount { get; set; }
			public long totalSize { get; set; }
			public Dictionary<string, Content> contents { get; set; }
		}

		public class Content
		{
			public string id { get; set; }
			public string type { get; set; }
			public string name { get; set; }
			public string parentFolder { get; set; }
			public long createTime { get; set; }
			public List<string> childs { get; set; }
			public string code { get; set; }
			public bool @public { get; set; }
			public long size { get; set; }
			public int downloadCount { get; set; }
			public string md5 { get; set; }
			public string mimetype { get; set; }
			public string serverChoosen { get; set; }
			public string link { get; set; }
			public bool overloaded { get; set; }
		}
	}
}
