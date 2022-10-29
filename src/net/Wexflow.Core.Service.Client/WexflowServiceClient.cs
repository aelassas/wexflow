using Newtonsoft.Json;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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

        public WorkflowInfo[] Search(string keyword, string username, string password)
        {
            string uri = Uri + "/search?s=" + keyword;
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public Guid StartWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/start?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            var instanceId = webClient.UploadString(uri, string.Empty);
            return Guid.Parse(instanceId.Replace("\"", string.Empty));
        }

        public void StopWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/stop?w=" + id + "&i=" + instanceId;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/suspend?w=" + id + "&i=" + instanceId;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/resume?w=" + id + "&i=" + instanceId;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            webClient.UploadString(uri, string.Empty);
        }

        public void ApproveWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/approve?w=" + id + "&i=" + instanceId;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            webClient.UploadString(uri, string.Empty);
        }

        public void RejectWorkflow(int id, Guid instanceId, string username, string password)
        {
            string uri = Uri + "/reject?w=" + id + "&i=" + instanceId;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(string username, string password, int id)
        {
            string uri = Uri + "/workflow?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + GetMd5(password)));
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public User GetUser(string qusername, string qpassword, string username)
        {
            string uri = Uri + "/user?username=" + username;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(qusername + ":" + GetMd5(qpassword)));
            var response = webClient.DownloadString(uri);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

    }
}
