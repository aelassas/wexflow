using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Wexflow.Core;
using Wexflow.Core.Service.Client;

namespace Wexflow.Tasks.Workflow
{
    public enum WorkflowAction
    {
        Start,
        Suspend,
        Resume,
        Stop,
        Approve,
        Reject
    }

    public class Workflow : Task
    {
        public string WexflowWebServiceUri { get; }
        public string Username { get; }
        public string Password { get; }
        public WorkflowAction Action { get; }
        public int[] WorkflowIds { get; }
        public Dictionary<int, Guid> Jobs { get; }

        public Workflow(XElement xe, Core.Workflow wf) : base(xe, wf)
        {
            Jobs = new Dictionary<int, Guid>();
            WexflowWebServiceUri = GetSetting("wexflowWebServiceUri");
            Username = GetSetting("username");
            Password = GetSetting("password");
            Action = (WorkflowAction)Enum.Parse(typeof(WorkflowAction), GetSetting("action"), true);
            WorkflowIds = GetSettingsInt("id");
        }

        public override TaskStatus Run()
        {
            Info("Task started.");
            var success = true;
            var atLeastOneSucceed = false;
            foreach (var id in WorkflowIds)
            {
                try
                {
                    var client = new WexflowServiceClient(WexflowWebServiceUri);
                    var wfInfo = client.GetWorkflow(Username, Password, id);
                    switch (Action)
                    {
                        case WorkflowAction.Start:
                            if (wfInfo.IsRunning)
                            {
                                success = false;
                                ErrorFormat("Can't start the workflow {0} because it's already running.", id);
                            }
                            else
                            {
                                var instanceId = client.StartWorkflow(id, Username, Password);
                                Jobs[id] = instanceId;
                                InfoFormat("Workflow {0} started.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            break;
                        case WorkflowAction.Suspend:
                            if (wfInfo.IsRunning)
                            {
                                var jobId = Workflow.WexflowEngine.GetWorkflow(id).Jobs.Select(j => j.Key).FirstOrDefault();
                                client.SuspendWorkflow(id, jobId, Username, Password);
                                InfoFormat("Workflow {0} suspended.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("Can't suspend the workflow {0} because it's not running.", id);
                            }
                            break;
                        case WorkflowAction.Resume:
                            if (wfInfo.IsPaused)
                            {
                                var jobId = Workflow.WexflowEngine.GetWorkflow(id).Jobs.Select(j => j.Key).FirstOrDefault();
                                client.ResumeWorkflow(id, jobId, Username, Password);
                                InfoFormat("Workflow {0} resumed.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("Can't resume the workflow {0} because it's not suspended.", id);
                            }
                            break;
                        case WorkflowAction.Stop:
                            if (wfInfo.IsRunning)
                            {
                                var jobId = Workflow.WexflowEngine.GetWorkflow(id).Jobs.Select(j => j.Key).FirstOrDefault();
                                client.StopWorkflow(id, jobId, Username, Password);
                                InfoFormat("Workflow {0} stopped.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("Can't stop the workflow {0} because it's not running.", id);
                            }
                            break;
                        case WorkflowAction.Approve:
                            if (wfInfo.IsApproval && wfInfo.IsWaitingForApproval)
                            {
                                var jobId = Workflow.WexflowEngine.GetWorkflow(id).Jobs.Select(j => j.Key).FirstOrDefault();
                                client.ApproveWorkflow(id, jobId, Username, Password);
                                InfoFormat("Workflow {0} approved.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("Can't approve the workflow {0} because it's not waiting for approval.", id);
                            }
                            break;
                        case WorkflowAction.Reject:
                            if (wfInfo.IsApproval && wfInfo.IsWaitingForApproval)
                            {
                                var jobId = Workflow.WexflowEngine.GetWorkflow(id).Jobs.Select(j => j.Key).FirstOrDefault();
                                client.RejectWorkflow(id, jobId, Username, Password);
                                InfoFormat("Workflow {0} rejected.", id);
                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("Can't reject the workflow {0} because it's not waiting for approval.", id);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    success = false;
                    ErrorFormat("An error occured while processing the workflow {0}", e, id);
                }
            }
            Info("Task finished.");
            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            return new TaskStatus(status);
        }
    }
}
