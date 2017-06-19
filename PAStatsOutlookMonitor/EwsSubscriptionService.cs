using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Exchange.WebServices.Data;
using InMo.Models;
using InMo.Interfaces;
using InMo.Config;
using InMo.ViewModels;
using InMo.Api;
using System.Reflection;
using Serilog;
using System.Threading;

namespace InMo
{

	public class EwsSubscriptionService : IEwsSubscriptionService
	{
		// private readonly IMessageHub _messageHub;
		private static ExchangeService service;

		public event EventHandler<OutlookFolderAndEvent> FolderEventReceived;
		public event EventHandler<OutlookFoldersAndEvent> FolderSeedDataReceived;
		public event EventHandler<OutlookItemAndEvent> ItemEventReceived;
		public event EventHandler<OutlookItemsAndEvent> ItemSeedReceived;

		/// <summary>
		/// Creates an instance of the ExchangeService
		/// </summary>
		public void InstantiateEwsService()
		{
			// Todo Dave are you happy to use the constructor to replace the BuildExchangeConnection() method
			// On instantiation of the EwsSubscriptionService, generate the ExchangeConnection. 
			// Return if service is already established
			if (service != null) { return; }

			service = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
			{
				TraceEnabled = true,
				//Url = new Uri(@"https://casarray.cccs.co.uk/EWS/Exchange.asmx"),;

				Credentials = new WebCredentials(
						MailboxConfig.GetConfigurables().OutlookMailboxUsername,
						MailboxConfig.GetConfigurables().OutlookMailboxPassword,
						MailboxConfig.GetConfigurables().OutlookMailboxDomain)
			};

			service.AutodiscoverUrl(MailboxConfig.GetConfigurables().OutlookMailbox.ToString());

			// Log it
			Log.Information("Instantiated EWS Service: {@ExchangeService}", service);

			// Initiate folder seed
			FolderSeed();

			// Initiate item seed later (30s)
			System.Threading.Tasks.Task.Delay(20000).ContinueWith(t => SeedEmailItems());

			// Now capture events
			StreamingSubscription();
		}

		/// <summary>
		/// This was to dispose of the existing service and implement a fresh one to avoid timeout issues.
		/// </summary>
		public void InstantiateNewEwsService()
		{
			// Create a new service instance
			ExchangeService newService = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
			{
				TraceEnabled = true,
				Credentials = new WebCredentials(
						MailboxConfig.GetConfigurables().OutlookMailboxUsername,
						MailboxConfig.GetConfigurables().OutlookMailboxPassword,
						MailboxConfig.GetConfigurables().OutlookMailboxDomain),
			};

			newService.AutodiscoverUrl(MailboxConfig.GetConfigurables().OutlookMailbox.ToString());

			service = null;
			service = newService;

			// Log it
			Log.Information("A new insance of the EWS Service was instantiated: {@ExchangeService}", service);

			System.Threading.Tasks.Task.Delay(1740000).ContinueWith(t => InstantiateNewEwsService());

			FolderSeed();

			// Now capture events
			StreamingSubscription();
		}

		/// <summary>
		/// Builds the subscription stream
		/// </summary>
		private void StreamingSubscription()
		{
			try
			{
				// Specify events to subscribe to
				StreamingSubscription notificationStream = service.SubscribeToStreamingNotifications(
					EwsSubscriptionHelper.GetFolders(service),
					EwsSubscriptionHelper.SubscriptionEventTypes()
					);

				// Open a subscription connection and bind to events
				StreamingSubscriptionConnection con = new StreamingSubscriptionConnection(service, 30);
				con.AddSubscription(notificationStream);

				con.OnNotificationEvent += new StreamingSubscriptionConnection.NotificationEventDelegate(SubscriptionEvent);
				con.OnSubscriptionError += new StreamingSubscriptionConnection.SubscriptionErrorDelegate(SubscriptionError);
				con.OnDisconnect += new StreamingSubscriptionConnection.SubscriptionErrorDelegate(SubscriptionDisconnect);

				// Open the connection
				con.Open();

				// Log it
				Log.Information("EWS subscription listener implemented: {@StreamingSubscriptionConnection}", con);
			}
			catch (Exception ex)
			{
				EventLogger.WriteToLog(string.Format("Failed to build subscription to the Exchange. {0}", ex.Message.ToString()),
					System.Diagnostics.EventLogEntryType.Error);

				// Log it
				Log.Error("Failed to build the subcription to the Exchange: {exception}", ex.Message.ToString());
				//throw;
			}
		}

		/// <summary>
		/// An event has been captured from the subscription so deal with it alright
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SubscriptionEvent(object sender, NotificationEventArgs args)
		{
			// Create streaming subscription from args
			StreamingSubscription subscription = args.Subscription;

			foreach (NotificationEvent notification in args.Events)
			{
				// Display the notification identifier. 
				if (notification is ItemEvent)
				{
					ItemEvent itemEvent = (ItemEvent)notification;
					RaiseItemEvent(sender, itemEvent);
				}
				else
				{
					FolderEvent folderEvent = (FolderEvent)notification;
					RaiseFolderEvent(sender, notification.EventType, folderEvent.FolderId);
				}

				// Log it
				Log.Information("A notification event has been captured: {@NotificationEvent}", notification);
			}
		}

		/// <summary>
		/// An error has been captured from the subscription so deal with it alright
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SubscriptionError(object sender, SubscriptionErrorEventArgs args)
		{
			EventLogger.WriteToLog(string.Format("Failed to subscribe to the Exchange. {0}", args.ToString()),
						System.Diagnostics.EventLogEntryType.Error);

		}

		/// <summary>
		/// The subscription has been disconnected. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SubscriptionDisconnect(object sender, SubscriptionErrorEventArgs args)
		{
			try
			{
				// Reinstantiate the subscription
				StreamingSubscription();
			}
			catch (Exception ex)
			{
				EventLogger.WriteToLog(string.Format("Failed to maintain subscription to the Exchange. {0}", ex.ToString()),
						System.Diagnostics.EventLogEntryType.Error);
				//throw; // Todo: Do we want this to throw?
			}
		}

		/// <summary>
		/// FolderSeed basically calls the web api to perform functionality against each result of ReturnInboxFolders
		/// </summary>
		public void FolderSeed()
		{
			List<OutlookFolder> outlookFolders = new List<OutlookFolder>();

			try
			{
				foreach (FolderId folderId in EwsSubscriptionHelper.GetFolders(service))
				{
					// Bind folderId to Exchange.Folder
					Folder folder = Folder.Bind(service, folderId);

					string parentFolderName = string.Empty;
					if (!string.IsNullOrEmpty(folder.ParentFolderId.ToString()) && Folder.Bind(service, folder.ParentFolderId) != null)
					{
						Folder parFolder = Folder.Bind(service, folder.ParentFolderId);
						parentFolderName = (!string.IsNullOrEmpty(parFolder.DisplayName.ToString())) ? parFolder.DisplayName.ToString() : string.Empty;
					}

					// Bind the Exchange Folder to type OutlookFolder
					OutlookFolder outlookFolder = new OutlookFolder
					{
						TotalCount = folder.TotalCount,
						DisplayName = (!string.IsNullOrEmpty(folder.DisplayName.ToString())) ? folder.DisplayName.ToString() : string.Empty,
						UniqueId = folder.Id.ToString(),
						TotalUnread = folder.UnreadCount,
						LastUpdated = DateTime.Now,
						ParentFolderName = parentFolderName
					};
					outlookFolders.Add(outlookFolder);
				}
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message.ToString());
				//throw;
			}

			// create a new item of event
			OutlookEvent outlookEvent = new OutlookEvent
			{
				EventDate = DateTime.Now,
				EventType = "Folder Seed",
				CurrentTotal = null,
				CurrentUnread = null,
				FolderAsset = string.Empty,
				ItemAsset = string.Empty,
				PreviousTotal = null,
				PreviousUnread = null,
				User = "System raised event"
			};

			OutlookFoldersAndEvent outlookFoldersandEvent = new OutlookFoldersAndEvent
			{
				OutlookEvent = outlookEvent,
				OutlookFolders = outlookFolders
			};

			FolderSeedDataReceived?.Invoke(this, outlookFoldersandEvent);

			// Log it
			Log.Information("Folder seed completed: {@OutlookFoldersAndEvent}", outlookFoldersandEvent);
		}

		/// <summary>
		/// Handles events raised against type of ItemEvent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="itemEvent"></param>
		private void RaiseItemEvent(object sender, NotificationEvent itemEvent)
		{
			try
			{
				var item = (ItemEvent)itemEvent;

				// Initialise with defaults
				OutlookItemAndEvent outlookItemAndEvent = new OutlookItemAndEvent
				{
					OutlookEvent = new OutlookEvent
					{
						CurrentTotal = null,
						CurrentUnread = null,
						EventDate = DateTime.Now,
						EventType = item.EventType.ToString(),
						FolderAsset = string.Empty,
						ItemAsset = item.ItemId.ToString(),
						PreviousTotal = null,
						PreviousUnread = null,
						User = "Unknown user"
					},
					OutlookItem = new OutlookItem
					{
						Categories = string.Empty,
						DateTimeReceived = DateTime.Now, // Todo Default value for item received date?
						EmailAddress = string.Empty,
						EmailSubject = string.Empty,
						Importance = string.Empty,
						OldParentFolderName = string.Empty,
						ParentFolderName = string.Empty,
						EmailId = item.ItemId.ToString()
					}
				};

				// Email Message
				if (EwsSubscriptionHelper.ReturnEmailMessage(item, service) != null)
				{
					var email = EwsSubscriptionHelper.ReturnEmailMessage(item, service);
					if (email == null) { return; } // Todo Do we want to return here?

					// Categories
					outlookItemAndEvent.OutlookItem.Categories = EwsSubscriptionHelper.StringifyCategories(email.Categories);

					// DateTimeReceived
					if (!string.IsNullOrEmpty(email.DateTimeReceived.ToShortDateString()))
						outlookItemAndEvent.OutlookItem.DateTimeReceived = email.DateTimeReceived;

					// EmailAddress
					if (!string.IsNullOrEmpty(email.From.Address))
						outlookItemAndEvent.OutlookItem.EmailAddress = EwsSubscriptionHelper.AnonymiseEmail(email.From.Address);

					// EmailSubject
					if (!string.IsNullOrEmpty(email.Subject))
						outlookItemAndEvent.OutlookItem.EmailSubject = EwsSubscriptionHelper.AnonymiseSubject(email.Subject);

					// Importance
					if (!string.IsNullOrEmpty(email.Importance.ToString()))
						outlookItemAndEvent.OutlookItem.Importance = email.Importance.ToString();
				}

				// Old parent folder
				if (itemEvent.OldParentFolderId != null)
				{
					if (Folder.Bind(service, item.OldParentFolderId) != null)
						outlookItemAndEvent.OutlookItem.OldParentFolderName = Folder.Bind(service, item.OldParentFolderId).DisplayName.ToString();
				}

				// Parent Folder
				if (!string.IsNullOrEmpty(itemEvent.ParentFolderId.ToString()))
				{
					if (Folder.Bind(service, item.ParentFolderId) != null)
						outlookItemAndEvent.OutlookItem.ParentFolderName = Folder.Bind(service, item.ParentFolderId).DisplayName.ToString();
				}

				//EventLogger.WriteToLog(string.Format(outlookItemAndEvent.OutlookItem.EmailId.ToString() + " item event captured of type: " + outlookItemAndEvent.OutlookEvent.EventType.ToString()),
				//		System.Diagnostics.EventLogEntryType.Information);

				ItemEventReceived?.Invoke(this, outlookItemAndEvent);
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message.ToString());
			}
		}

		/// <summary>
		/// Handles events raised against type of FolderEvent.
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="type">Type of event</param>
		/// /// <param name="folderId">Unique Id of folder event is passed from</param>
		private void RaiseFolderEvent(object sender, EventType type, FolderId folderId)
		{
			try
			{
				var folder = Folder.Bind(service, folderId);

				string parentFolderName = string.Empty;
				if (!string.IsNullOrEmpty(folder.ParentFolderId.ToString()) && Folder.Bind(service, folder.ParentFolderId) != null)
				{
					Folder parFolder = Folder.Bind(service, folder.ParentFolderId);
					parentFolderName = (!string.IsNullOrEmpty(parFolder.DisplayName.ToString())) ? parFolder.DisplayName.ToString() : string.Empty;
				}

				OutlookFolderAndEvent outlookFolderAndEvent = new OutlookFolderAndEvent();

				// Create a new item of Outlookfolder
				OutlookFolder outlookFolder = new OutlookFolder
				{
					DisplayName = folder.DisplayName,
					TotalCount = folder.TotalCount,
					TotalUnread = folder.UnreadCount,
					UniqueId = folder.Id.ToString(),
					LastUpdated = DateTime.Now,
					ParentFolderName = parentFolderName
				};

				// create a new item of event
				OutlookEvent outlookFolderEvent = new OutlookEvent
				{
					EventDate = DateTime.Now,
					EventType = type.ToString(),
					CurrentTotal = folder.TotalCount,
					CurrentUnread = folder.UnreadCount,
					FolderAsset = folder.DisplayName,
					ItemAsset = string.Empty,
					PreviousTotal = null,
					PreviousUnread = null,
					User = "Unknown user"
				};

				outlookFolderAndEvent.OutlookFolder = outlookFolder;
				outlookFolderAndEvent.OutlookEvent = outlookFolderEvent;

				//EventLogger.WriteToLog(string.Format(outlookFolder.DisplayName + " folder has an event captured of type: " + outlookFolderAndEvent.OutlookEvent.EventType.ToString()),
				//		System.Diagnostics.EventLogEntryType.Information);

				FolderEventReceived?.Invoke(this, outlookFolderAndEvent);
			}
			catch (Exception ex)
			{
				// Sometimes it can't bind. i get it. Sometimes i don't wanna do stuff too.
				Console.WriteLine(ex.Message.ToString());
			}
		}

		/// <summary>
		/// Returns a full list of each item in Outlook of type email
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public bool SeedEmailItems()
		{
			// Time the method execution
			var watch = System.Diagnostics.Stopwatch.StartNew();
			Log.Information("Item seed began: {time}", DateTime.Now.ToLongTimeString());

			List<OutlookItem> outlookItems = new List<OutlookItem>();
			int offset = 0;
			int pageSize = 50;
			bool moreItems = true;
			bool itemSeedComplete = false;
			int sleepProgress = 0;
			int totalProgress = 0;
			int parts = 0;

			ItemView view = new ItemView(pageSize, offset, OffsetBasePoint.Beginning);
			FindItemsResults<Item> findResults;

			foreach (FolderId folderId in EwsSubscriptionHelper.GetFoldersForItemSeed(service))
			{

				while (moreItems)
				{

					// View Properties
					//view.PropertySet = PropertySet.IdOnly;
					view.PropertySet = BasePropertySet.FirstClassProperties;
					findResults = service.FindItems(folderId, view);

					foreach (var item in findResults.Items)
					{
						var email = (EmailMessage)item;
						//var email = EwsSubscriptionHelper.ReturnEmailMessage(item.Id, service);
						//if (email == null)
						//{
						//	email = EwsSubscriptionHelper.ReturnEmailMessage(item.Id, service);
						//}

						// We have to do this long hand to avoid 'key' conflicts
						OutlookItem outlookItem = new OutlookItem
						{
							Categories = (email.Categories != null) ? EwsSubscriptionHelper.StringifyCategories(email.Categories) : string.Empty,
							DateTimeReceived = email.DateTimeReceived,
							EmailAddress = string.Empty,
							EmailId = (!string.IsNullOrEmpty(email.Id.ToString())) ? email.Id.ToString() : string.Empty,
							Importance = email.Importance.ToString(),
							EmailSubject = (email.Subject != null) ? EwsSubscriptionHelper.AnonymiseSubject(email.Subject.ToString()) : string.Empty,
							OldParentFolderName = string.Empty,
							ParentFolderName = string.Empty
						};

						// Email Address
						if (!string.IsNullOrEmpty(email.From.Address))
						{
							outlookItem.EmailAddress = EwsSubscriptionHelper.AnonymiseEmail(email.From.Address);
						} else
						{
							// Try bind it and get it
							var newEmail = EwsSubscriptionHelper.ReturnEmailMessage(item, service);

							if (newEmail != null && email.From != null && email.From.Address != null)
								outlookItem.EmailAddress = (!string.IsNullOrEmpty(newEmail.From.Address.ToString())) ? newEmail.From.Address.ToString() : string.Empty;
													
						}

						//string parentFolderName = string.Empty;
						//if (!string.IsNullOrEmpty(email.ParentFolderId.ToString()) && Folder.Bind(service, email.ParentFolderId) != null)
						//{
						//	Folder parFolder = Folder.Bind(service, email.ParentFolderId);
						//	parentFolderName = (!string.IsNullOrEmpty(parFolder.DisplayName.ToString())) ? parFolder.DisplayName.ToString() : string.Empty;
						//}

						// Dont check if it exists, just add it.
						outlookItems.Add(outlookItem);

						sleepProgress++;
						totalProgress++;

						if (sleepProgress >= 500)
						{
							sleepProgress = 0;
							parts++;


							if (ProcessPartialItemSeed(outlookItems, parts) == true)
							{
								outlookItems.Clear();
							}

							// Take your slippers off, put your feet up and sleep for a little while
							Thread.Sleep(62000);
						}
					}

					// Sets val if more items available for traversal
					moreItems = findResults.MoreAvailable;

					// Increment page size
					if (moreItems)
						view.Offset += pageSize;

				}

				// Reset to true for next folder
				moreItems = true;

				//try
				//{

				//}
				//catch (Exception ex)
				//{
				//	Log.Error("Error occured during Item Seed part {part}:", parts.ToString(), ex.Message.ToString());

				//	// Just start again...
				//	System.Threading.Tasks.Task.Delay(120000).ContinueWith(t => SeedEmailItems());

				//	return false;
				//}
			}

			// If we have hit here then we have finally finished
			itemSeedComplete = true;

			// Log it
			watch.Stop();
			Log.Information("Item seed completed processing {count} items after {executionTime} seconds: {@OutlookItems} ", outlookItems.Count.ToString(), watch.Elapsed.TotalSeconds.ToString(), outlookItems);


			// Now the seed is complete we need to initiate the service disposal and reimplementation
			InstantiateNewEwsService();

			return itemSeedComplete;
		}

		public bool ProcessPartialItemSeed(List<OutlookItem> outlookItems, int part)
		{
			try
			{
				// We now need to pass this little lot to the api for processing
				// create a new item of event
				OutlookEvent outlookEvent = new OutlookEvent
				{
					EventDate = DateTime.Now,
					EventType = string.Format("Item seed part {0} completed", part.ToString()),
					CurrentTotal = null,
					CurrentUnread = null,
					FolderAsset = string.Empty,
					ItemAsset = string.Empty,
					PreviousTotal = null,
					PreviousUnread = null,
					User = "System raised event"
				};

				OutlookItemsAndEvent outlookItemsAndEvent = new OutlookItemsAndEvent
				{
					OutlookEvent = outlookEvent,
					OutlookItems = outlookItems
				};

				ItemSeedReceived?.Invoke(this, outlookItemsAndEvent);

				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to process partial seed: {exception}", ex.Message.ToString());
				return false;
			}
		}

	}

	// Todo Post seed event to capture missing data
}
