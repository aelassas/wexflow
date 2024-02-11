using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpGet
{
    public class HttpGet : Task
    {
        public string Url { get; }
        public string AuthorizationScheme { get; }
        public string AuthorizationParameter { get; }

        public HttpGet(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            AuthorizationScheme = GetSetting("authorizationScheme");
            AuthorizationParameter = GetSetting("authorizationParameter");
        }

        public override TaskStatus Run()
        {
            Info("Executing GET request...");
            var status = Status.Success;
            try
            {
                var getTask = Post(Url, AuthorizationScheme, AuthorizationParameter);
                getTask.Wait();
                var result = getTask.Result;
                var destFile = Path.Combine(Workflow.WorkflowTempFolder,
                    $"HttpGet_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
                File.WriteAllText(destFile, result);
                Files.Add(new FileInf(destFile, Id));
                InfoFormat("GET request {0} executed with success -> {1}", Url, destFile);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the GET request {0}: {1}", Url, e.Message);
                status = Status.Error;
            }
            finally
            {
                WaitOne();
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }

        public static async System.Threading.Tasks.Task<string> Post(string url, string authScheme, string authParam)
        {
            using HttpClient httpClient = new();
            if (!string.IsNullOrEmpty(authScheme) && !string.IsNullOrEmpty(authParam))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParam);
            }

            var httpResponse = await httpClient.GetAsync(url);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}
