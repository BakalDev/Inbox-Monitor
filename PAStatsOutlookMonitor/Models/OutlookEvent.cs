using System;

namespace InMo.Models
{
	public class OutlookEvent
	{
		public int OutlookEventId { get; set; }


		public string FolderAsset { get; set; }

		public string ItemAsset { get; set; }

		public string EventType { get; set; }

		public DateTime EventDate { get; set; }

		public string User { get; set; }

		public int? PreviousTotal { get; set; }

		public int? PreviousUnread { get; set; }

		public int? CurrentTotal { get; set; }

		public int? CurrentUnread { get; set; }
	}
}
