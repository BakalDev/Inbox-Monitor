using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Models
{
	public class OutlookItem
	{
		public int OutlookItemId { get; set; }


		public string ParentFolderName { get; set; }

		public string OldParentFolderName { get; set; }

		public string EmailAddress { get; set; }

		public string EmailSubject { get; set; }

		public string EmailId { get; set; } // Todo This is the item Id right?

		public DateTime DateTimeReceived { get; set; }

		public string Importance { get; set; }

		public string Categories { get; set; }

		public string CustomUniqueId { get; set; } = string.Empty; // Todo This is set in the api
	}
}
