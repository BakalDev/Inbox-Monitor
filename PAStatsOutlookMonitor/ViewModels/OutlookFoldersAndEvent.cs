using InMo.Models;
using System.Collections.Generic;

namespace InMo.ViewModels
{
	public class OutlookFoldersAndEvent
	{
		public int OutlookFoldersAndEventId { get; set; }


		public List<OutlookFolder> OutlookFolders { get; set; }

		public OutlookEvent OutlookEvent { get; set; }
	}
}
