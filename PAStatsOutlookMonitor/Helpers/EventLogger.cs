using Microsoft.Exchange.WebServices.Data;
using InMo.Config;
using InMo.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace InMo
{
	public class EventLogger
	{
		private static string EventSource = LogConfig.GetConfigurables().EventSource;

		/// <summary>
		/// Writes string to log
		/// </summary>
		/// <param name="error"></param>
		/// <param name="type"></param>
		public static void WriteToLog(string error, EventLogEntryType type)
		{

			if (!EventLog.SourceExists(EventSource))
				EventLog.CreateEventSource(EventSource, "Application");

			EventLog.WriteEntry(EventSource, error, type);
		}

		/// <summary>
		/// Writes an instance of stringbuilder to log
		/// </summary>
		/// <param name="error"></param>
		/// <param name="type"></param>
		public static void WriteToLog(StringBuilder error, EventLogEntryType type)
		{
			string source = EventSource;
			if (!EventLog.SourceExists(EventSource))
				EventLog.CreateEventSource(EventSource, "Application");

			EventLog.WriteEntry(source, error.ToString(), type);
		}


		/// <summary>
		/// Basic help file that identifies what folders are being returned to the Web API.
		/// </summary>
		/// <param name="folders"></param>
		public static void WriteAllFolderNames(List<Folder> folders)
		{
			if (AppSettingsConfig.GetConfigurables().GenerateFolderResults != true) { return; } // Todo Do we write to the event log that this has been set to false?

			TextWriter textWriter = new StreamWriter(string.Format("{0}{1}", LogConfig.GetConfigurables().LogFileLocation, LogConfig.GetConfigurables().LogFilename + ".txt"));

			folders = folders.OrderBy(f => f.DisplayName).ToList();

			foreach (Folder folder in folders)
			{
				textWriter.Write(folder.DisplayName + ". Total Count = " + folder.TotalCount + ". Total unread = " + folder.UnreadCount + "\r\n");
			}

			textWriter.Close();
		}
	}

}
