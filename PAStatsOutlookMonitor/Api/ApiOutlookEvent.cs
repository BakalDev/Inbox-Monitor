using InMo.Config;
using InMo.Interfaces;
using InMo.Models;
using System;
using System.Net;
using System.Threading;

namespace InMo.Api
{
	public class ApiOutlookEvent : IApiOutlookEvent
	{
		private readonly Uri outlookeventuri;

		public ApiOutlookEvent()
		{
			string baseUrl = string.Format("{0}{1}", ApiConfig.GetConfigurables().WebApiUrl.ToString(), "api/");

			outlookeventuri = new Uri(baseUrl + "outlookevents/");
		}

		public bool PostOutlookEvent(OutlookEvent outlookEvent)
		{
			try
			{
				Thread.Sleep(10000);
				var httpRequest = ApiHelper.ReturnRequest(outlookeventuri, outlookEvent);
				var request = (HttpWebResponse)httpRequest.GetResponse();

				// If we reach here then the event has been successfully handled by the api
				return true;
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				return false;
				//throw; // Todo Do we want to throw here?
			}
		}



	}
}
