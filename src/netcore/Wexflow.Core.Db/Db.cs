using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Wexflow.Core.Db
{
    public enum EntryOrderBy
    {
        StatusDateAscending,
        StatusDateDescending,
        WorkflowIdAscending,
        WorkflowIdDescending,
        NameAscending,
        NameDescending,
        LaunchTypeAscending,
        LaunchTypeDescending,
        DescriptionAscending,
        DescriptionDescending,
        StatusAscending,
        StatusDescending
    }

    public enum UserOrderBy
    {
        UsernameAscending,
        UsernameDescending
    }

    public abstract class Db(string connectionString)
    {
        public string ConnectionString { get; } = connectionString;

        protected void InsertDefaultUser()
        {
            var password = GetMd5("wexflow2018");
            User user = new() { Username = "admin", Password = password, UserProfile = UserProfile.SuperAdministrator };
            InsertUser(user);
        }

        public abstract void Init();
        public abstract IEnumerable<Workflow> GetWorkflows();
        public abstract string InsertWorkflow(Workflow workflow);
        public abstract Workflow GetWorkflow(string id);
        public abstract void UpdateWorkflow(string dbId, Workflow workflow);
        public abstract void DeleteWorkflow(string id);
        public abstract void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId);
        public abstract void DeleteWorkflows(string[] ids);
        public abstract void InsertUserWorkflowRelation(UserWorkflow userWorkflow);
        public abstract void DeleteUserWorkflowRelationsByUserId(string userId);
        public abstract IEnumerable<string> GetUserWorkflows(string userId);
        public abstract bool CheckUserWorkflow(string userId, string workflowId);

        public abstract IEnumerable<User> GetAdministrators(string keyword, UserOrderBy uo);
        public abstract void InsertUser(User user);
        public abstract void UpdateUser(string id, User user);
        public abstract void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up);
        public abstract User GetUser(string username);
        public abstract User GetUserById(string id);
        public abstract void DeleteUser(string username, string password);
        public abstract string GetPassword(string username);
        public abstract IEnumerable<User> GetUsers();
        public abstract IEnumerable<User> GetUsers(string keyword, UserOrderBy uo);
        public abstract void UpdatePassword(string username, string password);
        public abstract IEnumerable<User> GetNonRestricedUsers();

        public abstract void ClearStatusCount();
        public abstract void ClearEntries();
        public abstract StatusCount GetStatusCount();
        public abstract IEnumerable<Entry> GetEntries();
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries();
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword);
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount);
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo);
        public abstract IEnumerable<Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo);
        public abstract long GetHistoryEntriesCount(string keyword);
        public abstract long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to);
        public abstract long GetEntriesCount(string keyword, DateTime from, DateTime to);
        public abstract DateTime GetHistoryEntryStatusDateMin();
        public abstract DateTime GetHistoryEntryStatusDateMax();
        public abstract DateTime GetEntryStatusDateMin();
        public abstract DateTime GetEntryStatusDateMax();
        public abstract Entry GetEntry(int workflowId);
        public abstract Entry GetEntry(int workflowId, Guid jobId);
        public abstract void InsertEntry(Entry entry);
        public abstract void UpdateEntry(string id, Entry entry);
        public abstract void IncrementDisabledCount();
        public abstract void IncrementRunningCount();
        public abstract void IncrementRejectedCount();
        public abstract void IncrementDoneCount();
        public abstract void IncrementWarningCount();
        public abstract void IncrementFailedCount();
        public abstract void IncrementStoppedCount();
        public abstract void IncrementPendingCount();
        public abstract void DecrementRunningCount();
        public abstract void DecrementPendingCount();
        public abstract void InsertHistoryEntry(HistoryEntry entry);
        public abstract string GetEntryLogs(string entryId);
        public abstract string GetHistoryEntryLogs(string entryId);

        public abstract string InsertRecord(Record record);
        public abstract void UpdateRecord(string recordId, Record record);
        public abstract void DeleteRecords(string[] recordIds);
        public abstract Record GetRecord(string id);
        public abstract IEnumerable<Record> GetRecords(string keyword);
        public abstract IEnumerable<Record> GetRecordsCreatedBy(string createdBy);
        public abstract IEnumerable<Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword);

        public abstract string InsertVersion(Version version);
        public abstract void UpdateVersion(string versionId, Version version);
        public abstract void DeleteVersions(string[] versionIds);
        public abstract IEnumerable<Version> GetVersions(string recordId);
        public abstract Version GetLatestVersion(string recordId);

        public abstract string InsertNotification(Notification notification);
        public abstract void MarkNotificationsAsRead(string[] notificationIds);
        public abstract void MarkNotificationsAsUnread(string[] notificationIds);
        public abstract void DeleteNotifications(string[] notificationIds);
        public abstract IEnumerable<Notification> GetNotifications(string assignedTo, string keyword);
        public abstract bool HasNotifications(string assignedTo);

        public abstract string InsertApprover(Approver approver);
        public abstract void UpdateApprover(string approverId, Approver approver);
        public abstract void DeleteApproversByRecordId(string recordId);
        public abstract void DeleteApprovedApprovers(string recordId);
        public abstract void DeleteApproversByUserId(string userId);
        public abstract IEnumerable<Approver> GetApprovers(string recordId);

        public abstract void Dispose();

        public static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = MD5.HashData(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < hashBytes.Length; i++)
            {
                _ = sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
