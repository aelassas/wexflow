using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wexflow.Core.Service.Client;

namespace Wexflow.Server.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new WexflowServiceClient(ConfigurationManager.AppSettings["WexflowWebServiceUri"]);
            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];

            Action startWorkflow = () =>
            {
                Thread.CurrentThread.IsBackground = true;
                var jobId = client.StartWorkflow(41, username, password);
                Console.WriteLine(jobId);
            };

            new Thread(() =>
            {
                startWorkflow();
            }).Start();

            new Thread(() =>
            {
                startWorkflow();
            }).Start();

            Console.ReadKey();
        }
    }
}
