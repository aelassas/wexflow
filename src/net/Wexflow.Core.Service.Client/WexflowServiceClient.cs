using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        public string Login(string username, string password, bool stayConnected = false)
        {
            var uri = $"{Uri}/login";
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                webClient.Encoding = Encoding.UTF8;

                var requestBody = JsonConvert.SerializeObject(new { username, password, stayConnected });
                var response = webClient.UploadString(uri, "POST", requestBody);

                // Deserialize response JSON into a dynamic object
                dynamic res = JsonConvert.DeserializeObject(response);

                // Return the access_token property
                return res?.access_token;
            }
        }

        public WorkflowInfo[] Search(string keyword, string token)
        {
            var uri = $"{Uri}/search?s={keyword}";
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public Guid StartWorkflow(int id, string token)
        {
            var uri = $"{Uri}/start?w={id}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            var instanceId = webClient.UploadString(uri, string.Empty);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public Guid StartWorkflowWithVariables(string payload, string token)
        {
            var uri = $"{Uri}/start-with-variables";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            var instanceId = webClient.UploadString(uri, payload);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public void StopWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/stop?w={id}&i={instanceId}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            _ = webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/suspend?w={id}&i={instanceId}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            _ = webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/resume?w={id}&i={instanceId}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            _ = webClient.UploadString(uri, string.Empty);
        }

        public void ApproveWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/approve?w={id}&i={instanceId}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            _ = webClient.UploadString(uri, string.Empty);
        }

        public void RejectWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/reject?w={id}&i={instanceId}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            _ = webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(string token, int id)
        {
            var uri = $"{Uri}/workflow?w={id}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public User GetUser(string username, string token)
        {
            var uri = $"{Uri}/user?username={System.Uri.EscapeDataString(username)}";
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Bearer {token}");
            var response = webClient.DownloadString(uri);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }
    }
}
