using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.CommandLine
{
    class Program
    {
        enum Operation
        {
            Start,
            Suspend,
            Resume,
            Stop,
            Approve,
            Reject
        }

        class Options
        {
            [Option('o', "operation", Required = true, HelpText = "start|suspend|resume|stop|approve|reject")]
            public Operation Operation { get; set; }

            [Option('i', "workflowId", Required = true, HelpText = "Workflow Id")]
            public int WorkflowId { get; set; }

            [Option('j', "jobId", Required = false, HelpText = "Job instance id (Guid)")]
            public string JobId { get; set; }

            [Option('w', "wait", Required = false, HelpText = "Wait until workflow finishes", Default = false)]
            public bool Wait { get; set; }
        }

        static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                Parser parser = new(cfg => cfg.CaseInsensitiveEnumValues = true);
                ParserResult<Options> res = parser.ParseArguments<Options>(args)
                   .WithParsed(async o =>
                   {
                       WexflowServiceClient client = new(config["WexflowWebServiceUri"]);
                       string username = config["Username"];
                       string password = config["Password"];

                       WorkflowInfo[] workflows = await client.Search(string.Empty, username, password);
                       if (!workflows.Any(w => w.Id == o.WorkflowId))
                       {
                           Console.WriteLine("Workflow id {0} is incorrect.", o.WorkflowId);
                           return;
                       }

                       WorkflowInfo workflow;
                       switch (o.Operation)
                       {
                           case Operation.Start:
                               Guid instanceId = await client.StartWorkflow(o.WorkflowId, username, password);
                               Console.WriteLine("JobId: {0}", instanceId);

                               if (o.Wait)
                               {
                                   Thread.Sleep(1000);
                                   workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                                   bool isRunning = workflow.IsRunning;
                                   while (isRunning)
                                   {
                                       Thread.Sleep(100);
                                       workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                                       isRunning = workflow.IsRunning;
                                   }
                               }
                               break;

                           case Operation.Suspend:
                               workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsRunning)
                               {
                                   Console.WriteLine("Workflow {0} is not running to be suspended.", o.WorkflowId);
                                   return;
                               }
                               await client.SuspendWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Stop:
                               workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsRunning)
                               {
                                   Console.WriteLine("Workflow {0} is not running to be stopped.", o.WorkflowId);
                                   return;
                               }
                               await client.StopWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Resume:
                               workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsPaused)
                               {
                                   Console.WriteLine("Workflow {0} is not suspended to be resumed.", o.WorkflowId);
                                   return;
                               }
                               await client.ResumeWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Approve:
                               workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsWaitingForApproval)
                               {
                                   Console.WriteLine("Workflow {0} is not waiting for approval to be approved.", o.WorkflowId);
                                   return;
                               }
                               await client.ApproveWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Reject:
                               workflow = await client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsWaitingForApproval)
                               {
                                   Console.WriteLine("Workflow {0} is not waiting for approval to be rejected.", o.WorkflowId);
                                   return;
                               }
                               await client.RejectWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                       }

                   });

                res.WithNotParsed(errs =>
                {
                    HelpText helpText = HelpText.AutoBuild(res, h => h, e =>
                    {
                        return e;
                    });
                    Console.WriteLine(helpText);
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }
        }
    }
}
