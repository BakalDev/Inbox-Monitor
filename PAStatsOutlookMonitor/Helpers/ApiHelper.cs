using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InMo
{
	public class ApiHelper
	{
		// Returns an instance of the HTTP Web Request with
		public static HttpWebRequest ReturnRequest(Uri api, object payload, string method = "POST")
		{
			var jsonPayload = JsonConvert.SerializeObject(payload);
			var httpRequest = (HttpWebRequest)WebRequest.Create(api);
			httpRequest.ContentType = "application/json";
			httpRequest.Method = method;
			httpRequest.Timeout = 1000 * 30;
			ASCIIEncoding encoder = new ASCIIEncoding();
			byte[] data = encoder.GetBytes(jsonPayload);
			httpRequest.ContentLength = data.Length;
			httpRequest.GetRequestStream().Write(data, 0, data.Length);

			return httpRequest;

		}
	}
}
