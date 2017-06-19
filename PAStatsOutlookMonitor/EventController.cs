using InMo.Models;
using InMo.Interfaces;
using System.Collections.Generic;
using InMo.ViewModels;

namespace InMo
{
    public class EventController
    {
		
		private readonly IEwsSubscriptionService _ewsSubscriptionService;

		private readonly IApiOutlookEvent _apiEvent;
		private readonly IApiOutlookFolder _apiFolder;
		private readonly IApiOutlookItem _apiItem;

		public EventController(IEwsSubscriptionService ewsSubscriptionService, IApiOutlookEvent apiEvent, IApiOutlookFolder apiFolder, IApiOutlookItem apiItem)
		{
			_ewsSubscriptionService = ewsSubscriptionService;
			_apiEvent = apiEvent;
			_apiFolder = apiFolder;
			_apiItem = apiItem;

			_ewsSubscriptionService.FolderSeedDataReceived += OnFolderSeedDataReceived;
			_ewsSubscriptionService.FolderEventReceived += OnFolderEventReceived;
			_ewsSubscriptionService.ItemEventReceived += OnItemEventReceived;
			ewsSubscriptionService.ItemSeedReceived += OnItemSeedReceived;

			_ewsSubscriptionService.InstantiateEwsService();
			
		}

		// When seed folder event raised, pass the list of folders within event to PostOutlookFolders() in API.
		public void OnFolderSeedDataReceived(object sender, OutlookFoldersAndEvent ofe)
		{
			_apiFolder.SeedOutlookFolders(ofe);
		}

		// When folder event raised, pass the list of folders within event to PostOutlookFolders() in API.
		public void OnFolderEventReceived(object sender, OutlookFolderAndEvent ofe)
		{
			_apiFolder.PostOutlookFolderAndEvent(ofe);
		}

		// TODO: When item event raised, pass the item within event to API.
		public void OnItemEventReceived(object sender, OutlookItemAndEvent oie)
		{
			_apiItem.PostOutlookItemAndEvent(oie);
		}

		public void OnItemSeedReceived(object sender, OutlookItemsAndEvent oie)
		{
			_apiItem.SeedOutlookItems(oie);
		}




	}
}
