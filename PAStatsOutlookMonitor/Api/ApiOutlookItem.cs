using InMo.Config;
using InMo.Interfaces;
using InMo.Models;
using InMo.Models.Reports;
using InMo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Api
{
	public class ApiOutlookItem : IApiOutlookItem
	{
		private readonly Uri outlookItemAndEventUri;
		private readonly Uri outlookItemLocationUri;
		private readonly Uri outlookItemsAndEventUri;

		public ApiOutlookItem()
		{
			string baseUrl = string.Format("{0}{1}", ApiConfig.GetConfigurables().WebApiUrl.ToString(), "api/");

			outlookItemAndEventUri = new Uri(baseUrl + "outlookitemandevent/");
			outlookItemLocationUri = new Uri(baseUrl + "outlookitemlocation/");
			outlookItemsAndEventUri = new Uri(baseUrl + "outlookitemsandevent/");
		}

		public void PostOutlookItem(OutlookItem outlookItem)
		{
			try
			{
				var httpRequest = ApiHelper.ReturnRequest(outlookItemAndEventUri, outlookItem);
				var request = (HttpWebResponse)httpRequest.GetResponse();

				// The item has been added so now we want to pass this to OutlookItemLocations
				PostOutlookItemLocation(outlookItem);
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}
		}

		/// <summary>
		/// Posts outlookitem and the event to the api
		/// </summary>
		/// <param name="outlookFolderAndEvent"></param>
		public void PostOutlookItemAndEvent(OutlookItemAndEvent outlookItemEvent)
		{
			try
			{
				var httpRequest = ApiHelper.ReturnRequest(outlookItemAndEventUri, outlookItemEvent);
				var request = (HttpWebResponse)httpRequest.GetResponse();

				// The item has been added so now we want to pass this to OutlookItemLocations
				PostOutlookItemLocation(outlookItemEvent.OutlookItem);
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}
		}

		// Todo Move this to 'reporting'
		public void PostOutlookItemLocation(OutlookItem outlookItem)
		{
			try
			{
				// Create item location 
				OutlookItemLocation outlookItemLocation = new OutlookItemLocation
				{
					CurrentFolderName = outlookItem.ParentFolderName,
					OldParentFolderName = outlookItem.OldParentFolderName,
					ItemActionDate = DateTime.Now,
					EmailId = outlookItem.EmailId
				};

				var httpRequest = ApiHelper.ReturnRequest(outlookItemLocationUri, outlookItemLocation);
				var request = (HttpWebResponse)httpRequest.GetResponse();

				// The item has been added so now we want to pass this to OutlookItemLocations
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}
		}

		public void SeedOutlookItems(OutlookItemsAndEvent outlookItemsAndEvent)
		{
			try
			{
				var httpRequest = ApiHelper.ReturnRequest(outlookItemsAndEventUri, outlookItemsAndEvent);
				var request = (HttpWebResponse)httpRequest.GetResponse();
			}
			catch (WebException ex)
			{
				Console.Write(ex.Message.ToString());
				throw;
			}

		}

	}
}
