using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
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
        public NameValueCollection Params { get; private set; }

        public HttpPatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            var parameters = GetSetting("params");
            var parametersArray = parameters.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            Params = new NameValueCollection();
            foreach (var param in parametersArray)
            {
                var paramKv = param.Split('=');
                Params.Add(paramKv[0], paramKv[1]);
            }
        }

        public override TaskStatus Run()
        {
            Info("Executing PATCH request...");
            var status = Status.Success;
            try
            {
                using HttpClient client = new();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = Tls12;

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), Url);
                var response = client.Send(request);
                var responseString = response.Content.ReadAsStringAsync().Result;

                var destFile = Path.Combine(Workflow.WorkflowTempFolder, string.Format("HttpPatch_{0:yyyy-MM-dd-HH-mm-ss-fff}.txt", DateTime.Now));
                File.WriteAllText(destFile, responseString);
                Files.Add(new FileInf(destFile, Id));
                InfoFormat("PATCH request {0} executed whith success -> {1}", Url, destFile);
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
