﻿using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.CommandLine
{
    internal sealed class Program
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

        private sealed class Options
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

        private static async Task Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                Parser parser = new(cfg => cfg.CaseInsensitiveEnumValues = true);

                async Task<int> ExecuteCommand(Options o)
                {
                    WexflowServiceClient client = new(config["WexflowWebServiceUri"]);
                    var username = config["Username"];
                    var password = config["Password"];
                    var token = await client.Login(username, password);

                    var workflows = await client.Search(string.Empty, token);
                    if (workflows.All(w => w.Id != o.WorkflowId))
                    {
                        Console.WriteLine("Workflow id {0} is incorrect.", o.WorkflowId);
                        return await Task.FromResult(1);
                    }

                    WorkflowInfo workflow;
                    switch (o.Operation)
                    {
                        case Operation.Start:
                            var instanceId = await client.StartWorkflow(o.WorkflowId, token);
                            Console.WriteLine("JobId: {0}", instanceId);

                            if (o.Wait)
                            {
                                await Task.Delay(1000);
                                workflow = await client.GetJob(token, o.WorkflowId, instanceId);
                                var isRunning = workflow?.IsRunning ?? false;
                                while (isRunning)
                                {
                                    await Task.Delay(100);
                                    workflow = await client.GetJob(token, o.WorkflowId, instanceId);
                                    isRunning = workflow?.IsRunning ?? false;
                                }
                            }

                            break;

                        case Operation.Suspend:
                            workflow = await client.GetWorkflow(token, o.WorkflowId);
                            if (!workflow.IsRunning)
                            {
                                Console.WriteLine("Workflow {0} is not running to be suspended.", o.WorkflowId);
                                return await Task.FromResult(1);
                            }

                            await client.SuspendWorkflow(o.WorkflowId, Guid.Parse(o.JobId), token);
                            break;

                        case Operation.Stop:
                            workflow = await client.GetWorkflow(token, o.WorkflowId);
                            if (!workflow.IsRunning)
                            {
                                Console.WriteLine("Workflow {0} is not running to be stopped.", o.WorkflowId);
                                return await Task.FromResult(1);
                            }

                            await client.StopWorkflow(o.WorkflowId, Guid.Parse(o.JobId), token);
                            break;

                        case Operation.Resume:
                            workflow = await client.GetWorkflow(token, o.WorkflowId);
                            if (!workflow.IsPaused)
                            {
                                Console.WriteLine("Workflow {0} is not suspended to be resumed.", o.WorkflowId);
                                return await Task.FromResult(1);
                            }

                            await client.ResumeWorkflow(o.WorkflowId, Guid.Parse(o.JobId), token);
                            break;

                        case Operation.Approve:
                            workflow = await client.GetWorkflow(token, o.WorkflowId);
                            if (!workflow.IsWaitingForApproval)
                            {
                                Console.WriteLine("Workflow {0} is not waiting for approval to be approved.", o.WorkflowId);
                                return await Task.FromResult(1);
                            }

                            await client.ApproveWorkflow(o.WorkflowId, Guid.Parse(o.JobId), token);
                            break;

                        case Operation.Reject:
                            workflow = await client.GetWorkflow(token, o.WorkflowId);
                            if (!workflow.IsWaitingForApproval)
                            {
                                Console.WriteLine("Workflow {0} is not waiting for approval to be rejected.", o.WorkflowId);
                                return await Task.FromResult(1);
                            }

                            await client.RejectWorkflow(o.WorkflowId, Guid.Parse(o.JobId), token);
                            break;
                        default:
                            break;
                    }

                    return await Task.FromResult(0);
                }

                var res = parser.ParseArguments<Options>(args);
                _ = await res.MapResult(ExecuteCommand, _ => Task.FromResult(1));

                _ = res.WithNotParsed(_ =>
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
