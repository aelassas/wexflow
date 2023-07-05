using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ApprovalWorkflowsCreator
{
    public class ApprovalWorkflowsCreator : Task
    {
        private static readonly string SmKey = "ApprovalRecordsCreator.RecordIds";

        public string AssignedTo { get; }
        public string Approver { get; }
        public bool DeleteWorkflowOnApproval { get; }

        public ApprovalWorkflowsCreator(XElement xe, Workflow wf) : base(xe, wf)
        {
            AssignedTo = GetSetting("assignedTo");
            Approver = GetSetting("approver");
            DeleteWorkflowOnApproval = bool.Parse(GetSetting("deleteWorkflowOnApproval", "true"));
        }

        public override TaskStatus Run()
        {
            Info("Creating and starting approval workflows for records...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                if (!SharedMemory.ContainsKey(SmKey))
                {
                    Error($"Shared memory key {SmKey} not found.");
                    success = false;
                }
                else
                {
                    var recordIds = (string[])SharedMemory[SmKey];

                    foreach (var recordId in recordIds)
                    {
                        try
                        {
                            var record = Workflow.Database.GetRecord(recordId);
                            var workflowId = Workflow.WexflowEngine.Workflows.Select(w => w.Id).Max() + 1;
                            var workflowName = $"Workflow_ApproveRecord_{SecurityElement.Escape(Approver)}_{SecurityElement.Escape(record.Name)}";

                            var xml = $"<Workflow xmlns='urn:wexflow-schema' id='{workflowId}' name='{workflowName}' description='{workflowName}'>\r\n"
                                    + "	 <Settings>\r\n"
                                    + "    <Setting name='launchType' value='trigger' />\r\n"
                                    + "    <Setting name='enabled' value='true' />\r\n"
                                    + "    <Setting name='approval' value='true' />\r\n"
                                    + "    <Setting name='enableParallelJobs' value='true' />\r\n"
                                    + "	  </Settings>\r\n"
                                    + "	 <LocalVariables />\r\n"
                                    + "	 <Tasks>\r\n"
                                   + $"    <Task id='1' name='ApproveRecord' description='Approving record {SecurityElement.Escape(record.Name)}' enabled='true'>\r\n"
                                   + $"      <Setting name='record' value='{SecurityElement.Escape(recordId)}' />\r\n"
                                   + $"      <Setting name='assignedTo' value='{SecurityElement.Escape(AssignedTo)}' />\r\n"
                                   + $"      <Setting name='deleteWorkflowOnApproval' value='{DeleteWorkflowOnApproval.ToString().ToLower()}' />"
                                    + "    </Task>\r\n"
                                    + "  </Tasks>\r\n"
                                    + "</Workflow>\r\n";

                            var approver = Workflow.WexflowEngine.GetUser(Approver);
                            var workflowDbId = Workflow.WexflowEngine.SaveWorkflow(approver.GetDbId(), approver.UserProfile, xml, false);

                            if (workflowDbId != "-1")
                            {
                                var workflow = Workflow.WexflowEngine.GetWorkflow(workflowId);

                                if (Workflow.WexflowEngine.EnableWorkflowsHotFolder)
                                {
                                    var filePath = Path.Combine(Workflow.WexflowEngine.WorkflowsFolder, $"Workflow_{workflowId}.xml");
                                    var xdoc = XDocument.Parse(xml);
                                    xdoc.Save(filePath);
                                    Thread.Sleep(5 * 1000); // Wait until the workflow get reloaded in the system
                                    workflow = Workflow.WexflowEngine.GetWorkflow(workflowId); // Reload the workflow
                                }

                                _ = workflow.StartAsync(Approver);
                                Info($"Approval Workflow of the record {recordId} - {record.Name} created and started successfully.");
                                if (!atLeastOneSuccess)
                                {
                                    atLeastOneSuccess = true;
                                }
                            }
                            else
                            {
                                Error($"An error occured while creating the approval workflow of the record {recordId} - {record.Name}.");
                                success = false;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while creating the approval workflow for the record {0}.", e, recordId);
                            success = false;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating approval workflows.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
