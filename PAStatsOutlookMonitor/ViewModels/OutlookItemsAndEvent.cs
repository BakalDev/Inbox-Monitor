using InMo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.ViewModels
{
	public class OutlookItemsAndEvent
	{
		public int OutlookItemsAndEventId { get; set; }

		public List<OutlookItem> OutlookItems { get; set; }

		public OutlookEvent OutlookEvent { get; set; }
	}
}
