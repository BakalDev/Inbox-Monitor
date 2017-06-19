namespace InMo.Models
{
	public class OutlookFolder
	{
		public int OutlookFolderId { get; set; }


		public string UniqueId { get; set; }

		public string DisplayName { get; set; }

		public int TotalCount { get; set; }

		public int TotalUnread { get; set; }

		public System.DateTime LastUpdated { get; set; }

		public string ParentFolderName { get; set; }
	}
}
