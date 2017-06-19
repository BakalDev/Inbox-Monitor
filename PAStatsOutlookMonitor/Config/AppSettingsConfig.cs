using System;
using System.Configuration;

namespace InMo.Config
{
	public class AppSettingsConfig : ConfigurationSection
	{
		public static AppSettingsConfig GetConfigurables()
		{
			AppSettingsConfig appSettingsConfig =
				ConfigurationManager
				.GetSection("appSettingsConfig")
				as AppSettingsConfig;


			if (appSettingsConfig != null)
				return appSettingsConfig;

			return new AppSettingsConfig();
		}

		[ConfigurationProperty("generateFolderResults", IsRequired = true)]
		public bool GenerateFolderResults
		{
			get
			{
				return Convert.ToBoolean(this["generateFolderResults"]);
			}
		}
	}
}
