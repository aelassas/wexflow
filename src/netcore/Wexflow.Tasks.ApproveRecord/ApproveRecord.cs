using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using Wexflow.Core.Db;
using Workflow = Wexflow.Core.Workflow;

namespace Wexflow.Tasks.ApproveRecord
{
    public class ApproveRecord : Task
    {
        public string RecordId { get; }
        public string AssignedTo { get; }
        public TimeSpan ReminderDelay { get; }
        public string OnApproved { get; }
        public string OnRejected { get; }
        public string OnDueDateReached { get; }
        public string OnReminderDateReached { get; }
        public string OnDeleted { get; }
        public string OnStopped { get; }
        public bool DeleteWorkflowOnApproval { get; }

        private static readonly char[] separator = [','];

        public ApproveRecord(XElement xe, Workflow wf) : base(xe, wf)
        {
            RecordId = GetSetting("record");
            AssignedTo = GetSetting("assignedTo");
            ReminderDelay = TimeSpan.Parse(GetSetting("reminderDelay", "3.00:00:00"));
            OnApproved = GetSetting("onApproved");
            OnRejected = GetSetting("onRejected");
            OnDueDateReached = GetSetting("onDueDateReached");
            OnReminderDateReached = GetSetting("onReminderDateReached");
            OnDeleted = GetSetting("onDeleted");
            OnStopped = GetSetting("onStopped");
            DeleteWorkflowOnApproval = bool.Parse(GetSetting("deleteWorkflowOnApproval", "false"));
        }

        public override TaskStatus Run()
        {
            Info($"Approval process starting on the reocrd {RecordId} ...");

            var status = Core.Status.Success;

            try
            {
                if (Workflow.IsApproval)
                {
                    var trigger = Path.Combine(Workflow.ApprovalFolder, Workflow.Id.ToString(), Workflow.InstanceId.ToString(), Id.ToString(), "task.approved");

                    if (string.IsNullOrEmpty(RecordId))
                    {
                        Error("The record id setting is empty.");
                        status = Core.Status.Error;
                    }
                    else if (string.IsNullOrEmpty(AssignedTo))
                    {
                        Error("The assignedTo id setting is empty.");
                        status = Core.Status.Error;
                    }
                    else
                    {
                        var record = Workflow.Database.GetRecord(RecordId);

                        if (record == null)
                        {
                            Error($"Record {RecordId} does not exist in the database.");
                            status = Core.Status.Error;
                        }
                        else
                        {
                            var recordName = record.Name;
                            var assignedTo = Workflow.Database.GetUser(AssignedTo);

                            if (assignedTo == null)
                            {
                                Error($"The user {AssignedTo} does not exist in the database.");
                                status = Core.Status.Error;
                            }
                            else
                            {
                                // notification onStart
                                var approverUser = Workflow.Database.GetUser(Workflow.StartedBy);
                                var notificationMessage = $"An approval process on the record {record.Name} has started. You must update that record by adding new file versions. You can also add comments on that record.";
                                Notification notification = new()
                                {
                                    Message = notificationMessage,
                                    AssignedBy = approverUser.GetDbId(),
                                    AssignedTo = assignedTo.GetDbId(),
                                    AssignedOn = DateTime.Now,
                                    IsRead = false
                                };
                                _ = Workflow.Database.InsertNotification(notification);

                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                {
                                    var subject = $"Wexflow notification from {approverUser.Username}";
                                    var body = notificationMessage;

                                    var host = Workflow.WexflowEngine.SmptHost;
                                    var port = Workflow.WexflowEngine.SmtpPort;
                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                }
                                Info($"ApproveRecord.OnStart: User {assignedTo.Username} notified for the start of approval process on the record {record.GetDbId()} - {record.Name}.");

                                // assign the record
                                record.ModifiedBy = approverUser.GetDbId();
                                record.AssignedTo = assignedTo.GetDbId();
                                record.AssignedOn = DateTime.Now;
                                record.Approved = false;
                                Workflow.Database.UpdateRecord(record.GetDbId(), record);
                                Info($"Record {record.GetDbId()} - {record.Name} assigned to {assignedTo.Username}.");

                                // Insert/update the approver
                                var approvedApproversDeleted = Workflow.WexflowEngine.DeleteApprovedApprovers(record.GetDbId());
                                if (approvedApproversDeleted)
                                {
                                    Info($"Approved approvers of the record {record.GetDbId()} - {record.Name} deleted.");
                                    var approvers = Workflow.WexflowEngine.GetApprovers(record.GetDbId());
                                    var approver = approvers.FirstOrDefault(a => a.UserId == approverUser.GetDbId());
                                    bool approverUpserted;

                                    if (approver == null)
                                    {
                                        // insert
                                        Approver a = new()
                                        {
                                            UserId = approverUser.GetDbId(),
                                            RecordId = record.GetDbId(),
                                            Approved = false
                                        };

                                        var id = Workflow.WexflowEngine.InsertApprover(a);
                                        approverUpserted = id != "-1";
                                    }
                                    else
                                    {
                                        // Update
                                        approver.Approved = false;
                                        approver.ApprovedOn = null;
                                        approverUpserted = Workflow.WexflowEngine.UpdateApprover(approver.GetDbId(), approver);
                                    }

                                    if (!approverUpserted)
                                    {
                                        Error($"An error occured while inserting the approver {approverUser.Username}.");
                                        status = Core.Status.Error;
                                    }
                                    else
                                    {
                                        Info($"Approver {approverUser.Username} inserted.");

                                        IsWaitingForApproval = true;
                                        Workflow.IsWaitingForApproval = true;

                                        var reminderNotificationDone = false;
                                        while (true)
                                        {
                                            // notification onRecordDeleted
                                            record = Workflow.Database.GetRecord(RecordId);
                                            if (record == null)
                                            {
                                                notificationMessage = $"The approval process on the record {recordName} was stopped because the record was deleted.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);

                                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                {
                                                    var subject = $"Wexflow notification from {approverUser.Username}";
                                                    var body = notificationMessage;

                                                    var host = Workflow.WexflowEngine.SmptHost;
                                                    var port = Workflow.WexflowEngine.SmtpPort;
                                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                }

                                                Info($"ApproveRecord.OnRecordDeleted: User {assignedTo.Username} notified for the removal of the record {RecordId}.");

                                                var tasks = GetTasks(OnDeleted);
                                                ClearFiles();
                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                break;
                                            }

                                            // notification onApproved
                                            if (File.Exists(trigger))
                                            {
                                                var currentApprover = Workflow.WexflowEngine.GetUser(Workflow.ApprovedBy);
                                                notificationMessage = $"The record {record.Name} was approved by the user {Workflow.ApprovedBy}.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = currentApprover.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);

                                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                {
                                                    var subject = $"Wexflow notification from {approverUser.Username}";
                                                    var body = notificationMessage;

                                                    var host = Workflow.WexflowEngine.SmptHost;
                                                    var port = Workflow.WexflowEngine.SmtpPort;
                                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                }

                                                Info($"ApproveRecord.OnApproved: User {assignedTo.Username} notified for the approval of the record {record.GetDbId()} - {record.Name}.");

                                                // update the record
                                                var recordApprovers = Workflow.WexflowEngine.GetApprovers(record.GetDbId());
                                                var currApprover = recordApprovers.First(a => a.UserId == approverUser.GetDbId());
                                                currApprover.UserId = currentApprover.GetDbId();
                                                currApprover.Approved = true;
                                                currApprover.ApprovedOn = DateTime.Now;
                                                _ = Workflow.WexflowEngine.UpdateApprover(currApprover.GetDbId(), currApprover);
                                                var otherApprovers = recordApprovers.Where(a => a.UserId != approverUser.GetDbId()).ToArray();
                                                var approved = true;
                                                foreach (var otherApprover in otherApprovers)
                                                {
                                                    approved &= otherApprover.Approved;
                                                }

                                                record.Approved = approved;
                                                Workflow.Database.UpdateRecord(record.GetDbId(), record);
                                                Info($"Record {record.GetDbId()} - {record.Name} updated.");

                                                // Delete workflow on approval
                                                if (approved && DeleteWorkflowOnApproval)
                                                {
                                                    Workflow.Database.DeleteWorkflow(Workflow.DbId);
                                                    Workflow.Database.DeleteUserWorkflowRelationsByWorkflowId(Workflow.DbId);

                                                    var removedWorkflow = Workflow.WexflowEngine.Workflows.SingleOrDefault(wf => wf.DbId == Workflow.DbId);
                                                    if (removedWorkflow != null)
                                                    {
                                                        InfoFormat("Workflow {0} is removed.", removedWorkflow.Name);
                                                        WexflowEngine.StopCronJobs(removedWorkflow);
                                                        lock (Workflow.WexflowEngine.Workflows)
                                                        {
                                                            _ = Workflow.WexflowEngine.Workflows.Remove(removedWorkflow);
                                                        }

                                                        if (Workflow.WexflowEngine.EnableWorkflowsHotFolder)
                                                        {
                                                            if (!string.IsNullOrEmpty(removedWorkflow.FilePath) && File.Exists(removedWorkflow.FilePath))
                                                            {
                                                                File.Delete(removedWorkflow.FilePath);
                                                                InfoFormat("Workflow file {0} removed.", removedWorkflow.FilePath);
                                                            }
                                                        }
                                                    }
                                                }

                                                // All must approve notification
                                                if (approved && otherApprovers.Length > 0)
                                                {
                                                    notificationMessage = $"The record {record.Name} was approved by all approvers.";
                                                    notification = new Notification
                                                    {
                                                        Message = notificationMessage,
                                                        AssignedBy = approverUser.GetDbId(),
                                                        AssignedTo = assignedTo.GetDbId(),
                                                        AssignedOn = DateTime.Now,
                                                        IsRead = false
                                                    };
                                                    _ = Workflow.Database.InsertNotification(notification);

                                                    notification = new Notification
                                                    {
                                                        Message = notificationMessage,
                                                        AssignedBy = approverUser.GetDbId(),
                                                        AssignedTo = approverUser.GetDbId(),
                                                        AssignedOn = DateTime.Now,
                                                        IsRead = false
                                                    };
                                                    _ = Workflow.Database.InsertNotification(notification);

                                                    if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                    {
                                                        var subject = $"Wexflow notification on the record {record.Name}";
                                                        var body = notificationMessage;

                                                        var host = Workflow.WexflowEngine.SmptHost;
                                                        var port = Workflow.WexflowEngine.SmtpPort;
                                                        var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                        var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                        var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                        var from = Workflow.WexflowEngine.SmtpFrom;

                                                        Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                        Send(host, port, enableSsl, smtpUser, smtpPassword, approverUser.Email, from, subject, body);
                                                    }

                                                    // Notify other approvers
                                                    foreach (var otherApprover in otherApprovers)
                                                    {
                                                        notification = new Notification
                                                        {
                                                            Message = notificationMessage,
                                                            AssignedBy = otherApprover.UserId,
                                                            AssignedTo = otherApprover.UserId,
                                                            AssignedOn = DateTime.Now,
                                                            IsRead = false
                                                        };
                                                        _ = Workflow.Database.InsertNotification(notification);

                                                        if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                        {
                                                            var subject = $"Wexflow notification on the record {record.Name}";
                                                            var body = notificationMessage;

                                                            var host = Workflow.WexflowEngine.SmptHost;
                                                            var port = Workflow.WexflowEngine.SmtpPort;
                                                            var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                            var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                            var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                            var from = Workflow.WexflowEngine.SmtpFrom;

                                                            var otherApproverUser = Workflow.WexflowEngine.GetUserById(otherApprover.UserId);
                                                            Send(host, port, enableSsl, smtpUser, smtpPassword, otherApproverUser.Email, from, subject, body);
                                                        }
                                                    }
                                                }

                                                var tasks = GetTasks(OnApproved);
                                                var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                                                if (latestVersion != null)
                                                {
                                                    ClearFiles();
                                                    Files.Add(new FileInf(latestVersion.FilePath, Id));
                                                }

                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                if (latestVersion != null)
                                                {
                                                    _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                                                }

                                                break;
                                            }

                                            // notification onRejected
                                            if (Workflow.IsRejected)
                                            {
                                                var rejectedUser = Workflow.WexflowEngine.GetUser(Workflow.RejectedBy);
                                                notificationMessage = $"The record {record.Name} was rejected by the user {Workflow.RejectedBy}.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = rejectedUser.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);

                                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                {
                                                    var subject = $"Wexflow notification from {approverUser.Username}";
                                                    var body = notificationMessage;

                                                    var host = Workflow.WexflowEngine.SmptHost;
                                                    var port = Workflow.WexflowEngine.SmtpPort;
                                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                }

                                                Info($"ApproveRecord.OnRejected: User {assignedTo.Username} notified for the rejection of the record {record.GetDbId()} - {record.Name}.");

                                                // update the record
                                                var recordApprovers = Workflow.WexflowEngine.GetApprovers(record.GetDbId());
                                                var currApprover = recordApprovers.First(a => a.UserId == approverUser.GetDbId());
                                                currApprover.UserId = rejectedUser.GetDbId();
                                                currApprover.Approved = false;
                                                currApprover.ApprovedOn = null;
                                                _ = Workflow.WexflowEngine.UpdateApprover(currApprover.GetDbId(), currApprover);

                                                record.Approved = false;
                                                Workflow.Database.UpdateRecord(record.GetDbId(), record);
                                                Info($"Record {record.GetDbId()} - {record.Name} updated.");

                                                var tasks = GetTasks(OnRejected);
                                                var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                                                if (latestVersion != null)
                                                {
                                                    ClearFiles();
                                                    Files.Add(new FileInf(latestVersion.FilePath, Id));
                                                }

                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                if (latestVersion != null)
                                                {
                                                    _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                                                }

                                                break;
                                            }

                                            // notification onReminderDateReached
                                            var reminderDelayMs = ReminderDelay.TotalMilliseconds;
                                            var reminderDateTime = DateTime.Now.AddMilliseconds(reminderDelayMs);
                                            if (!reminderNotificationDone && record.EndDate.HasValue && record.EndDate.Value < reminderDateTime)
                                            {
                                                notificationMessage = $"The record {record.Name} due date will be reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage + " The task has not been completed.",
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = approverUser.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);

                                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                {
                                                    var subject = $"Wexflow notification on the record {record.Name}";
                                                    var body = notificationMessage;

                                                    var host = Workflow.WexflowEngine.SmptHost;
                                                    var port = Workflow.WexflowEngine.SmtpPort;
                                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, approverUser.Email, from, subject, body + " The task has not been completed.");
                                                }

                                                Info($"ApproveRecord.OnReminderDateReached: User {assignedTo.Username} notified that due date of the record {record.GetDbId()} - {record.Name} will be reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.");
                                                Info($"ApproveRecord.OnReminderDateReached: User {approverUser.Username} notified that due date of the record {record.GetDbId()} - {record.Name} will be reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.");

                                                var tasks = GetTasks(OnReminderDateReached);
                                                var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                                                if (latestVersion != null)
                                                {
                                                    ClearFiles();
                                                    Files.Add(new FileInf(latestVersion.FilePath, Id));
                                                }

                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                if (latestVersion != null)
                                                {
                                                    _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                                                }
                                                reminderNotificationDone = true;
                                            }

                                            // notification onDueDateReached
                                            if (record.EndDate.HasValue && DateTime.Now > record.EndDate.Value)
                                            {
                                                notificationMessage = $"The record {record.Name} due date was reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = approverUser.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);

                                                if (Workflow.WexflowEngine.EnableEmailNotifications)
                                                {
                                                    var subject = $"Wexflow notification on the record {record.Name}";
                                                    var body = notificationMessage;

                                                    var host = Workflow.WexflowEngine.SmptHost;
                                                    var port = Workflow.WexflowEngine.SmtpPort;
                                                    var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                                                    var smtpUser = Workflow.WexflowEngine.SmtpUser;
                                                    var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                                                    var from = Workflow.WexflowEngine.SmtpFrom;

                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                                                    Send(host, port, enableSsl, smtpUser, smtpPassword, approverUser.Email, from, subject, body);
                                                }

                                                Info($"ApproveRecord.OnDueDateReached: User {assignedTo.Username} notified for due date of the record {record.GetDbId()} - {record.Name} reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.");
                                                Info($"ApproveRecord.OnDueDateReached: User {approverUser.Username} notified for due date of the record {record.GetDbId()} - {record.Name} reached at {record.EndDate.Value:yyyy-MM-dd HH:mm:ss.fff}.");

                                                var tasks = GetTasks(OnDueDateReached);
                                                var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                                                if (latestVersion != null)
                                                {
                                                    ClearFiles();
                                                    Files.Add(new FileInf(latestVersion.FilePath, Id));
                                                }

                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                if (latestVersion != null)
                                                {
                                                    _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                                                }

                                                break;
                                            }

                                            // notification onStopped
                                            if (IsStopped)
                                            {
                                                notificationMessage = $"The approval process on the record {record.Name} was stopped by the user {Workflow.StoppedBy}.";
                                                notification = new Notification
                                                {
                                                    Message = notificationMessage,
                                                    AssignedBy = approverUser.GetDbId(),
                                                    AssignedTo = assignedTo.GetDbId(),
                                                    AssignedOn = DateTime.Now,
                                                    IsRead = false
                                                };
                                                _ = Workflow.Database.InsertNotification(notification);
                                                Info($"ApproveRecord.OnStopped: User {assignedTo.Username} notified for the stop of the approval process of the record {record.GetDbId()} - {record.Name}.");

                                                var tasks = GetTasks(OnStopped);
                                                var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                                                if (latestVersion != null)
                                                {
                                                    ClearFiles();
                                                    Files.Add(new FileInf(latestVersion.FilePath, Id));
                                                }

                                                foreach (var task in tasks)
                                                {
                                                    _ = task.Run();
                                                }

                                                if (latestVersion != null)
                                                {
                                                    _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                                                }

                                                break;
                                            }

                                            Thread.Sleep(1000);
                                        }
                                    }
                                }
                                else
                                {
                                    Error($"An error occured while deleting approved approvers of the record {record.GetDbId()} - {record.Name}.");
                                    status = Core.Status.Error;
                                }
                                IsWaitingForApproval = false;
                                Workflow.IsWaitingForApproval = false;
                                if (!Workflow.IsRejected && !IsStopped)
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
                        }
                    }
                }
                else
                {
                    Error("This workflow is not an approval workflow. Mark this workflow as an approval workflow to use this task.");
                    status = Core.Status.Error;
                }
            }
            catch (ThreadInterruptedException)
            {
                var record = Workflow.Database.GetRecord(RecordId);
                if (record != null)
                {
                    var assignedBy = Workflow.Database.GetUser(Workflow.StartedBy);
                    var assignedTo = Workflow.Database.GetUser(AssignedTo);
                    if (assignedBy != null && assignedTo != null)
                    {
                        var notificationMessage = $"The approval process on the record {record.Name} was stopped by the user {Workflow.StoppedBy}.";
                        Notification notification = new()
                        {
                            Message = notificationMessage,
                            AssignedBy = assignedBy.GetDbId(),
                            AssignedTo = assignedTo.GetDbId(),
                            AssignedOn = DateTime.Now,
                            IsRead = false
                        };
                        _ = Workflow.Database.InsertNotification(notification);

                        if (Workflow.WexflowEngine.EnableEmailNotifications)
                        {
                            var subject = $"Wexflow notification from {assignedBy.Username}";
                            var body = notificationMessage;

                            var host = Workflow.WexflowEngine.SmptHost;
                            var port = Workflow.WexflowEngine.SmtpPort;
                            var enableSsl = Workflow.WexflowEngine.SmtpEnableSsl;
                            var smtpUser = Workflow.WexflowEngine.SmtpUser;
                            var smtpPassword = Workflow.WexflowEngine.SmtpPassword;
                            var from = Workflow.WexflowEngine.SmtpFrom;

                            Send(host, port, enableSsl, smtpUser, smtpPassword, assignedTo.Email, from, subject, body);
                        }

                        Info($"ApproveRecord.OnStopped: User {assignedTo.Username} notified for the stop of the approval process of the record {record.GetDbId()} - {record.Name}.");

                        var tasks = GetTasks(OnStopped);
                        var latestVersion = Workflow.Database.GetLatestVersion(RecordId);
                        if (latestVersion != null)
                        {
                            ClearFiles();
                            Files.Add(new FileInf(latestVersion.FilePath, Id));
                        }

                        foreach (var task in tasks)
                        {
                            _ = task.Run();
                        }

                        if (latestVersion != null)
                        {
                            _ = Files.RemoveAll(f => f.Path == latestVersion.FilePath);
                        }
                    }
                }

                throw;
            }
            catch (Exception e)
            {
                Error("An error occured during approval process.", e);
                status = Core.Status.Error;
            }
            finally
            {
                WaitOne();
            }

            Info("Approval process finished.");
            return new TaskStatus(status);
        }

        private void ClearFiles()
        {
            foreach (var task in Workflow.Tasks)
            {
                task.Files.Clear();
            }
        }

        private Task[] GetTasks(string evt)
        {
            List<Task> tasks = [];

            if (!string.IsNullOrEmpty(evt))
            {
                var ids = evt.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    var taskId = int.Parse(id.Trim());
                    var task = Workflow.Tasks.First(t => t.Id == taskId);
                    tasks.Add(task);
                }
            }

            return [.. tasks];
        }

        private static void Send(string host, int port, bool enableSsl, string user, string password, string to, string from, string subject, string body)
        {
            SmtpClient smtp = new()
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            using MailMessage msg = new();
            msg.From = new MailAddress(from);
            msg.To.Add(new MailAddress(to));
            msg.Subject = subject;
            msg.Body = body;

            smtp.Send(msg);
        }
    }
}
