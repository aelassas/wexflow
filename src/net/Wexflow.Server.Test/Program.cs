using System;
using System.Configuration;
using System.Threading;
using Wexflow.Core.Service.Client;

namespace Wexflow.Server.Test
{
    internal class Program
    {
        private static void Main()
        {
            var client = new WexflowServiceClient(ConfigurationManager.AppSettings["WexflowWebServiceUri"]);
            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];

            void startWorkflow()
            {
                Thread.CurrentThread.IsBackground = true;
                //var jobId = client.StartWorkflow(41, username, password);
                //Console.WriteLine(jobId);

                var payload = $@"
                {{
	                ""WorkflowId"":131,
	                ""Variables"":[
	                  {{
		                 ""Name"":""restVar1"",
		                 ""Value"":""C:\\WexflowTesting\\file1.txt""
	                  }},
	                  {{
		                 ""Name"":""restVar2"",
		                 ""Value"":""C:\\WexflowTesting\\file2.txt""
	                  }}
	                ]
                }}
                ";

                var jobId = client.StartWorkflowWithVariables(payload, username, password);
                Console.WriteLine(jobId);
            }

            new Thread(() =>
            {
                startWorkflow();
            }).Start();

            new Thread(() =>
            {
                startWorkflow();
            }).Start();

            _ = Console.ReadKey();
        }
    }
}
