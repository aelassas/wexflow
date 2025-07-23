using System;
using System.Net;

namespace Wexflow.Core.Service.Client
{
    public class WebClientWithTimeout : WebClient
    {
        public int Timeout { get; set; } = 300_000; // Timeout in milliseconds. 5 minutes default.

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = Timeout;
            }
            return request;
        }
    }

}
