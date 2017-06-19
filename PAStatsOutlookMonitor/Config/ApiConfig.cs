using System.Configuration;

namespace InMo.Config
{
	public class ApiConfig : ConfigurationSection
	{
		public static ApiConfig GetConfigurables()
		{
			ApiConfig appSettingsConfig =
				ConfigurationManager
				.GetSection("apiConfig")
				as ApiConfig;


			if (appSettingsConfig != null)
				return appSettingsConfig;

			return new ApiConfig();
		}

		[ConfigurationProperty("url", IsRequired = true)]
		public string WebApiUrl
		{
			get
			{
				return this["url"] as string;
			}
		}
	}
}
