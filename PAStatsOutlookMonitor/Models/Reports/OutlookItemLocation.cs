using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Models.Reports
{
	public class OutlookItemLocation
	{
		public string EmailId { get; set; }

		public string CurrentFolderName { get; set; }

		public string OldParentFolderName { get; set; }

		public DateTime ItemActionDate { get; set; }
	}
}
