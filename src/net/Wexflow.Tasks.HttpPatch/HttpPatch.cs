using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpPatch
{
    public class HttpPatch : Task
    {
        private const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        private const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;

        public string Url { get; private set; }
        public string Payload { get; private set; }
        public string AuthorizationScheme { get; private set; }
        public string AuthorizationParameter { get; private set; }
        public string Type { get; private set; }

        public HttpPatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            Payload = GetSetting("payload");
            AuthorizationScheme = GetSetting("authorizationScheme");
            AuthorizationParameter = GetSetting("authorizationParameter");
            Type = GetSetting("type", "application/json");
        }

        public override TaskStatus Run()
        {
            Info("Executing PATCH request...");
            var status = Status.Success;
            try
            {
                using (var client = new HttpClient())
                using (var httpContent = new StringContent(Payload, Encoding.UTF8, Type))
                {

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = Tls12;

                    if (!string.IsNullOrEmpty(AuthorizationScheme) && !string.IsNullOrEmpty(AuthorizationParameter))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, AuthorizationParameter);
                    }

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), Url)
                    {
                        Content = httpContent
                    };
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;

                    var destFile = Path.Combine(Workflow.WorkflowTempFolder, string.Format("HttpPatch_{0:yyyy-MM-dd-HH-mm-ss-fff}.txt", DateTime.Now));
                    File.WriteAllText(destFile, responseString);
                    Files.Add(new FileInf(destFile, Id));
                    InfoFormat("PATCH request {0} executed whith success -> {1}", Url, destFile);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the PATCH request {0}: {1}", Url, e.Message);
                status = Status.Error;
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
