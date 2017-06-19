using Microsoft.Exchange.WebServices.Data;
using InMo.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using InMo.Models;

namespace InMo
{
	public class EwsSubscriptionHelper
	{
		/// <summary>
		/// Rather than call folders individually, just traverse all folders under specific WellKnownFolderName
		/// </summary>
		/// <returns>List of FolderIds to pass to the subscriptionservice</returns>
		public static List<FolderId> GetFolders(ExchangeService service)
		{
			List<FolderId> folderIds = new List<FolderId>();
			List<Folder> folders = new List<Folder>();

			// First set the mailbox to...the one that we want, oooh ooh ooh honey
			Mailbox mb = new Mailbox(MailboxConfig.GetConfigurables().OutlookMailbox);

			try
			{
				// Ok now we want to find subfolders of mb inbox
				var inboxFolderId = new FolderId(WellKnownFolderName.Inbox, mb);

				// Include inbox too
				folders.Add(Folder.Bind(service, inboxFolderId));
				folderIds.Add(inboxFolderId);

				// Folder filter - only return the DMP Emails folder and Actioned
				SearchFilter.SearchFilterCollection topLevelFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.Or);
				topLevelFilter.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, "Actioned"));
				topLevelFilter.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, "DMP Emails"));
				FindFoldersResults topFolderResults = service.FindFolders(inboxFolderId, topLevelFilter, new FolderView(2));

				// Add 'Actioned' folder to folders
				folders.Add(Folder.Bind(service, (FolderId)topFolderResults.FirstOrDefault(f => f.DisplayName == "Actioned").Id));
				folderIds.Add(((FolderId)topFolderResults.FirstOrDefault(f => f.DisplayName == "Actioned").Id));

				// Use DMP folder to query deeper
				Folder dmpEmailFolder = Folder.Bind(service, topFolderResults.FirstOrDefault(f => f.DisplayName == "DMP Emails").Id);

				// Now find all subfolders of dmpEmailFolder
				SearchFilter.SearchFilterCollection searchFilterCollection = new SearchFilter.SearchFilterCollection(LogicalOperator.Or);
				searchFilterCollection.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, "08:00"));
				searchFilterCollection.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, "09:00"));
				searchFilterCollection.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, "10:00"));
				searchFilterCollection.Add(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, ".Other Shift Pattern"));

				FindFoldersResults dmpSubFolders = service.FindFolders(dmpEmailFolder.Id, searchFilterCollection, new FolderView(1000));

				// Now we have the 3 shift pattern folders
				foreach (Folder folder in dmpSubFolders)
				{
					if (folder.DisplayName == ".Other Shift Pattern")
					{
						// Add this folder
						folders.Add(folder);
						folderIds.Add(folder.Id);

					}

					FindFoldersResults results = service.FindFolders(folder.Id, new FolderView(1000));

					foreach (Folder item in results)
					{
						folders.Add(item);
						folderIds.Add(item.Id);
					}
				}

				// Write help file of folder results
				EventLogger.WriteAllFolderNames(folders);

				return folderIds;
			}
			catch (Exception ex)
			{
				EventLogger.WriteToLog(string.Format("Failed to return a list of folders under inbox. {0}", ex.Message.ToString()),
					System.Diagnostics.EventLogEntryType.Error);
				throw;
			}

			/* Folder Structure for reference

				 *		DMP Emails
				 **		08:00-16:00
				 ***	User Folder Name

				 *		DMP Emails
				 **		09:00-17:00
				 ***	User Folder Name

				 *		DMP Emails
				 **		10:00-18:00
				 ***	User Folder Name
				 
				 *		DMP Emails
				 **		.Other Shift Pattern
				 
			*/
		}

		public static List<FolderId> GetFoldersForItemSeed(ExchangeService service)
		{
			List<FolderId> folders = GetFolders(service);

			foreach (FolderId item in folders)
			{
				try
				{
					Folder itemFolder = Folder.Bind(service, item);
					Folder parentFolder = Folder.Bind(service, itemFolder.ParentFolderId);

					// Remove Actioned folder where parent is Inbox
					if (itemFolder.DisplayName.ToLower().ToString() == "Actioned" && parentFolder.DisplayName.ToLower().ToString() == "Inbox")
						folders.Remove(item);
				}
				catch (Exception)
				{
					// Do nothing

				}
			}

			return folders;
		}

		/// <summary>
		/// Returns an array of the event types we want to listen out for
		/// </summary>
		/// <returns></returns>
		public static EventType[] SubscriptionEventTypes()
		{
			EventType[] subscriptionEvents = {
				EventType.Copied,
				EventType.Created,
				EventType.Deleted,
				EventType.Modified,
				EventType.Moved,
				EventType.NewMail
				//EventType.FreeBusyChanged,
				//EventType.Status
				};

			return subscriptionEvents;
		}

		/// <summary>
		/// Returns an object of EmailMessage from the ItemEvent
		/// </summary>
		/// <param name="item"></param>
		public static EmailMessage ReturnEmailMessage(ItemEvent itemEvent, ExchangeService service)
		{
			try
			{
				Item item = Item.Bind(service, itemEvent.ItemId);
				EmailMessage message = item as EmailMessage;
				return message;
			}
			catch (Exception)
			{
				//EventLogger.WriteToLog(string.Format("Failed to return the email object from ItemEvent. {0}", ex.Message.ToString()),
				//	System.Diagnostics.EventLogEntryType.Information);

				// TODO: Some instances of itemEvent cannot be bound to item. Removed the throw for now.
				// The specified object was not found in the store.
				return null;
				//throw;
			}
		}

		/// <summary>
		/// Returns an object of EmailMessage from the ItemEvent
		/// </summary>
		/// <param name="item"></param>
		public static EmailMessage ReturnEmailMessage(Item item, ExchangeService service)
		{
			try
			{				
				EmailMessage message = item as EmailMessage;
				return message;
			}
			catch (Exception)
			{
				//EventLogger.WriteToLog(string.Format("Failed to return the email object from ItemEvent. {0}", ex.Message.ToString()),
				//	System.Diagnostics.EventLogEntryType.Information);

				// TODO: Some instances of itemEvent cannot be bound to item. Removed the throw for now.
				// The specified object was not found in the store.
				return null;
				//throw;
			}
		}



		/// <summary>
		/// Anonymises the subject while the api is on the dev server
		/// </summary>
		/// <param name="emailSubject"></param>
		/// <returns></returns>
		public static string AnonymiseSubject(string emailSubject)
		{
			if (string.IsNullOrEmpty(emailSubject))

				return string.Empty;

			char[] array = emailSubject.ToCharArray();
			Random rng = new Random();
			int n = array.Length;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				var value = array[k];
				array[k] = array[n];
				array[n] = value;
			}

			return new string(array);
		}

		/// <summary>
		/// Anonymises the email address while the api is hosted on the dev server
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns></returns>
		public static string AnonymiseEmail(string emailAddress)
		{
			if (string.IsNullOrEmpty(emailAddress))
				return string.Empty;

			// get first part
			var pre = emailAddress.Substring(0, emailAddress.IndexOf("@"));
			var post = emailAddress.Split('@').Last();

			char[] preArray = pre.ToCharArray();
			char[] postArray = post.ToCharArray();
			Random rng = new Random();
			int preArrLength = preArray.Length;
			while (preArrLength > 1)
			{
				preArrLength--;
				int k = rng.Next(preArrLength + 1);
				var value = preArray[k];
				preArray[k] = preArray[preArrLength];
				preArray[preArrLength] = value;
			}

			int postArrLength = postArray.Length;
			while (postArrLength > 1)
			{
				postArrLength--;
				int k = rng.Next(postArrLength + 1);
				var value = postArray[k];
				postArray[k] = postArray[postArrLength];
				postArray[postArrLength] = value;
			}

			var newPre = new string(preArray);
			var newPost = new string(postArray);

			var anonymised = string.Format("{0}{1}{2}", newPre, "@", newPost);

			return anonymised;
		}

		/// <summary>
		/// Converts stringlist to comma serperated string
		/// </summary>
		/// <param name="categories"></param>
		/// <returns></returns>
		public static string StringifyCategories(StringList categories)
		{
			string emailCategoriesCsl = string.Empty;
			// Categories 
			if (categories != null || categories.Count > 0)
			{
				foreach (var cat in categories)
				{

					emailCategoriesCsl = string.Join(", ", cat);
				}
			}

			return emailCategoriesCsl;
		}
	}
}
