using System;
using System.Configuration;
using Wexflow.Core.Service.Client;

namespace Wexflow.Scripts.RunAllWorkflows
{
    internal sealed class Program
    {
        private static void Main()
        {
            try
            {
                var client = new WexflowServiceClient(ConfigurationManager.AppSettings["WexflowWebServiceUri"]);
                var username = ConfigurationManager.AppSettings["Username"];
                var password = ConfigurationManager.AppSettings["Password"];

                var token = client.Login(username, password);
                var workflows = client.Search(string.Empty, token);

                foreach (var workflow in workflows)
                {
                    Console.WriteLine($"Starting workflow {workflow.Id} - {workflow.Name} => {client.StartWorkflow(workflow.Id, token)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            _ = Console.ReadKey();
        }
    }
}
