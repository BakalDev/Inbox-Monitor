using InMo.Models;
using InMo.Models.Reports;
using InMo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Interfaces
{
	public interface IApiOutlookItem
	{
		void SeedOutlookItems(OutlookItemsAndEvent outlookItemsAndEvent);

		void PostOutlookItem(OutlookItem outlookItem);

		void PostOutlookItemAndEvent(OutlookItemAndEvent outlookItemEvent);

		void PostOutlookItemLocation(OutlookItem outlookItem); // OutlookItemLocation generated from OutlookItem model
	}
}
