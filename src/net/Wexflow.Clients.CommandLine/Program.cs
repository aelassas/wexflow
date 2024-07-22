using CommandLine;
using CommandLine.Text;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.CommandLine
{
    internal class Program
    {
        private enum Operation
        {
            Start,
            Suspend,
            Resume,
            Stop,
            Approve,
            Reject
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Options
        {
            [Option('o', "operation", Required = true, HelpText = "start|suspend|resume|stop|approve|reject")]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Operation Operation { get; set; }

            [Option('i', "workflowId", Required = true, HelpText = "Workflow Id")]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int WorkflowId { get; set; }

            [Option('j', "jobId", Required = false, HelpText = "Job instance id (Guid)")]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string JobId { get; set; }

            [Option('w', "wait", Required = false, HelpText = "Wait until workflow finishes", Default = false)]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public bool Wait { get; set; }
        }

        private static void Main(string[] args)
        {
            try
            {
                var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);
                var res = parser.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       var client = new WexflowServiceClient(ConfigurationManager.AppSettings["WexflowWebServiceUri"]);
                       var username = ConfigurationManager.AppSettings["Username"];
                       var password = ConfigurationManager.AppSettings["Password"];

                       var workflows = client.Search(string.Empty, username, password);
                       if (workflows.All(w => w.Id != o.WorkflowId))
                       {
                           Console.WriteLine("Workflow id {0} is incorrect.", o.WorkflowId);
                           return;
                       }

                       WorkflowInfo workflow;
                       switch (o.Operation)
                       {
                           case Operation.Start:
                               var instanceId = client.StartWorkflow(o.WorkflowId, username, password);
                               Console.WriteLine("JobId: {0}", instanceId);

                               if (o.Wait)
                               {
                                   Thread.Sleep(1000);
                                   workflow = client.GetWorkflow(username, password, o.WorkflowId);
                                   var isRunning = workflow.IsRunning;
                                   while (isRunning)
                                   {
                                       Thread.Sleep(100);
                                       workflow = client.GetWorkflow(username, password, o.WorkflowId);
                                       isRunning = workflow.IsRunning;
                                   }
                               }
                               break;

                           case Operation.Suspend:
                               workflow = client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsRunning)
                               {
                                   Console.WriteLine("Workflow {0} is not running to be suspended.", o.WorkflowId);
                                   return;
                               }
                               client.SuspendWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Stop:
                               workflow = client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsRunning)
                               {
                                   Console.WriteLine("Workflow {0} is not running to be stopped.", o.WorkflowId);
                                   return;
                               }
                               client.StopWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Resume:
                               workflow = client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsPaused)
                               {
                                   Console.WriteLine("Workflow {0} is not suspended to be resumed.", o.WorkflowId);
                                   return;
                               }
                               client.ResumeWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Approve:
                               workflow = client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsWaitingForApproval)
                               {
                                   Console.WriteLine("Workflow {0} is not waiting for approval to be approved.", o.WorkflowId);
                                   return;
                               }
                               client.ApproveWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;

                           case Operation.Reject:
                               workflow = client.GetWorkflow(username, password, o.WorkflowId);
                               if (!workflow.IsWaitingForApproval)
                               {
                                   Console.WriteLine("Workflow {0} is not waiting for approval to be rejected.", o.WorkflowId);
                                   return;
                               }
                               client.RejectWorkflow(o.WorkflowId, Guid.Parse(o.JobId), username, password);
                               break;
                           default:
                               break;
                       }
                   });

                _ = res.WithNotParsed(errs =>
                {
                    var helpText = HelpText.AutoBuild(res, h => h, e => e);
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
