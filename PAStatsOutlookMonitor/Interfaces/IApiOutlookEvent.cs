using InMo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Interfaces
{
	public interface IApiOutlookEvent
	{
		bool PostOutlookEvent(OutlookEvent outlookEvent);
	}
}
