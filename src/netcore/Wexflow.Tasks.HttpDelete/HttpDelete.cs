using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpDelete
{
    public class HttpDelete : Task
    {
        public string Url { get; }
        public string AuthorizationScheme { get; }
        public string AuthorizationParameter { get; }

        public HttpDelete(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            AuthorizationScheme = GetSetting("authorizationScheme");
            AuthorizationParameter = GetSetting("authorizationParameter");
        }

        public override TaskStatus Run()
        {
            Info("Executing DELETE request...");
            var status = Status.Success;
            try
            {
                var deleteTask = Delete(Url, AuthorizationScheme, AuthorizationParameter);
                deleteTask.Wait();
                var result = deleteTask.Result;
                var destFile = Path.Combine(Workflow.WorkflowTempFolder,
                    $"HttpDelete_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
                File.WriteAllText(destFile, result);
                Files.Add(new FileInf(destFile, Id));
                InfoFormat("DELETE request {0} executed whith success -> {1}", Url, destFile);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the DELETE request {0}: {1}", Url, e.Message);
                status = Status.Error;
            }
            finally
            {
                WaitOne();
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }

        public static async System.Threading.Tasks.Task<string> Delete(string url, string authScheme, string authParam)
        {
            using HttpClient httpClient = new();
            if (!string.IsNullOrEmpty(authScheme) && !string.IsNullOrEmpty(authParam))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParam);
            }

            var httpResponse = await httpClient.DeleteAsync(url);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}
