using InMo.Models;
using InMo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMo.Interfaces
{
	public interface IApiOutlookFolder
	{
		void SeedOutlookFolders(OutlookFoldersAndEvent outlookFoldersAndEvent);

		void PostOutlookFolderAndEvent(OutlookFolderAndEvent outlookFolderAndEvent);
	}
}
