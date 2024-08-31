using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient(string uri)
    {
        public string Uri { get; } = uri.TrimEnd('/');

        private static async Task<string> DownloadStringAsync(HttpClient client, string url, string username, string password)
        {
            HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Basic {Base64Encode($"{username}:{GetMd5(password)}")}");
            var response = await client.SendAsync(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        private static async Task<string> UploadStringAsync(HttpClient client, string url, string username, string password, string body = "")
        {
            HttpRequestMessage request = new(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Basic {Base64Encode($"{username}:{GetMd5(password)}")}");
            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            var response = await client.SendAsync(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        private static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = MD5.HashData(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < hashBytes.Length; i++)
            {
                _ = sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public async Task<WorkflowInfo[]> Search(string keyword, string username, string password)
        {
            var uri = $"{Uri}/search?s={keyword}";
            using HttpClient webClient = new();

            var response = await DownloadStringAsync(webClient, uri, username, password);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public async Task<Guid> StartWorkflow(int id, string username, string password)
        {
            var uri = $"{Uri}/start?w={id}";
            using HttpClient webClient = new();
            var instanceId = await UploadStringAsync(webClient, uri, username, password);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public async Task<Guid> StartWorkflowWithVariables(string payload, string username, string password)
        {
            var uri = $"{Uri}/start-with-variables";
            using HttpClient webClient = new();
            var instanceId = await UploadStringAsync(webClient, uri, username, password, payload);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public async Task StopWorkflow(int id, Guid instanceId, string username, string password)
        {
            var uri = $"{Uri}/stop?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task SuspendWorkflow(int id, Guid instanceId, string username, string password)
        {
            var uri = $"{Uri}/suspend?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task ResumeWorkflow(int id, Guid instanceId, string username, string password)
        {
            var uri = $"{Uri}/resume?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task ApproveWorkflow(int id, Guid instanceId, string username, string password)
        {
            var uri = $"{Uri}/approve?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task RejectWorkflow(int id, Guid instanceId, string username, string password)
        {
            var uri = $"{Uri}/reject?w={id}&i={instanceId}";
            using HttpClient webClient = new();
            _ = await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task<WorkflowInfo> GetWorkflow(string username, string password, int id)
        {
            var uri = $"{Uri}/workflow?w={id}";
            using HttpClient webClient = new();
            var response = await DownloadStringAsync(webClient, uri, username, password);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public async Task<User> GetUser(string qusername, string qpassword, string username)
        {
            var uri = $"{Uri}/user?username={System.Uri.EscapeDataString(username)}";
            using HttpClient webClient = new();
            var response = await DownloadStringAsync(webClient, uri, qusername, qpassword);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }
    }
}
