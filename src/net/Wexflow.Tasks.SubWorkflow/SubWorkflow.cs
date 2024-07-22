using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.SubWorkflow
{
    public enum KickOffAction
    {
        Start,
        Stop,
        Approve,
        Reject
    }

    public enum KickOffMode
    {
        Sync,
        Async
    }

    public class SubWorkflow : Task
    {
        public int WorkflowId { get; }
        public KickOffAction Action { get; }
        public KickOffMode Mode { get; }

        public SubWorkflow(XElement xe, Workflow wf) : base(xe, wf)
        {
            WorkflowId = int.Parse(GetSetting("id"));
            Action = (KickOffAction)Enum.Parse(typeof(KickOffAction), GetSetting("action", "start"), true);
            Mode = (KickOffMode)Enum.Parse(typeof(KickOffMode), GetSetting("mode", "sync"), true);
        }

        public override TaskStatus Run()
        {
            InfoFormat("Processing the sub workflow {0} ...", WorkflowId);

            var success = true;
            var warning = false;

            try
            {
                var workflow = Workflow.WexflowEngine.GetWorkflow(WorkflowId);
                if (workflow != null)
                {
                    switch (Action)
                    {
                        case KickOffAction.Start:
                            switch (Mode)
                            {
                                case KickOffMode.Sync:
                                    success = workflow.StartSync(Workflow.StartedBy, Guid.NewGuid(), ref warning);
                                    break;
                                case KickOffMode.Async:
                                    _ = workflow.StartAsync(Workflow.StartedBy);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case KickOffAction.Stop:
                            if (workflow.IsRunning)
                            {
                                success = workflow.Stop(Workflow.StartedBy);
                                if (success)
                                {
                                    InfoFormat("Workflow {0} stopped.", WorkflowId);
                                }
                                else
                                {
                                    ErrorFormat("An error occured while stopping the workflow {0}.", WorkflowId);
                                }
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("The workflow {0} is not running to be stopped.", WorkflowId);
                            }
                            break;
                        case KickOffAction.Approve:
                            if (workflow.IsApproval && workflow.IsWaitingForApproval)
                            {
                                workflow.Approve(Workflow.StartedBy);
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("The workflow {0} is not waiting for approval to be approved.", WorkflowId);
                            }
                            break;
                        case KickOffAction.Reject:
                            if (workflow.IsApproval && workflow.IsWaitingForApproval)
                            {
                                workflow.Reject(Workflow.StartedBy);
                            }
                            else
                            {
                                success = false;
                                ErrorFormat("The workflow {0} is not waiting for approval to be rejected.", WorkflowId);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    ErrorFormat("Workflow {0} not found.", WorkflowId);
                    success = false;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while processing the sub workflow {0}.", e, WorkflowId);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }
            else if (warning)
            {
                status = Status.Warning;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
