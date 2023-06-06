using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Approval
{
    public class Approval : Task
    {
        public Approval(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Approval process starting...");

            var status = Status.Success;

            try
            {
                if (Workflow.IsApproval)
                {
                    var trigger = Path.Combine(Workflow.ApprovalFolder, Workflow.Id.ToString(), Workflow.InstanceId.ToString(), Id.ToString(), "task.approved");

                    IsWaitingForApproval = true;
                    Workflow.IsWaitingForApproval = true;

                    while (!File.Exists(trigger) && !Workflow.IsRejected && !IsStopped)
                    {
                        Thread.Sleep(1000);
                    }

                    IsWaitingForApproval = false;
                    Workflow.IsWaitingForApproval = false;
                    if (!Workflow.IsRejected)
                    {
                        InfoFormat("Task approved: {0}", trigger);
                    }
                    else if (!IsStopped)
                    {
                        Info("This workflow has been rejected.");
                    }

                    if (File.Exists(trigger))
                    {
                        File.Delete(trigger);
                    }
                }
                else
                {
                    Error("This workflow is not an approval workflow. Mark this workflow as an approval workflow to use this task.");
                    status = Status.Error;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                Error("An error occured during approval process.", e);
                status = Status.Error;
            }

            Info("Approval process finished.");
            return new TaskStatus(status);
        }
    }
}
