using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient(string uri)
    {
        public string Uri { get; } = uri.TrimEnd('/');

        private static async Task<string> DownloadStringAsync(HttpClient client, string url, string token)
        {
            HttpRequestMessage request = new(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
            var response = await client.SendAsync(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        private static async Task<string> UploadStringAsync(HttpClient client, string url, string token, string body = "")
        {
            HttpRequestMessage request = new(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            var response = await client.SendAsync(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        public async Task<string> Login(string username, string password)
        {
            var uri = $"{Uri}/login";
            using HttpClient webClient = new();

            var requestBody = JsonConvert.SerializeObject(new { username, password });
            var response = await UploadStringAsync(webClient, uri, null, requestBody);

            // Deserialize response JSON into a dynamic object
            dynamic res = JsonConvert.DeserializeObject(response);

            // Return the access_token property
            return res?.access_token;
        }

        public async Task<WorkflowInfo[]> Search(string keyword, string token)
        {
            var uri = $"{Uri}/search?s={keyword}";
            using HttpClient webClient = new();

            var response = await DownloadStringAsync(webClient, uri, token);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public async Task<Guid> StartWorkflow(int id, string token)
        {
            var uri = $"{Uri}/start?w={id}";
            using HttpClient webClient = new();
            var instanceId = await UploadStringAsync(webClient, uri, token);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public async Task<Guid> StartWorkflowWithVariables(string payload, string token)
        {
            var uri = $"{Uri}/start-with-variables";
            using HttpClient webClient = new();
            var instanceId = await UploadStringAsync(webClient, uri, token, payload);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public async Task StopWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/stop?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, token);
        }

        public async Task SuspendWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/suspend?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, token);
        }

        public async Task ResumeWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/resume?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, token);
        }

        public async Task ApproveWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/approve?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, token);
        }

        public async Task RejectWorkflow(int id, Guid instanceId, string token)
        {
            var uri = $"{Uri}/reject?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, token);
        }

        public async Task<WorkflowInfo> GetWorkflow(string token, int id)
        {
            var uri = $"{Uri}/workflow?w={id}";
            using HttpClient webClient = new();
            var response = await DownloadStringAsync(webClient, uri, token);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public async Task<User> GetUser(string username, string token)
        {
            var uri = $"{Uri}/user?username={System.Uri.EscapeDataString(username)}";
            using HttpClient webClient = new();
            var response = await DownloadStringAsync(webClient, uri, token);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }
    }
}
