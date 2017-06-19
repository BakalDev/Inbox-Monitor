using System;
using System.Configuration;

namespace InMo.Config
{
	public class LogConfig : ConfigurationSection
	{
		public static LogConfig GetConfigurables()
		{
			LogConfig logConfig =
				ConfigurationManager
				.GetSection("logConfig")
				as LogConfig;



			if (logConfig != null)
				return logConfig;

			return new LogConfig();
		}

		[ConfigurationProperty("eventsource", IsRequired = true)]
		public string EventSource
		{
			get
			{
				return this["eventsource"] as string;
			}
		}

		/// <summary>
		/// This flag will determine whether event output is raised from EwsSubscriptionService to the event log. 
		/// Not recommended for production
		/// </summary>
		[ConfigurationProperty("outputEvents", IsRequired = true)]
		public bool OutputEvents
		{
			get
			{
				return Convert.ToBoolean(this["outputEvents"]);
			}
		}

		[ConfigurationProperty("location", IsRequired = true)]
		public string LogFileLocation
		{
			get
			{
				return this["location"] as string;
			}
		}

		[ConfigurationProperty("filename", IsRequired = false)]
		public string LogFilename
		{
			get
			{
				return this["filename"] as string;
			}
		}
	}
}
