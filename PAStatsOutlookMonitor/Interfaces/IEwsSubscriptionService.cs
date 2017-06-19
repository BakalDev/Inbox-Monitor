using System;
using InMo.Models;
using InMo.ViewModels;
using System.Collections.Generic;

namespace InMo.Interfaces
{
    public interface IEwsSubscriptionService
    {
        
        event EventHandler<OutlookFolderAndEvent> FolderEventReceived;
        event EventHandler<OutlookFoldersAndEvent> FolderSeedDataReceived;
        event EventHandler<OutlookItemAndEvent> ItemEventReceived;
		event EventHandler<OutlookItemsAndEvent> ItemSeedReceived;

		void InstantiateEwsService();
    }
}
