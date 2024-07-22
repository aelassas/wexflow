using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core.Db.Oracle
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object Padlock = new();
        private const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
        private const int CHUNK_SIZE = 2000;

        private static string _connectionString;

        public Db(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
            var helper = new Helper(connectionString);
            helper.CreateTableIfNotExists(Core.Db.Entry.DOCUMENT_NAME, Entry.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.HistoryEntry.DOCUMENT_NAME, HistoryEntry.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.StatusCount.DOCUMENT_NAME, StatusCount.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.User.DOCUMENT_NAME, User.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.UserWorkflow.DOCUMENT_NAME, UserWorkflow.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.Workflow.DOCUMENT_NAME, Workflow.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.Version.DOCUMENT_NAME, Version.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.Record.DOCUMENT_NAME, Record.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.Notification.DOCUMENT_NAME, Notification.TABLE_STRUCT);
            helper.CreateTableIfNotExists(Core.Db.Approver.DOCUMENT_NAME, Approver.TABLE_STRUCT);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            var statusCount = new StatusCount
            {
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.StatusCount.DOCUMENT_NAME + "("
                    + StatusCount.COLUMN_NAME_PENDING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_RUNNING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_DONE_COUNT + ", "
                    + StatusCount.COLUMN_NAME_FAILED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_WARNING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_DISABLED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_STOPPED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_REJECTED_COUNT + ") VALUES("
                    + statusCount.PendingCount + ", "
                    + statusCount.RunningCount + ", "
                    + statusCount.DoneCount + ", "
                    + statusCount.FailedCount + ", "
                    + statusCount.WarningCount + ", "
                    + statusCount.DisabledCount + ", "
                    + statusCount.StoppedCount + ", "
                    + statusCount.RejectedCount + ")"
                    , conn);
                _ = command.ExecuteNonQuery();
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*) FROM " + Core.Db.User.DOCUMENT_NAME, conn);
                var usersCount = Convert.ToInt64((decimal)command.ExecuteScalar());

                if (usersCount == 0)
                {
                    InsertDefaultUser();
                }
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DOCUMENT_NAME
                    + " WHERE " + UserWorkflow.COLUMN_NAME_USER_ID + "=" + int.Parse(userId)
                    + " AND " + UserWorkflow.COLUMN_NAME_WORKFLOW_ID + "=" + int.Parse(workflowId)
                    , conn);
                var count = Convert.ToInt64((decimal)command.ExecuteScalar());

                return count > 0;
            }
        }

        public override void ClearEntries()
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Entry.DOCUMENT_NAME, conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void ClearStatusCount()
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.StatusCount.DOCUMENT_NAME, conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.User.DOCUMENT_NAME
                    + " WHERE " + User.COLUMN_NAME_USERNAME + " = '" + username + "'"
                    + " AND " + User.COLUMN_NAME_PASSWORD + " = '" + password + "'"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.UserWorkflow.DOCUMENT_NAME
                    + " WHERE " + UserWorkflow.COLUMN_NAME_USER_ID + " = " + int.Parse(userId), conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.UserWorkflow.DOCUMENT_NAME
                    + " WHERE " + UserWorkflow.COLUMN_NAME_WORKFLOW_ID + " = " + int.Parse(workflowDbId), conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Workflow.DOCUMENT_NAME
                    + " WHERE " + Workflow.COLUMN_NAME_ID + " = " + int.Parse(id), conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var builder = new StringBuilder("(");

                for (var i = 0; i < ids.Length; i++)
                {
                    var id = ids[i];
                    _ = builder.Append(id);
                    _ = i < ids.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Workflow.DOCUMENT_NAME
                    + " WHERE " + Workflow.COLUMN_NAME_ID + " IN " + builder, conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (Padlock)
            {
                var admins = new List<User>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_ID + ", "
                                                      + User.COLUMN_NAME_USERNAME + ", "
                                                      + User.COLUMN_NAME_PASSWORD + ", "
                                                      + User.COLUMN_NAME_EMAIL + ", "
                                                      + User.COLUMN_NAME_USER_PROFILE + ", "
                                                      + User.COLUMN_NAME_CREATED_ON + ", "
                                                      + User.COLUMN_NAME_MODIFIED_ON
                                                      + " FROM " + Core.Db.User.DOCUMENT_NAME
                                                      + " WHERE " + "(LOWER(" + User.COLUMN_NAME_USERNAME + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " AND " + User.COLUMN_NAME_USER_PROFILE + " = " + (int)UserProfile.Administrator + ")"
                                                      + " ORDER BY " + User.COLUMN_NAME_USERNAME + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var admin = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    admins.Add(admin);
                }

                return admins;
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            lock (Padlock)
            {
                var entries = new List<Entry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Entry.COLUMN_NAME_ID + ", "
                                                      + Entry.COLUMN_NAME_NAME + ", "
                                                      + Entry.COLUMN_NAME_DESCRIPTION + ", "
                                                      + Entry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                      + Entry.COLUMN_NAME_STATUS + ", "
                                                      + Entry.COLUMN_NAME_STATUS_DATE + ", "
                                                      + Entry.COLUMN_NAME_WORKFLOW_ID + ", "
                                                      + Entry.COLUMN_NAME_JOB_ID
                                                      + " FROM " + Core.Db.Entry.DOCUMENT_NAME, conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new Entry
                    {
                        Id = Convert.ToInt64((decimal)reader[Entry.COLUMN_NAME_ID]),
                        Name = reader[Entry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_NAME],
                        Description = reader[Entry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_WORKFLOW_ID]),
                        JobId = reader[Entry.COLUMN_NAME_JOB_ID] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_JOB_ID]
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            lock (Padlock)
            {
                var entries = new List<Entry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var sqlBuilder = new StringBuilder("SELECT "
                                                   + Entry.COLUMN_NAME_ID + ", "
                                                   + Entry.COLUMN_NAME_NAME + ", "
                                                   + Entry.COLUMN_NAME_DESCRIPTION + ", "
                                                   + Entry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                   + Entry.COLUMN_NAME_STATUS + ", "
                                                   + Entry.COLUMN_NAME_STATUS_DATE + ", "
                                                   + Entry.COLUMN_NAME_WORKFLOW_ID + ", "
                                                   + Entry.COLUMN_NAME_JOB_ID
                                                   + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                                                   + " WHERE " + "(LOWER(" + Entry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                   + " OR " + "LOWER(" + Entry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                                   + " AND (" + Entry.COLUMN_NAME_STATUS_DATE + " BETWEEN TO_TIMESTAMP('" + from.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))"
                                                   + " ORDER BY ");

                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_STATUS_DATE).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_STATUS_DATE).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_WORKFLOW_ID).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_WORKFLOW_ID).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_NAME).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_NAME).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_LAUNCH_TYPE).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_LAUNCH_TYPE).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_DESCRIPTION).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_DESCRIPTION).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_STATUS).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        _ = sqlBuilder.Append(Entry.COLUMN_NAME_STATUS).Append(" DESC");
                        break;

                    default:
                        break;
                }

                _ = sqlBuilder.Append(" OFFSET ").Append((page - 1) * entriesCount).Append(" ROWS FETCH NEXT ").Append(entriesCount).Append(" ROWS ONLY");

                using var command = new OracleCommand(sqlBuilder.ToString(), conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new Entry
                    {
                        Id = Convert.ToInt64((decimal)reader[Entry.COLUMN_NAME_ID]),
                        Name = reader[Entry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_NAME],
                        Description = reader[Entry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_WORKFLOW_ID]),
                        JobId = reader[Entry.COLUMN_NAME_JOB_ID] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_JOB_ID]
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                    + " WHERE " + "(LOWER(" + Entry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + Entry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                    + " AND (" + Entry.COLUMN_NAME_STATUS_DATE + " BETWEEN TO_TIMESTAMP('" + from.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))", conn);
                var count = (decimal)command.ExecuteScalar();

                return Convert.ToInt64(count);
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                    + Entry.COLUMN_NAME_ID + ", "
                    + Entry.COLUMN_NAME_NAME + ", "
                    + Entry.COLUMN_NAME_DESCRIPTION + ", "
                    + Entry.COLUMN_NAME_LAUNCH_TYPE + ", "
                    + Entry.COLUMN_NAME_STATUS + ", "
                    + Entry.COLUMN_NAME_STATUS_DATE + ", "
                    + Entry.COLUMN_NAME_WORKFLOW_ID + ", "
                    + Entry.COLUMN_NAME_JOB_ID
                    + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                    + " WHERE " + Entry.COLUMN_NAME_WORKFLOW_ID + " = " + workflowId, conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var entry = new Entry
                    {
                        Id = Convert.ToInt64((decimal)reader[Entry.COLUMN_NAME_ID]),
                        Name = reader[Entry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_NAME],
                        Description = reader[Entry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_WORKFLOW_ID]),
                        JobId = reader[Entry.COLUMN_NAME_JOB_ID] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_JOB_ID]
                    };

                    return entry;
                }

                return null;
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId, Guid jobId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                    + Entry.COLUMN_NAME_ID + ", "
                    + Entry.COLUMN_NAME_NAME + ", "
                    + Entry.COLUMN_NAME_DESCRIPTION + ", "
                    + Entry.COLUMN_NAME_LAUNCH_TYPE + ", "
                    + Entry.COLUMN_NAME_STATUS + ", "
                    + Entry.COLUMN_NAME_STATUS_DATE + ", "
                    + Entry.COLUMN_NAME_WORKFLOW_ID + ", "
                    + Entry.COLUMN_NAME_JOB_ID
                    + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                    + " WHERE (" + Entry.COLUMN_NAME_WORKFLOW_ID + " = " + workflowId
                    + " AND " + Entry.COLUMN_NAME_JOB_ID + " = '" + jobId + "')", conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var entry = new Entry
                    {
                        Id = Convert.ToInt64((decimal)reader[Entry.COLUMN_NAME_ID]),
                        Name = reader[Entry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_NAME],
                        Description = reader[Entry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[Entry.COLUMN_NAME_WORKFLOW_ID]),
                        JobId = reader[Entry.COLUMN_NAME_JOB_ID] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_JOB_ID]
                    };

                    return entry;
                }

                return null;
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            lock (Padlock)
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using var command = new OracleCommand("SELECT " + Entry.COLUMN_NAME_STATUS_DATE
                        + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                        + " WHERE rownum = 1"
                        + " ORDER BY " + Entry.COLUMN_NAME_STATUS_DATE + " DESC", conn);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE];

                        return statusDate;
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            lock (Padlock)
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using var command = new OracleCommand("SELECT " + Entry.COLUMN_NAME_STATUS_DATE
                        + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                        + " WHERE rownum = 1"
                        + " ORDER BY " + Entry.COLUMN_NAME_STATUS_DATE + " ASC", conn);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = (DateTime)reader[Entry.COLUMN_NAME_STATUS_DATE];

                        return statusDate;
                    }
                }

                return DateTime.Now;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + HistoryEntry.COLUMN_NAME_ID + ", "
                                                      + HistoryEntry.COLUMN_NAME_NAME + ", "
                                                      + HistoryEntry.COLUMN_NAME_DESCRIPTION + ", "
                                                      + HistoryEntry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS_DATE + ", "
                                                      + HistoryEntry.COLUMN_NAME_WORKFLOW_ID
                                                      + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME, conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new HistoryEntry
                    {
                        Id = Convert.ToInt64((decimal)reader[HistoryEntry.COLUMN_NAME_ID]),
                        Name = reader[HistoryEntry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_NAME],
                        Description = reader[HistoryEntry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_WORKFLOW_ID]),
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + HistoryEntry.COLUMN_NAME_ID + ", "
                                                      + HistoryEntry.COLUMN_NAME_NAME + ", "
                                                      + HistoryEntry.COLUMN_NAME_DESCRIPTION + ", "
                                                      + HistoryEntry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS_DATE + ", "
                                                      + HistoryEntry.COLUMN_NAME_WORKFLOW_ID
                                                      + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                                                      + " WHERE " + "LOWER(" + HistoryEntry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " OR " + "LOWER(" + HistoryEntry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new HistoryEntry
                    {
                        Id = Convert.ToInt64((decimal)reader[HistoryEntry.COLUMN_NAME_ID]),
                        Name = reader[HistoryEntry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_NAME],
                        Description = reader[HistoryEntry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_WORKFLOW_ID]),
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + HistoryEntry.COLUMN_NAME_ID + ", "
                                                      + HistoryEntry.COLUMN_NAME_NAME + ", "
                                                      + HistoryEntry.COLUMN_NAME_DESCRIPTION + ", "
                                                      + HistoryEntry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS + ", "
                                                      + HistoryEntry.COLUMN_NAME_STATUS_DATE + ", "
                                                      + HistoryEntry.COLUMN_NAME_WORKFLOW_ID
                                                      + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                                                      + " WHERE " + "LOWER(" + HistoryEntry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " OR " + "LOWER(" + HistoryEntry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " OFFSET " + ((page - 1) * entriesCount) + " ROWS FETCH NEXT " + entriesCount + " ROWS ONLY"
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new HistoryEntry
                    {
                        Id = Convert.ToInt64((decimal)reader[HistoryEntry.COLUMN_NAME_ID]),
                        Name = reader[HistoryEntry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_NAME],
                        Description = reader[HistoryEntry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_WORKFLOW_ID]),
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var sqlBuilder = new StringBuilder("SELECT "
                                                   + HistoryEntry.COLUMN_NAME_ID + ", "
                                                   + HistoryEntry.COLUMN_NAME_NAME + ", "
                                                   + HistoryEntry.COLUMN_NAME_DESCRIPTION + ", "
                                                   + HistoryEntry.COLUMN_NAME_LAUNCH_TYPE + ", "
                                                   + HistoryEntry.COLUMN_NAME_STATUS + ", "
                                                   + HistoryEntry.COLUMN_NAME_STATUS_DATE + ", "
                                                   + HistoryEntry.COLUMN_NAME_WORKFLOW_ID
                                                   + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                                                   + " WHERE " + "(LOWER(" + HistoryEntry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                   + " OR " + "LOWER(" + HistoryEntry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                                   + " AND (" + HistoryEntry.COLUMN_NAME_STATUS_DATE + " BETWEEN TO_TIMESTAMP('" + from.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))"
                                                   + " ORDER BY ");

                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_STATUS_DATE).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_STATUS_DATE).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_WORKFLOW_ID).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_WORKFLOW_ID).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_NAME).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_NAME).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_LAUNCH_TYPE).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_LAUNCH_TYPE).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_DESCRIPTION).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_DESCRIPTION).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_STATUS).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        _ = sqlBuilder.Append(HistoryEntry.COLUMN_NAME_STATUS).Append(" DESC");
                        break;

                    default:
                        break;
                }

                _ = sqlBuilder.Append(" OFFSET ").Append((page - 1) * entriesCount).Append(" ROWS FETCH NEXT ").Append(entriesCount).Append(" ROWS ONLY");

                using var command = new OracleCommand(sqlBuilder.ToString(), conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var entry = new HistoryEntry
                    {
                        Id = Convert.ToInt64((decimal)reader[HistoryEntry.COLUMN_NAME_ID]),
                        Name = reader[HistoryEntry.COLUMN_NAME_NAME] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_NAME],
                        Description = reader[HistoryEntry.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_DESCRIPTION],
                        LaunchType = (LaunchType)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_LAUNCH_TYPE]),
                        Status = (Status)Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_STATUS]),
                        StatusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE],
                        WorkflowId = Convert.ToInt32((decimal)reader[HistoryEntry.COLUMN_NAME_WORKFLOW_ID]),
                    };

                    entries.Add(entry);
                }

                return entries;
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                    + " WHERE " + "LOWER(" + HistoryEntry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'", conn);
                var count = (decimal)command.ExecuteScalar();

                return Convert.ToInt64(count);
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                    + " WHERE " + "(LOWER(" + HistoryEntry.COLUMN_NAME_NAME + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.COLUMN_NAME_DESCRIPTION + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                    + " AND (" + HistoryEntry.COLUMN_NAME_STATUS_DATE + " BETWEEN TO_TIMESTAMP('" + from.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))", conn);
                var count = (decimal)command.ExecuteScalar();

                return Convert.ToInt64(count);
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (Padlock)
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using var command = new OracleCommand("SELECT " + HistoryEntry.COLUMN_NAME_STATUS_DATE
                        + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                        + " WHERE rownum = 1"
                        + " ORDER BY " + HistoryEntry.COLUMN_NAME_STATUS_DATE + " DESC", conn);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE];

                        return statusDate;
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            lock (Padlock)
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using var command = new OracleCommand("SELECT " + HistoryEntry.COLUMN_NAME_STATUS_DATE
                        + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                        + " WHERE rownum = 1"
                        + " ORDER BY " + HistoryEntry.COLUMN_NAME_STATUS_DATE + " ASC", conn);
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = (DateTime)reader[HistoryEntry.COLUMN_NAME_STATUS_DATE];

                        return statusDate;
                    }
                }

                return DateTime.Now;
            }
        }

        public override string GetPassword(string username)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_PASSWORD
                    + " FROM " + Core.Db.User.DOCUMENT_NAME
                    + " WHERE " + User.COLUMN_NAME_USERNAME + " = '" + (username ?? "").Replace("'", "''") + "'"
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var password = (string)reader[User.COLUMN_NAME_PASSWORD];

                    return password;
                }

                return null;
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + StatusCount.COLUMN_NAME_ID + ", "
                    + StatusCount.COLUMN_NAME_PENDING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_RUNNING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_DONE_COUNT + ", "
                    + StatusCount.COLUMN_NAME_FAILED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_WARNING_COUNT + ", "
                    + StatusCount.COLUMN_NAME_DISABLED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_STOPPED_COUNT + ", "
                    + StatusCount.COLUMN_NAME_REJECTED_COUNT
                    + " FROM " + Core.Db.StatusCount.DOCUMENT_NAME
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var statusCount = new StatusCount
                    {
                        Id = Convert.ToInt64((decimal)reader[StatusCount.COLUMN_NAME_ID]),
                        PendingCount = Convert.ToInt32((decimal)reader[StatusCount.COLUMN_NAME_PENDING_COUNT]),
                        RunningCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_RUNNING_COUNT]),
                        DoneCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_DONE_COUNT]),
                        FailedCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_FAILED_COUNT]),
                        WarningCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_WARNING_COUNT]),
                        DisabledCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_DISABLED_COUNT]),
                        StoppedCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_STOPPED_COUNT]),
                        RejectedCount = Convert.ToInt32(reader[StatusCount.COLUMN_NAME_REJECTED_COUNT])
                    };

                    return statusCount;
                }

                return null;
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_ID + ", "
                    + User.COLUMN_NAME_USERNAME + ", "
                    + User.COLUMN_NAME_PASSWORD + ", "
                    + User.COLUMN_NAME_EMAIL + ", "
                    + User.COLUMN_NAME_USER_PROFILE + ", "
                    + User.COLUMN_NAME_CREATED_ON + ", "
                    + User.COLUMN_NAME_MODIFIED_ON
                    + " FROM " + Core.Db.User.DOCUMENT_NAME
                    + " WHERE " + User.COLUMN_NAME_USERNAME + " = '" + (username ?? "").Replace("'", "''") + "'"
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    return user;
                }

                return null;
            }
        }

        public override Core.Db.User GetUserById(string id)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_ID + ", "
                    + User.COLUMN_NAME_USERNAME + ", "
                    + User.COLUMN_NAME_PASSWORD + ", "
                    + User.COLUMN_NAME_EMAIL + ", "
                    + User.COLUMN_NAME_USER_PROFILE + ", "
                    + User.COLUMN_NAME_CREATED_ON + ", "
                    + User.COLUMN_NAME_MODIFIED_ON
                    + " FROM " + Core.Db.User.DOCUMENT_NAME
                    + " WHERE " + User.COLUMN_NAME_ID + " = '" + int.Parse(id) + "'"
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    return user;
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_ID + ", "
                                                      + User.COLUMN_NAME_USERNAME + ", "
                                                      + User.COLUMN_NAME_PASSWORD + ", "
                                                      + User.COLUMN_NAME_EMAIL + ", "
                                                      + User.COLUMN_NAME_USER_PROFILE + ", "
                                                      + User.COLUMN_NAME_CREATED_ON + ", "
                                                      + User.COLUMN_NAME_MODIFIED_ON
                                                      + " FROM " + Core.Db.User.DOCUMENT_NAME
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    users.Add(user);
                }

                return users;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + User.COLUMN_NAME_ID + ", "
                                                      + User.COLUMN_NAME_USERNAME + ", "
                                                      + User.COLUMN_NAME_PASSWORD + ", "
                                                      + User.COLUMN_NAME_EMAIL + ", "
                                                      + User.COLUMN_NAME_USER_PROFILE + ", "
                                                      + User.COLUMN_NAME_CREATED_ON + ", "
                                                      + User.COLUMN_NAME_MODIFIED_ON
                                                      + " FROM " + Core.Db.User.DOCUMENT_NAME
                                                      + " WHERE " + "LOWER(" + User.COLUMN_NAME_USERNAME + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " ORDER BY " + User.COLUMN_NAME_USERNAME + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    users.Add(user);
                }

                return users;
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            lock (Padlock)
            {
                var workflowIds = new List<string>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + UserWorkflow.COLUMN_NAME_ID + ", "
                                                      + UserWorkflow.COLUMN_NAME_USER_ID + ", "
                                                      + UserWorkflow.COLUMN_NAME_WORKFLOW_ID
                                                      + " FROM " + Core.Db.UserWorkflow.DOCUMENT_NAME
                                                      + " WHERE " + UserWorkflow.COLUMN_NAME_USER_ID + " = " + int.Parse(userId)
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var workflowId = Convert.ToInt64((decimal)reader[UserWorkflow.COLUMN_NAME_WORKFLOW_ID]);

                    workflowIds.Add(workflowId.ToString());
                }

                return workflowIds;
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + Workflow.COLUMN_NAME_ID + ", "
                    + Workflow.COLUMN_NAME_XML
                    + " FROM " + Core.Db.Workflow.DOCUMENT_NAME
                    + " WHERE " + Workflow.COLUMN_NAME_ID + " = " + int.Parse(id), conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var workflow = new Workflow
                    {
                        Id = Convert.ToInt64((decimal)reader[Workflow.COLUMN_NAME_ID]),
                        Xml = (string)reader[Workflow.COLUMN_NAME_XML]
                    };

                    return workflow;
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            lock (Padlock)
            {
                var workflows = new List<Core.Db.Workflow>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + Workflow.COLUMN_NAME_ID + ", "
                                                      + Workflow.COLUMN_NAME_XML
                                                      + " FROM " + Core.Db.Workflow.DOCUMENT_NAME, conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var workflow = new Workflow
                    {
                        Id = Convert.ToInt64((decimal)reader[Workflow.COLUMN_NAME_ID]),
                        Xml = (string)reader[Workflow.COLUMN_NAME_XML]
                    };

                    workflows.Add(workflow);
                }

                return workflows;
            }
        }

        private static void IncrementStatusCountColumn(string statusCountColumnName)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.StatusCount.DOCUMENT_NAME + " SET " + statusCountColumnName + " = " + statusCountColumnName + " + 1", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void IncrementDisabledCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_DISABLED_COUNT);
        }

        public override void IncrementRejectedCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_REJECTED_COUNT);
        }

        public override void IncrementDoneCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_DONE_COUNT);
        }

        public override void IncrementFailedCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_FAILED_COUNT);
        }

        public override void IncrementPendingCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_PENDING_COUNT);
        }

        public override void IncrementRunningCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_RUNNING_COUNT);
        }

        public override void IncrementStoppedCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_STOPPED_COUNT);
        }

        public override void IncrementWarningCount()
        {
            IncrementStatusCountColumn(StatusCount.COLUMN_NAME_WARNING_COUNT);
        }

        private static void DecrementStatusCountColumn(string statusCountColumnName)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.StatusCount.DOCUMENT_NAME + " SET " + statusCountColumnName + " = " + statusCountColumnName + " - 1", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DecrementPendingCount()
        {
            DecrementStatusCountColumn(StatusCount.COLUMN_NAME_PENDING_COUNT);
        }

        public override void DecrementRunningCount()
        {
            DecrementStatusCountColumn(StatusCount.COLUMN_NAME_RUNNING_COUNT);
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Entry.DOCUMENT_NAME + "("
                    + Entry.COLUMN_NAME_NAME + ", "
                    + Entry.COLUMN_NAME_DESCRIPTION + ", "
                    + Entry.COLUMN_NAME_LAUNCH_TYPE + ", "
                    + Entry.COLUMN_NAME_STATUS_DATE + ", "
                    + Entry.COLUMN_NAME_STATUS + ", "
                    + Entry.COLUMN_NAME_WORKFLOW_ID + ", "
                    + Entry.COLUMN_NAME_JOB_ID + ", "
                    + Entry.COLUMN_NAME_LOGS + ") VALUES("
                    + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "TO_TIMESTAMP('" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF'), "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + "'" + (entry.JobId ?? "") + "', "
                    + ToClob(entry.Logs)
                    + ")"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.HistoryEntry.DOCUMENT_NAME + "("
                    + HistoryEntry.COLUMN_NAME_NAME + ", "
                    + HistoryEntry.COLUMN_NAME_DESCRIPTION + ", "
                    + HistoryEntry.COLUMN_NAME_LAUNCH_TYPE + ", "
                    + HistoryEntry.COLUMN_NAME_STATUS_DATE + ", "
                    + HistoryEntry.COLUMN_NAME_STATUS + ", "
                    + HistoryEntry.COLUMN_NAME_WORKFLOW_ID + ", "
                    + HistoryEntry.COLUMN_NAME_LOGS + ") VALUES("
                    + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "TO_TIMESTAMP('" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF'), "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + ToClob(entry.Logs)
                    + ")"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.User.DOCUMENT_NAME + "("
                    + User.COLUMN_NAME_USERNAME + ", "
                    + User.COLUMN_NAME_PASSWORD + ", "
                    + User.COLUMN_NAME_USER_PROFILE + ", "
                    + User.COLUMN_NAME_EMAIL + ", "
                    + User.COLUMN_NAME_CREATED_ON + ", "
                    + User.COLUMN_NAME_MODIFIED_ON + ") VALUES("
                    + "'" + (user.Username ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (user.Password ?? "").Replace("'", "''") + "'" + ", "
                    + (int)user.UserProfile + ", "
                    + "'" + (user.Email ?? "").Replace("'", "''") + "'" + ", "
                    + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                    + (user.ModifiedOn == DateTime.MinValue ? "NULL" : ("TO_TIMESTAMP(" + "'" + user.ModifiedOn.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")) + ")"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.UserWorkflow.DOCUMENT_NAME + "("
                    + UserWorkflow.COLUMN_NAME_USER_ID + ", "
                    + UserWorkflow.COLUMN_NAME_WORKFLOW_ID + ") VALUES("
                    + int.Parse(userWorkflow.UserId) + ", "
                    + int.Parse(userWorkflow.WorkflowId) + ")"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        private static IEnumerable<string> ToChuncks(string str, int maxChunkSize)
        {
            lock (Padlock)
            {
                for (var i = 0; i < str.Length; i += maxChunkSize)
                {
                    yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
                }
            }
        }

        private static string ToClob(string str)
        {
            lock (Padlock)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return "''";
                }

                str = str.Replace("'", "''");
                var builder = new StringBuilder();
                _ = builder.Append('(');
                var chunks = ToChuncks(str, CHUNK_SIZE).ToArray();

                for (var i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];
                    _ = builder.Append("TO_CLOB(").Append('\'').Append(chunk).Append("')");
                    if (i < chunks.Length - 1)
                    {
                        _ = builder.Append(" || ");
                    }
                }

                _ = builder.Append(')');
                var val = builder.ToString();

                return val;
            }
        }

        private static string ToClob(Core.Db.Workflow workflow)
        {
            return ToClob(workflow.Xml);
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();
                var xml = ToClob(workflow);

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Workflow.DOCUMENT_NAME + "("
                    + Workflow.COLUMN_NAME_XML + ") VALUES("
                    + xml + ") RETURNING " + Workflow.COLUMN_NAME_ID + " INTO :id"
                    , conn);
                _ = command.Parameters.Add(new OracleParameter
                {
                    ParameterName = ":id",
                    DbType = System.Data.DbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                });

                _ = command.ExecuteNonQuery();

                var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                return id;
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.Entry.DOCUMENT_NAME + " SET "
                    + Entry.COLUMN_NAME_NAME + " = '" + (entry.Name ?? "").Replace("'", "''") + "', "
                    + Entry.COLUMN_NAME_DESCRIPTION + " = '" + (entry.Description ?? "").Replace("'", "''") + "', "
                    + Entry.COLUMN_NAME_LAUNCH_TYPE + " = " + (int)entry.LaunchType + ", "
                    + Entry.COLUMN_NAME_STATUS_DATE + " = TO_TIMESTAMP('" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF'), "
                    + Entry.COLUMN_NAME_STATUS + " = " + (int)entry.Status + ", "
                    + Entry.COLUMN_NAME_WORKFLOW_ID + " = " + entry.WorkflowId + ", "
                    + Entry.COLUMN_NAME_JOB_ID + " = '" + (entry.JobId ?? "") + "', "
                    + Entry.COLUMN_NAME_LOGS + " = " + ToClob(entry.Logs)
                    + " WHERE "
                    + Entry.COLUMN_NAME_ID + " = " + int.Parse(id)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.User.DOCUMENT_NAME + " SET "
                    + User.COLUMN_NAME_PASSWORD + " = '" + (password ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + User.COLUMN_NAME_USERNAME + " = '" + (username ?? "").Replace("'", "''") + "'"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.User.DOCUMENT_NAME + " SET "
                    + User.COLUMN_NAME_USERNAME + " = '" + (user.Username ?? "").Replace("'", "''") + "', "
                    + User.COLUMN_NAME_PASSWORD + " = '" + (user.Password ?? "").Replace("'", "''") + "', "
                    + User.COLUMN_NAME_USER_PROFILE + " = " + (int)user.UserProfile + ", "
                    + User.COLUMN_NAME_EMAIL + " = '" + (user.Email ?? "").Replace("'", "''") + "', "
                    + User.COLUMN_NAME_CREATED_ON + " = TO_TIMESTAMP('" + user.CreatedOn.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                    + User.COLUMN_NAME_MODIFIED_ON + " = TO_TIMESTAMP('" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF')"
                    + " WHERE "
                    + User.COLUMN_NAME_ID + " = " + int.Parse(id)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.User.DOCUMENT_NAME + " SET "
                    + User.COLUMN_NAME_USERNAME + " = '" + (username ?? "").Replace("'", "''") + "', "
                    + User.COLUMN_NAME_USER_PROFILE + " = " + (int)up + ", "
                    + User.COLUMN_NAME_EMAIL + " = '" + (email ?? "").Replace("'", "''") + "', "
                    + User.COLUMN_NAME_MODIFIED_ON + " = TO_TIMESTAMP('" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "', 'YYYY-MM-DD HH24:MI:SS.FF')"
                    + " WHERE "
                    + User.COLUMN_NAME_ID + " = " + int.Parse(userId)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var xml = ToClob(workflow);

                using var command = new OracleCommand("UPDATE " + Core.Db.Workflow.DOCUMENT_NAME + " SET "
                    + Workflow.COLUMN_NAME_XML + " = " + xml
                    + " WHERE "
                    + User.COLUMN_NAME_ID + " = " + int.Parse(dbId)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + Entry.COLUMN_NAME_LOGS
                    + " FROM " + Core.Db.Entry.DOCUMENT_NAME
                    + " WHERE "
                    + Entry.COLUMN_NAME_ID + " = " + int.Parse(entryId)
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var logs = reader[Entry.COLUMN_NAME_LOGS] == DBNull.Value ? string.Empty : (string)reader[Entry.COLUMN_NAME_LOGS];
                    return logs;
                }

                return null;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT " + HistoryEntry.COLUMN_NAME_LOGS
                    + " FROM " + Core.Db.HistoryEntry.DOCUMENT_NAME
                    + " WHERE "
                    + HistoryEntry.COLUMN_NAME_ID + " = " + int.Parse(entryId)
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var logs = reader[Entry.COLUMN_NAME_LOGS] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.COLUMN_NAME_LOGS];
                    return logs;
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + User.COLUMN_NAME_ID + ", "
                                                      + User.COLUMN_NAME_USERNAME + ", "
                                                      + User.COLUMN_NAME_PASSWORD + ", "
                                                      + User.COLUMN_NAME_EMAIL + ", "
                                                      + User.COLUMN_NAME_USER_PROFILE + ", "
                                                      + User.COLUMN_NAME_CREATED_ON + ", "
                                                      + User.COLUMN_NAME_MODIFIED_ON
                                                      + " FROM " + Core.Db.User.DOCUMENT_NAME
                                                      + " WHERE (" + User.COLUMN_NAME_USER_PROFILE + " = " + (int)UserProfile.SuperAdministrator
                                                      + " OR " + User.COLUMN_NAME_USER_PROFILE + " = " + (int)UserProfile.Administrator + ")"
                                                      + " ORDER BY " + User.COLUMN_NAME_USERNAME
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var admin = new User
                    {
                        Id = Convert.ToInt64((decimal)reader[User.COLUMN_NAME_ID]),
                        Username = (string)reader[User.COLUMN_NAME_USERNAME],
                        Password = (string)reader[User.COLUMN_NAME_PASSWORD],
                        Email = reader[User.COLUMN_NAME_EMAIL] == DBNull.Value ? string.Empty : (string)reader[User.COLUMN_NAME_EMAIL],
                        UserProfile = (UserProfile)Convert.ToInt32((decimal)reader[User.COLUMN_NAME_USER_PROFILE]),
                        CreatedOn = (DateTime)reader[User.COLUMN_NAME_CREATED_ON],
                        ModifiedOn = reader[User.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.COLUMN_NAME_MODIFIED_ON]
                    };

                    users.Add(admin);
                }

                return users;
            }
        }

        public override string InsertRecord(Core.Db.Record record)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Record.DOCUMENT_NAME + "("
                    + Record.COLUMN_NAME_NAME + ", "
                    + Record.COLUMN_NAME_DESCRIPTION + ", "
                    + Record.COLUMN_NAME_APPROVED + ", "
                    + Record.COLUMN_NAME_START_DATE + ", "
                    + Record.COLUMN_NAME_END_DATE + ", "
                    + Record.COLUMN_NAME_COMMENTS + ", "
                    + Record.COLUMN_NAME_MANAGER_COMMENTS + ", "
                    + Record.COLUMN_NAME_CREATED_BY + ", "
                    + Record.COLUMN_NAME_CREATED_ON + ", "
                    + Record.COLUMN_NAME_MODIFIED_BY + ", "
                    + Record.COLUMN_NAME_MODIFIED_ON + ", "
                    + Record.COLUMN_NAME_ASSIGNED_TO + ", "
                    + Record.COLUMN_NAME_ASSIGNED_ON + ")"
                    + " VALUES("
                    + "'" + (record.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (record.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (record.Approved ? "1" : "0") + ", "
                    + (record.StartDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                    + (record.EndDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                    + "'" + (record.Comments ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (record.ManagerComments ?? "").Replace("'", "''") + "'" + ", "
                    + int.Parse(record.CreatedBy) + ", "
                    + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                    + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                    + (record.ModifiedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.ModifiedOn.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                     + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                    + (record.AssignedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ")"
                    + " RETURNING " + Record.COLUMN_NAME_ID + " INTO :id"
                    , conn);
                _ = command.Parameters.Add(new OracleParameter
                {
                    ParameterName = ":id",
                    DbType = System.Data.DbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                });

                _ = command.ExecuteNonQuery();

                var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                return id;
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.Record.DOCUMENT_NAME + " SET "
                    + Record.COLUMN_NAME_NAME + " = '" + (record.Name ?? "").Replace("'", "''") + "', "
                    + Record.COLUMN_NAME_DESCRIPTION + " = '" + (record.Description ?? "").Replace("'", "''") + "', "
                    + Record.COLUMN_NAME_APPROVED + " = " + (record.Approved ? "1" : "0") + ", "
                    + Record.COLUMN_NAME_START_DATE + " = " + (record.StartDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                    + Record.COLUMN_NAME_END_DATE + " = " + (record.EndDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                    + Record.COLUMN_NAME_COMMENTS + " = '" + (record.Comments ?? "").Replace("'", "''") + "', "
                    + Record.COLUMN_NAME_MANAGER_COMMENTS + " = '" + (record.ManagerComments ?? "").Replace("'", "''") + "', "
                    + Record.COLUMN_NAME_CREATED_BY + " = " + int.Parse(record.CreatedBy) + ", "
                    + Record.COLUMN_NAME_MODIFIED_BY + " = " + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                    + Record.COLUMN_NAME_MODIFIED_ON + " = " + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                    + Record.COLUMN_NAME_ASSIGNED_TO + " = " + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                    + Record.COLUMN_NAME_ASSIGNED_ON + " = " + (record.AssignedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")
                    + " WHERE "
                    + Record.COLUMN_NAME_ID + " = " + int.Parse(recordId)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteRecords(string[] recordIds)
        {
            lock (Padlock)
            {
                if (recordIds.Length > 0)
                {
                    using var conn = new OracleConnection(_connectionString);
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < recordIds.Length; i++)
                    {
                        var id = recordIds[i];
                        _ = builder.Append(id);
                        _ = i < recordIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using var command = new OracleCommand("DELETE FROM " + Core.Db.Record.DOCUMENT_NAME
                        + " WHERE " + Record.COLUMN_NAME_ID + " IN " + builder, conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                    + Record.COLUMN_NAME_ID + ", "
                    + Record.COLUMN_NAME_NAME + ", "
                    + Record.COLUMN_NAME_DESCRIPTION + ", "
                    + Record.COLUMN_NAME_APPROVED + ", "
                    + Record.COLUMN_NAME_START_DATE + ", "
                    + Record.COLUMN_NAME_END_DATE + ", "
                    + Record.COLUMN_NAME_COMMENTS + ", "
                    + Record.COLUMN_NAME_MANAGER_COMMENTS + ", "
                    + Record.COLUMN_NAME_CREATED_BY + ", "
                    + Record.COLUMN_NAME_CREATED_ON + ", "
                    + Record.COLUMN_NAME_MODIFIED_BY + ", "
                    + Record.COLUMN_NAME_MODIFIED_ON + ", "
                    + Record.COLUMN_NAME_ASSIGNED_TO + ", "
                    + Record.COLUMN_NAME_ASSIGNED_ON
                    + " FROM " + Core.Db.Record.DOCUMENT_NAME
                    + " WHERE " + Record.COLUMN_NAME_ID + " = " + int.Parse(id)
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var record = new Record
                    {
                        Id = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ID]),
                        Name = reader[Record.COLUMN_NAME_NAME] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_NAME],
                        Description = reader[Record.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_DESCRIPTION],
                        Approved = ((short)reader[Record.COLUMN_NAME_APPROVED]) == 1,
                        StartDate = reader[Record.COLUMN_NAME_START_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_START_DATE],
                        EndDate = reader[Record.COLUMN_NAME_END_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_END_DATE],
                        Comments = reader[Record.COLUMN_NAME_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_COMMENTS],
                        ManagerComments = reader[Record.COLUMN_NAME_MANAGER_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_MANAGER_COMMENTS],
                        CreatedBy = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_CREATED_BY]).ToString(),
                        CreatedOn = (DateTime)reader[Record.COLUMN_NAME_CREATED_ON],
                        ModifiedBy = reader[Record.COLUMN_NAME_MODIFIED_BY] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_MODIFIED_BY]).ToString(),
                        ModifiedOn = reader[Record.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_MODIFIED_ON],
                        AssignedTo = reader[Record.COLUMN_NAME_ASSIGNED_TO] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ASSIGNED_TO]).ToString(),
                        AssignedOn = reader[Record.COLUMN_NAME_ASSIGNED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_ASSIGNED_ON]
                    };

                    return record;
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecords(string keyword)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Record.COLUMN_NAME_ID + ", "
                                                      + Record.COLUMN_NAME_NAME + ", "
                                                      + Record.COLUMN_NAME_DESCRIPTION + ", "
                                                      + Record.COLUMN_NAME_APPROVED + ", "
                                                      + Record.COLUMN_NAME_START_DATE + ", "
                                                      + Record.COLUMN_NAME_END_DATE + ", "
                                                      + Record.COLUMN_NAME_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_MANAGER_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_CREATED_BY + ", "
                                                      + Record.COLUMN_NAME_CREATED_ON + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_BY + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_ON + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_TO + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_ON
                                                      + " FROM " + Core.Db.Record.DOCUMENT_NAME
                                                      + " WHERE " + "LOWER(" + Record.COLUMN_NAME_NAME + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " OR " + "LOWER(" + Record.COLUMN_NAME_DESCRIPTION + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " ORDER BY " + Record.COLUMN_NAME_CREATED_ON + " DESC"
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var record = new Record
                    {
                        Id = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ID]),
                        Name = reader[Record.COLUMN_NAME_NAME] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_NAME],
                        Description = reader[Record.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_DESCRIPTION],
                        Approved = ((short)reader[Record.COLUMN_NAME_APPROVED]) == 1,
                        StartDate = reader[Record.COLUMN_NAME_START_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_START_DATE],
                        EndDate = reader[Record.COLUMN_NAME_END_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_END_DATE],
                        Comments = reader[Record.COLUMN_NAME_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_COMMENTS],
                        ManagerComments = reader[Record.COLUMN_NAME_MANAGER_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_MANAGER_COMMENTS],
                        CreatedBy = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_CREATED_BY]).ToString(),
                        CreatedOn = (DateTime)reader[Record.COLUMN_NAME_CREATED_ON],
                        ModifiedBy = reader[Record.COLUMN_NAME_MODIFIED_BY] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_MODIFIED_BY]).ToString(),
                        ModifiedOn = reader[Record.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_MODIFIED_ON],
                        AssignedTo = reader[Record.COLUMN_NAME_ASSIGNED_TO] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ASSIGNED_TO]).ToString(),
                        AssignedOn = reader[Record.COLUMN_NAME_ASSIGNED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_ASSIGNED_ON]
                    };

                    records.Add(record);
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedBy(string createdBy)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Record.COLUMN_NAME_ID + ", "
                                                      + Record.COLUMN_NAME_NAME + ", "
                                                      + Record.COLUMN_NAME_DESCRIPTION + ", "
                                                      + Record.COLUMN_NAME_APPROVED + ", "
                                                      + Record.COLUMN_NAME_START_DATE + ", "
                                                      + Record.COLUMN_NAME_END_DATE + ", "
                                                      + Record.COLUMN_NAME_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_MANAGER_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_CREATED_BY + ", "
                                                      + Record.COLUMN_NAME_CREATED_ON + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_BY + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_ON + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_TO + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_ON
                                                      + " FROM " + Core.Db.Record.DOCUMENT_NAME
                                                      + " WHERE " + Record.COLUMN_NAME_CREATED_BY + " = " + int.Parse(createdBy)
                                                      + " ORDER BY " + Record.COLUMN_NAME_NAME + " ASC"
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var record = new Record
                    {
                        Id = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ID]),
                        Name = reader[Record.COLUMN_NAME_NAME] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_NAME],
                        Description = reader[Record.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_DESCRIPTION],
                        Approved = ((short)reader[Record.COLUMN_NAME_APPROVED]) == 1,
                        StartDate = reader[Record.COLUMN_NAME_START_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_START_DATE],
                        EndDate = reader[Record.COLUMN_NAME_END_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_END_DATE],
                        Comments = reader[Record.COLUMN_NAME_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_COMMENTS],
                        ManagerComments = reader[Record.COLUMN_NAME_MANAGER_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_MANAGER_COMMENTS],
                        CreatedBy = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_CREATED_BY]).ToString(),
                        CreatedOn = (DateTime)reader[Record.COLUMN_NAME_CREATED_ON],
                        ModifiedBy = reader[Record.COLUMN_NAME_MODIFIED_BY] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_MODIFIED_BY]).ToString(),
                        ModifiedOn = reader[Record.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_MODIFIED_ON],
                        AssignedTo = reader[Record.COLUMN_NAME_ASSIGNED_TO] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ASSIGNED_TO]).ToString(),
                        AssignedOn = reader[Record.COLUMN_NAME_ASSIGNED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_ASSIGNED_ON]
                    };

                    records.Add(record);
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Record.COLUMN_NAME_ID + ", "
                                                      + Record.COLUMN_NAME_NAME + ", "
                                                      + Record.COLUMN_NAME_DESCRIPTION + ", "
                                                      + Record.COLUMN_NAME_APPROVED + ", "
                                                      + Record.COLUMN_NAME_START_DATE + ", "
                                                      + Record.COLUMN_NAME_END_DATE + ", "
                                                      + Record.COLUMN_NAME_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_MANAGER_COMMENTS + ", "
                                                      + Record.COLUMN_NAME_CREATED_BY + ", "
                                                      + Record.COLUMN_NAME_CREATED_ON + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_BY + ", "
                                                      + Record.COLUMN_NAME_MODIFIED_ON + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_TO + ", "
                                                      + Record.COLUMN_NAME_ASSIGNED_ON
                                                      + " FROM " + Core.Db.Record.DOCUMENT_NAME
                                                      + " WHERE " + "(LOWER(" + Record.COLUMN_NAME_NAME + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " OR " + "LOWER(" + Record.COLUMN_NAME_DESCRIPTION + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                                      + " AND (" + Record.COLUMN_NAME_CREATED_BY + " = " + int.Parse(createdBy) + " OR " + Record.COLUMN_NAME_ASSIGNED_TO + " = " + int.Parse(assingedTo) + ")"
                                                      + " ORDER BY " + Record.COLUMN_NAME_CREATED_ON + " DESC"
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var record = new Record
                    {
                        Id = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ID]),
                        Name = reader[Record.COLUMN_NAME_NAME] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_NAME],
                        Description = reader[Record.COLUMN_NAME_DESCRIPTION] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_DESCRIPTION],
                        Approved = ((short)reader[Record.COLUMN_NAME_APPROVED]) == 1,
                        StartDate = reader[Record.COLUMN_NAME_START_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_START_DATE],
                        EndDate = reader[Record.COLUMN_NAME_END_DATE] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_END_DATE],
                        Comments = reader[Record.COLUMN_NAME_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_COMMENTS],
                        ManagerComments = reader[Record.COLUMN_NAME_MANAGER_COMMENTS] == DBNull.Value ? null : (string)reader[Record.COLUMN_NAME_MANAGER_COMMENTS],
                        CreatedBy = Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_CREATED_BY]).ToString(),
                        CreatedOn = (DateTime)reader[Record.COLUMN_NAME_CREATED_ON],
                        ModifiedBy = reader[Record.COLUMN_NAME_MODIFIED_BY] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_MODIFIED_BY]).ToString(),
                        ModifiedOn = reader[Record.COLUMN_NAME_MODIFIED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_MODIFIED_ON],
                        AssignedTo = reader[Record.COLUMN_NAME_ASSIGNED_TO] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.COLUMN_NAME_ASSIGNED_TO]).ToString(),
                        AssignedOn = reader[Record.COLUMN_NAME_ASSIGNED_ON] == DBNull.Value ? null : (DateTime?)reader[Record.COLUMN_NAME_ASSIGNED_ON]
                    };

                    records.Add(record);
                }

                return records;
            }
        }

        public override string InsertVersion(Core.Db.Version version)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Version.DOCUMENT_NAME + "("
                    + Version.COLUMN_NAME_RECORD_ID + ", "
                    + Version.COLUMN_NAME_FILE_PATH + ", "
                    + Version.COLUMN_NAME_CREATED_ON + ")"
                    + " VALUES("
                    + int.Parse(version.RecordId) + ", "
                    + "'" + (version.FilePath ?? "").Replace("'", "''") + "'" + ", "
                    + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ")"
                    + " RETURNING " + Version.COLUMN_NAME_ID + " INTO :id"
                    , conn);
                _ = command.Parameters.Add(new OracleParameter
                {
                    ParameterName = ":id",
                    DbType = System.Data.DbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                });

                _ = command.ExecuteNonQuery();

                var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                return id;
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.Version.DOCUMENT_NAME + " SET "
                    + Version.COLUMN_NAME_RECORD_ID + " = " + int.Parse(version.RecordId) + ", "
                    + Version.COLUMN_NAME_FILE_PATH + " = '" + (version.FilePath ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + Version.COLUMN_NAME_ID + " = " + int.Parse(versionId)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteVersions(string[] versionIds)
        {
            lock (Padlock)
            {
                if (versionIds.Length > 0)
                {
                    using var conn = new OracleConnection(_connectionString);
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < versionIds.Length; i++)
                    {
                        var id = versionIds[i];
                        _ = builder.Append(id);
                        _ = i < versionIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using var command = new OracleCommand("DELETE FROM " + Core.Db.Version.DOCUMENT_NAME
                        + " WHERE " + Version.COLUMN_NAME_ID + " IN " + builder, conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (Padlock)
            {
                var versions = new List<Version>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Version.COLUMN_NAME_ID + ", "
                                                      + Version.COLUMN_NAME_RECORD_ID + ", "
                                                      + Version.COLUMN_NAME_FILE_PATH + ", "
                                                      + Version.COLUMN_NAME_CREATED_ON
                                                      + " FROM " + Core.Db.Version.DOCUMENT_NAME
                                                      + " WHERE " + Version.COLUMN_NAME_RECORD_ID + " = " + int.Parse(recordId)
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var version = new Version
                    {
                        Id = Convert.ToInt64((decimal)reader[Version.COLUMN_NAME_ID]),
                        RecordId = Convert.ToInt64((decimal)reader[Version.COLUMN_NAME_RECORD_ID]).ToString(),
                        FilePath = reader[Version.COLUMN_NAME_FILE_PATH] == DBNull.Value ? null : (string)reader[Version.COLUMN_NAME_FILE_PATH],
                        CreatedOn = (DateTime)reader[Version.COLUMN_NAME_CREATED_ON]
                    };

                    versions.Add(version);
                }

                return versions;
            }
        }

        public override Core.Db.Version GetLatestVersion(string recordId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                    + Version.COLUMN_NAME_ID + ", "
                    + Version.COLUMN_NAME_RECORD_ID + ", "
                    + Version.COLUMN_NAME_FILE_PATH + ", "
                    + Version.COLUMN_NAME_CREATED_ON
                    + " FROM " + Core.Db.Version.DOCUMENT_NAME
                    + " WHERE " + Version.COLUMN_NAME_RECORD_ID + " = " + int.Parse(recordId)
                    + " ORDER BY " + Version.COLUMN_NAME_CREATED_ON + " DESC"
                    + " FETCH NEXT 1 ROWS ONLY", conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var version = new Version
                    {
                        Id = Convert.ToInt64((decimal)reader[Version.COLUMN_NAME_ID]),
                        RecordId = Convert.ToInt64((decimal)reader[Version.COLUMN_NAME_RECORD_ID]).ToString(),
                        FilePath = reader[Version.COLUMN_NAME_FILE_PATH] == DBNull.Value ? null : (string)reader[Version.COLUMN_NAME_FILE_PATH],
                        CreatedOn = (DateTime)reader[Version.COLUMN_NAME_CREATED_ON]
                    };

                    return version;
                }

                return null;
            }
        }

        public override string InsertNotification(Core.Db.Notification notification)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Notification.DOCUMENT_NAME + "("
                    + Notification.COLUMN_NAME_ASSIGNED_BY + ", "
                    + Notification.COLUMN_NAME_ASSIGNED_ON + ", "
                    + Notification.COLUMN_NAME_ASSIGNED_TO + ", "
                    + Notification.COLUMN_NAME_MESSAGE + ", "
                    + Notification.COLUMN_NAME_IS_READ + ")"
                    + " VALUES("
                    + (!string.IsNullOrEmpty(notification.AssignedBy) ? int.Parse(notification.AssignedBy).ToString() : "NULL") + ", "
                    + "TO_TIMESTAMP(" + "'" + notification.AssignedOn.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                    + (!string.IsNullOrEmpty(notification.AssignedTo) ? int.Parse(notification.AssignedTo).ToString() : "NULL") + ", "
                    + "'" + (notification.Message ?? "").Replace("'", "''") + "'" + ", "
                    + (notification.IsRead ? "1" : "0") + ")"
                    + " RETURNING " + Notification.COLUMN_NAME_ID + " INTO :id"
                    , conn);
                _ = command.Parameters.Add(new OracleParameter
                {
                    ParameterName = ":id",
                    DbType = System.Data.DbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                });

                _ = command.ExecuteNonQuery();

                var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                return id;
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var builder = new StringBuilder("(");

                for (var i = 0; i < notificationIds.Length; i++)
                {
                    var id = notificationIds[i];
                    _ = builder.Append(id);
                    _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using var command = new OracleCommand("UPDATE " + Core.Db.Notification.DOCUMENT_NAME
                    + " SET " + Notification.COLUMN_NAME_IS_READ + " = " + "1"
                    + " WHERE " + Notification.COLUMN_NAME_ID + " IN " + builder, conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var builder = new StringBuilder("(");

                for (var i = 0; i < notificationIds.Length; i++)
                {
                    var id = notificationIds[i];
                    _ = builder.Append(id);
                    _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using var command = new OracleCommand("UPDATE " + Core.Db.Notification.DOCUMENT_NAME
                    + " SET " + Notification.COLUMN_NAME_IS_READ + " = " + "0"
                    + " WHERE " + Notification.COLUMN_NAME_ID + " IN " + builder, conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (Padlock)
            {
                if (notificationIds.Length > 0)
                {
                    using var conn = new OracleConnection(_connectionString);
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        _ = builder.Append(id);
                        _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using var command = new OracleCommand("DELETE FROM " + Core.Db.Notification.DOCUMENT_NAME
                        + " WHERE " + Notification.COLUMN_NAME_ID + " IN " + builder, conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (Padlock)
            {
                var notifications = new List<Notification>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Notification.COLUMN_NAME_ID + ", "
                                                      + Notification.COLUMN_NAME_ASSIGNED_BY + ", "
                                                      + Notification.COLUMN_NAME_ASSIGNED_ON + ", "
                                                      + Notification.COLUMN_NAME_ASSIGNED_TO + ", "
                                                      + Notification.COLUMN_NAME_MESSAGE + ", "
                                                      + Notification.COLUMN_NAME_IS_READ
                                                      + " FROM " + Core.Db.Notification.DOCUMENT_NAME
                                                      + " WHERE " + "(LOWER(" + Notification.COLUMN_NAME_MESSAGE + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                      + " AND " + Notification.COLUMN_NAME_ASSIGNED_TO + " = " + int.Parse(assignedTo) + ")"
                                                      + " ORDER BY " + Notification.COLUMN_NAME_ASSIGNED_ON + " DESC"
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var notification = new Notification
                    {
                        Id = Convert.ToInt64((decimal)reader[Notification.COLUMN_NAME_ID]),
                        AssignedBy = Convert.ToInt64((decimal)reader[Notification.COLUMN_NAME_ASSIGNED_BY]).ToString(),
                        AssignedOn = (DateTime)reader[Notification.COLUMN_NAME_ASSIGNED_ON],
                        AssignedTo = Convert.ToInt64((decimal)reader[Notification.COLUMN_NAME_ASSIGNED_TO]).ToString(),
                        Message = reader[Notification.COLUMN_NAME_MESSAGE] == DBNull.Value ? null : (string)reader[Notification.COLUMN_NAME_MESSAGE],
                        IsRead = ((short)reader[Notification.COLUMN_NAME_IS_READ]) == 1
                    };

                    notifications.Add(notification);
                }

                return notifications;
            }
        }

        public override bool HasNotifications(string assignedTo)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.Notification.DOCUMENT_NAME
                    + " WHERE (" + Notification.COLUMN_NAME_ASSIGNED_TO + " = " + int.Parse(assignedTo)
                    + " AND " + Notification.COLUMN_NAME_IS_READ + " = " + "0" + ")"
                    , conn);
                var count = Convert.ToInt64((decimal)command.ExecuteScalar());
                var hasNotifications = count > 0;
                return hasNotifications;
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("INSERT INTO " + Core.Db.Approver.DOCUMENT_NAME + "("
                    + Approver.COLUMN_NAME_USER_ID + ", "
                    + Approver.COLUMN_NAME_RECORD_ID + ", "
                    + Approver.COLUMN_NAME_APPROVED + ", "
                    + Approver.COLUMN_NAME_APPROVED_ON + ") VALUES("
                    + int.Parse(approver.UserId) + ", "
                    + int.Parse(approver.RecordId) + ", "
                    + (approver.Approved ? "1" : "0") + ", "
                    + (approver.ApprovedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ") "
                    + "RETURNING " + Approver.COLUMN_NAME_ID + " INTO :id"
                    , conn);
                _ = command.Parameters.Add(new OracleParameter
                {
                    ParameterName = ":id",
                    DbType = System.Data.DbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                });

                _ = command.ExecuteNonQuery();

                var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                return id;
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("UPDATE " + Core.Db.Approver.DOCUMENT_NAME + " SET "
                    + Approver.COLUMN_NAME_USER_ID + " = " + int.Parse(approver.UserId) + ", "
                    + Approver.COLUMN_NAME_RECORD_ID + " = " + int.Parse(approver.RecordId) + ", "
                    + Approver.COLUMN_NAME_APPROVED + " = " + (approver.Approved ? "1" : "0") + ", "
                    + Approver.COLUMN_NAME_APPROVED_ON + " = " + (approver.ApprovedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")
                    + " WHERE "
                    + Approver.COLUMN_NAME_ID + " = " + int.Parse(approverId)
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DOCUMENT_NAME
                    + " WHERE " + Approver.COLUMN_NAME_RECORD_ID + " = " + int.Parse(recordId), conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DOCUMENT_NAME
                    + " WHERE " + Approver.COLUMN_NAME_RECORD_ID + " = " + int.Parse(recordId)
                    + " AND " + Approver.COLUMN_NAME_APPROVED + " = " + "1"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (Padlock)
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DOCUMENT_NAME
                    + " WHERE " + Approver.COLUMN_NAME_USER_ID + " = " + int.Parse(userId), conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (Padlock)
            {
                var approvers = new List<Approver>();

                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var command = new OracleCommand("SELECT "
                                                      + Approver.COLUMN_NAME_ID + ", "
                                                      + Approver.COLUMN_NAME_USER_ID + ", "
                                                      + Approver.COLUMN_NAME_RECORD_ID + ", "
                                                      + Approver.COLUMN_NAME_APPROVED + ", "
                                                      + Approver.COLUMN_NAME_APPROVED_ON
                                                      + " FROM " + Core.Db.Approver.DOCUMENT_NAME
                                                      + " WHERE " + Approver.COLUMN_NAME_RECORD_ID + " = " + int.Parse(recordId)
                    , conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var approver = new Approver
                    {
                        Id = Convert.ToInt64((decimal)reader[Approver.COLUMN_NAME_ID]),
                        UserId = Convert.ToInt64((decimal)reader[Approver.COLUMN_NAME_USER_ID]).ToString(),
                        RecordId = Convert.ToInt64((decimal)reader[Approver.COLUMN_NAME_RECORD_ID]).ToString(),
                        Approved = (short)reader[Approver.COLUMN_NAME_APPROVED] == 1,
                        ApprovedOn = reader[Approver.COLUMN_NAME_APPROVED_ON] == DBNull.Value ? null : (DateTime?)reader[Approver.COLUMN_NAME_APPROVED_ON]
                    };

                    approvers.Add(approver);
                }

                return approvers;
            }
        }

        public override void Dispose()
        {
        }
    }
}
