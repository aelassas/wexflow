using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpPost
{
    public class HttpPost : Task
    {
        public string Url { get; }
        public string Payload { get; }
        public string AuthorizationScheme { get; }
        public string AuthorizationParameter { get; }
        public string Type { get; }

        public HttpPost(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            Payload = GetSetting("payload");
            AuthorizationScheme = GetSetting("authorizationScheme");
            AuthorizationParameter = GetSetting("authorizationParameter");
            Type = GetSetting("type", "application/json");
        }

        public override TaskStatus Run()
        {
            Info("Executing POST request...");
            var status = Status.Success;
            try
            {
                var postTask = Post(Url, AuthorizationScheme, AuthorizationParameter, Payload);
                postTask.Wait();
                var result = postTask.Result;
                var destFile = Path.Combine(Workflow.WorkflowTempFolder,
                    $"HttpPost_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
                File.WriteAllText(destFile, result);
                Files.Add(new FileInf(destFile, Id));
                InfoFormat("POST request {0} executed whith success -> {1}", Url, destFile);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the POST request {0}: {1}", Url, e.Message);
                status = Status.Error;
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }

        public async System.Threading.Tasks.Task<string> Post(string url, string authScheme, string authParam, string payload)
        {
            using (var httpContent = new StringContent(payload, Encoding.UTF8, Type))
            using (var httpClient = new HttpClient())
            {
                if (!string.IsNullOrEmpty(authScheme) && !string.IsNullOrEmpty(authParam))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParam);
                }

                var httpResponse = await httpClient.PostAsync(url, httpContent);
                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    return responseContent;
                }
            }
            return string.Empty;
        }
    }
}
