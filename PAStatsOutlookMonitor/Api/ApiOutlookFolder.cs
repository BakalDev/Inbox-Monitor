using InMo.Config;
using InMo.Interfaces;
using InMo.Models;
using InMo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Api
{
	public class ApiOutlookFolder : IApiOutlookFolder
	{
		private readonly Uri outlookFolderAndEventUri;
		private readonly Uri seedAndEventUri;

		public ApiOutlookFolder()
		{
			string baseUrl = string.Format("{0}{1}", ApiConfig.GetConfigurables().WebApiUrl.ToString(), "api/");
			
			outlookFolderAndEventUri = new Uri(baseUrl + "outlookfolderandevents/");
			seedAndEventUri = new Uri(baseUrl + "outlookfoldersandevents/"); // Todo We need a seperate seed endpoint for now?
		}

		public void SeedOutlookFolders(OutlookFoldersAndEvent outlookFoldersAndEvent)
		{
			try
			{
				var httpRequest = ApiHelper.ReturnRequest(seedAndEventUri, outlookFoldersAndEvent);
				var request = (HttpWebResponse)httpRequest.GetResponse();

				// Todo Loop request object and write
			}	
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}
		}

		public void PostOutlookFolderAndEvent(OutlookFolderAndEvent outlookFolderAndEvent)
		{
			try
			{
				var httprequest = ApiHelper.ReturnRequest(outlookFolderAndEventUri, outlookFolderAndEvent);
				var request = (HttpWebResponse)httprequest.GetResponse();

				// Todo Loop request object and write
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}
		}
	}
}
