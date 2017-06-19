using System.ServiceProcess;
using InMo.Interfaces;
using InMo.Api;
using System.Threading;
using Serilog;
using System.Diagnostics;
using System;

namespace InMo
{
	public partial class Service1 : ServiceBase
	{

		public Service1()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			IEwsSubscriptionService exchangeServiceConnection = new EwsSubscriptionService();
			IApiOutlookEvent apiEvent = new ApiOutlookEvent();
			IApiOutlookFolder apiFolder = new ApiOutlookFolder();
			IApiOutlookItem apiItem = new ApiOutlookItem();

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.LiterateConsole()
				.WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\inmo-{Date}.log")
				.CreateLogger();

			Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

			EventController eventController = new EventController(exchangeServiceConnection, apiEvent, apiFolder, apiItem);

		}

		protected override void OnStop()
		{

		}
	}
}
