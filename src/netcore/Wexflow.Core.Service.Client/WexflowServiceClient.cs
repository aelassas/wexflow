using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        private static async Task<string> DownloadStringAsync(HttpClient client, string url, string username, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Basic {Base64Encode(username + ":" + GetMd5(password))}");
            var response = client.Send(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        private static async Task<string> UploadStringAsync(HttpClient client, string url, string username, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Basic {Base64Encode(username + ":" + GetMd5(password))}");
            var response = client.Send(request);
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            return responseString;
        }

        private static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public async Task<WorkflowInfo[]> Search(string keyword, string username, string password)
        {
            string uri = Uri + "/search?s=" + keyword;
            using var webClient = new HttpClient();

            var response = await DownloadStringAsync(webClient, uri, username, password);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public async Task<Guid> StartWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/start?w=" + id;
            using var webClient = new HttpClient();
            var instanceId = await UploadStringAsync(webClient, uri, username, password);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public async Task StopWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/stop?w=" + id + "&i=" + instanceId;
            using var webClient = new HttpClient();
            await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task SuspendWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/suspend?w=" + id + "&i=" + instanceId;
            using var webClient = new HttpClient();
            await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task ResumeWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/resume?w=" + id + "&i=" + instanceId;
            using var webClient = new HttpClient();
            await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task ApproveWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/approve?w=" + id + "&i=" + instanceId;
            using var webClient = new HttpClient();
            await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task RejectWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/reject?w=" + id + "&i=" + instanceId;
            using var webClient = new HttpClient();
            await UploadStringAsync(webClient, uri, username, password);
        }

        public async Task<WorkflowInfo> GetWorkflow(string username, string password, int id)
        {
            string uri = Uri + "/workflow?w=" + id;
            using var webClient = new HttpClient();
            var response = await DownloadStringAsync(webClient, uri, username, password);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public async Task<User> GetUser(string qusername, string qpassword, string username)
        {
            string uri = Uri + "/user?username=" + username;
            using var webClient = new HttpClient();
            var response = await DownloadStringAsync(webClient, uri, qusername, qpassword);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

    }
}
