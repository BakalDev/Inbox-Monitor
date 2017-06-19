using System.Configuration;

namespace InMo.Config
{
	public class MailboxConfig : ConfigurationSection
	{
		public static MailboxConfig GetConfigurables()
		{
			MailboxConfig mailboxConfig =
				ConfigurationManager
				.GetSection("mailboxConfig")
				as MailboxConfig;



			if (mailboxConfig != null)
				return mailboxConfig;

			return new MailboxConfig();
		}

		[ConfigurationProperty("username", IsRequired = true)]
		public string OutlookMailboxUsername
		{
			get
			{
				return this["username"] as string;
			}
		}

		[ConfigurationProperty("password", IsRequired = true)]
		public string OutlookMailboxPassword
		{
			get
			{
				return this["password"] as string;
			}
		}

		[ConfigurationProperty("domain", IsRequired = true)]
		public string OutlookMailboxDomain
		{
			get
			{
				return this["domain"] as string;
			}
		}

		[ConfigurationProperty("mailbox", IsRequired = true)]
		public string OutlookMailbox
		{
			get
			{
				return this["mailbox"] as string;
			}
		}

	}
}
