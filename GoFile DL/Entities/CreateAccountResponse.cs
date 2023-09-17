using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFile_DL.Entities
{
	public class CreateAccountResponse
	{
		public string status { get; set; }
		public Data data { get; set; }
		public class Data
		{
			public string token { get; set; }
		}
	}
}
