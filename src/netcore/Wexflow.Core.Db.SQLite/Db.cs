using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Wexflow.Core.Db.SQLite
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object Padlock = new();
        private const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        private static string _connectionString;

        public Db(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;

            var dataSource = string.Empty;

            var connectionStringParts = ConnectionString.Split(';');

            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    var connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("Data Source="))
                    {
                        dataSource = connPart.Replace("Data Source=", string.Empty);
                        break;
                    }
                }
            }

            Helper helper = new(connectionString);
            Helper.CreateDatabaseIfNotExists(dataSource);
            helper.CreateTableIfNotExists(Core.Db.Entry.DocumentName, Entry.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.HistoryEntry.DocumentName, HistoryEntry.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.StatusCount.DocumentName, StatusCount.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.User.DocumentName, User.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.UserWorkflow.DocumentName, UserWorkflow.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Workflow.DocumentName, Workflow.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Version.DocumentName, Version.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Record.DocumentName, Record.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Notification.DocumentName, Notification.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Approver.DocumentName, Approver.TableStruct);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            StatusCount statusCount = new()
            {
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            using (SQLiteConnection conn = new(_connectionString))
            {
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.StatusCount.DocumentName + "("
                    + StatusCount.ColumnNamePendingCount + ", "
                    + StatusCount.ColumnNameRunningCount + ", "
                    + StatusCount.ColumnNameDoneCount + ", "
                    + StatusCount.ColumnNameFailedCount + ", "
                    + StatusCount.ColumnNameWarningCount + ", "
                    + StatusCount.ColumnNameDisabledCount + ", "
                    + StatusCount.ColumnNameStoppedCount + ", "
                    + StatusCount.ColumnNameRejectedCount + ") VALUES("
                    + statusCount.PendingCount + ", "
                    + statusCount.RunningCount + ", "
                    + statusCount.DoneCount + ", "
                    + statusCount.FailedCount + ", "
                    + statusCount.WarningCount + ", "
                    + statusCount.DisabledCount + ", "
                    + statusCount.StoppedCount + ", "
                    + statusCount.RejectedCount + ");"
                    , conn);
                _ = command.ExecuteNonQuery();
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (SQLiteConnection conn = new(_connectionString))
            {
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*) FROM " + Core.Db.User.DocumentName + ";", conn);
                var usersCount = (long)command.ExecuteScalar();

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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnNameUserId + "=" + int.Parse(userId)
                    + " AND " + UserWorkflow.ColumnNameWorkflowId + "=" + int.Parse(workflowId)
                    + ";", conn);

                var count = (long)command.ExecuteScalar();

                return count > 0;
            }
        }

        public override void ClearEntries()
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Entry.DocumentName + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void ClearStatusCount()
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.StatusCount.DocumentName + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnNameUsername + " = '" + username + "'"
                    + " AND " + User.ColumnNamePassword + " = '" + password + "'"
                    + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnNameUserId + " = " + int.Parse(userId) + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnNameWorkflowId + " = " + int.Parse(workflowDbId) + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnNameId + " = " + int.Parse(id) + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                StringBuilder builder = new("(");

                for (var i = 0; i < ids.Length; i++)
                {
                    var id = ids[i];
                    _ = builder.Append(id);
                    _ = i < ids.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnNameId + " IN " + builder + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (Padlock)
            {
                List<User> admins = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNameId + ", "
                                                  + User.ColumnNameUsername + ", "
                                                  + User.ColumnNamePassword + ", "
                                                  + User.ColumnNameEmail + ", "
                                                  + User.ColumnNameUserProfile + ", "
                                                  + User.ColumnNameCreatedOn + ", "
                                                  + User.ColumnNameModifiedOn
                                                  + " FROM " + Core.Db.User.DocumentName
                                                  + " WHERE " + "(LOWER(" + User.ColumnNameUsername + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " AND " + User.ColumnNameUserProfile + " = " + (int)UserProfile.Administrator + ")"
                                                  + " ORDER BY " + User.ColumnNameUsername + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                                                  + ";", conn);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User admin = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
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
                List<Entry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Entry.ColumnNameId + ", "
                                                  + Entry.ColumnNameName + ", "
                                                  + Entry.ColumnNameDescription + ", "
                                                  + Entry.ColumnNameLaunchType + ", "
                                                  + Entry.ColumnNameStatus + ", "
                                                  + Entry.ColumnNameStatusDate + ", "
                                                  + Entry.ColumnNameWorkflowId + ", "
                                                  + Entry.ColumnNameJobId
                                                  + " FROM " + Core.Db.Entry.DocumentName + ";", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Entry entry = new()
                    {
                        Id = (long)reader[Entry.ColumnNameId],
                        Name = (string)reader[Entry.ColumnNameName],
                        Description = (string)reader[Entry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[Entry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[Entry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[Entry.ColumnNameWorkflowId],
                        JobId = (string)reader[Entry.ColumnNameJobId]
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
                List<Entry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                StringBuilder sqlBuilder = new("SELECT "
                                               + Entry.ColumnNameId + ", "
                                               + Entry.ColumnNameName + ", "
                                               + Entry.ColumnNameDescription + ", "
                                               + Entry.ColumnNameLaunchType + ", "
                                               + Entry.ColumnNameStatus + ", "
                                               + Entry.ColumnNameStatusDate + ", "
                                               + Entry.ColumnNameWorkflowId + ", "
                                               + Entry.ColumnNameJobId
                                               + " FROM " + Core.Db.Entry.DocumentName
                                               + " WHERE " + "(LOWER(" + Entry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                               + " OR " + "LOWER(" + Entry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                               + " AND (" + Entry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "')"
                                               + " ORDER BY ");

                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameStatusDate).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameStatusDate).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameWorkflowId).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameWorkflowId).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameName).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameName).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameLaunchType).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameLaunchType).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameDescription).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameDescription).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        _ = sqlBuilder.Append(Entry.ColumnNameStatus).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        _ = sqlBuilder.Append(Entry.ColumnNameStatus).Append(" DESC");
                        break;
                }

                _ = sqlBuilder.Append(" LIMIT ").Append(entriesCount).Append(" OFFSET ").Append((page - 1) * entriesCount).Append(';');

                using SQLiteCommand command = new(sqlBuilder.ToString(), conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Entry entry = new()
                    {
                        Id = (long)reader[Entry.ColumnNameId],
                        Name = (string)reader[Entry.ColumnNameName],
                        Description = (string)reader[Entry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[Entry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[Entry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[Entry.ColumnNameWorkflowId],
                        JobId = (string)reader[Entry.ColumnNameJobId]
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*)"
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE " + "(LOWER(" + Entry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + Entry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                    + " AND (" + Entry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "');", conn);

                var count = (long)command.ExecuteScalar();

                return count;
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                    + Entry.ColumnNameId + ", "
                    + Entry.ColumnNameName + ", "
                    + Entry.ColumnNameDescription + ", "
                    + Entry.ColumnNameLaunchType + ", "
                    + Entry.ColumnNameStatus + ", "
                    + Entry.ColumnNameStatusDate + ", "
                    + Entry.ColumnNameWorkflowId + ", "
                    + Entry.ColumnNameJobId
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE " + Entry.ColumnNameWorkflowId + " = " + workflowId + ";", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Entry entry = new()
                    {
                        Id = (long)reader[Entry.ColumnNameId],
                        Name = (string)reader[Entry.ColumnNameName],
                        Description = (string)reader[Entry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[Entry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[Entry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[Entry.ColumnNameWorkflowId],
                        JobId = (string)reader[Entry.ColumnNameJobId]
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                    + Entry.ColumnNameId + ", "
                    + Entry.ColumnNameName + ", "
                    + Entry.ColumnNameDescription + ", "
                    + Entry.ColumnNameLaunchType + ", "
                    + Entry.ColumnNameStatus + ", "
                    + Entry.ColumnNameStatusDate + ", "
                    + Entry.ColumnNameWorkflowId + ", "
                    + Entry.ColumnNameJobId
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE (" + Entry.ColumnNameWorkflowId + " = " + workflowId
                    + " AND " + Entry.ColumnNameJobId + " = '" + jobId + "');", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Entry entry = new()
                    {
                        Id = (long)reader[Entry.ColumnNameId],
                        Name = (string)reader[Entry.ColumnNameName],
                        Description = (string)reader[Entry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[Entry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[Entry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[Entry.ColumnNameWorkflowId],
                        JobId = (string)reader[Entry.ColumnNameJobId]
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
                using (SQLiteConnection conn = new(_connectionString))
                {
                    conn.Open();

                    using SQLiteCommand command = new("SELECT " + Entry.ColumnNameStatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " ORDER BY " + Entry.ColumnNameStatusDate + " DESC LIMIT 1;", conn);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]);

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
                using (SQLiteConnection conn = new(_connectionString))
                {
                    conn.Open();

                    using SQLiteCommand command = new("SELECT " + Entry.ColumnNameStatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " ORDER BY " + Entry.ColumnNameStatusDate + " ASC LIMIT 1;", conn);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var statusDate = DateTime.Parse((string)reader[Entry.ColumnNameStatusDate]);

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
                List<HistoryEntry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + HistoryEntry.ColumnNameId + ", "
                                                  + HistoryEntry.ColumnNameName + ", "
                                                  + HistoryEntry.ColumnNameDescription + ", "
                                                  + HistoryEntry.ColumnNameLaunchType + ", "
                                                  + HistoryEntry.ColumnNameStatus + ", "
                                                  + HistoryEntry.ColumnNameStatusDate + ", "
                                                  + HistoryEntry.ColumnNameWorkflowId
                                                  + " FROM " + Core.Db.HistoryEntry.DocumentName + ";", conn);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HistoryEntry entry = new()
                    {
                        Id = (long)reader[HistoryEntry.ColumnNameId],
                        Name = (string)reader[HistoryEntry.ColumnNameName],
                        Description = (string)reader[HistoryEntry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[HistoryEntry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[HistoryEntry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[HistoryEntry.ColumnNameWorkflowId]
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
                List<HistoryEntry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + HistoryEntry.ColumnNameId + ", "
                                                  + HistoryEntry.ColumnNameName + ", "
                                                  + HistoryEntry.ColumnNameDescription + ", "
                                                  + HistoryEntry.ColumnNameLaunchType + ", "
                                                  + HistoryEntry.ColumnNameStatus + ", "
                                                  + HistoryEntry.ColumnNameStatusDate + ", "
                                                  + HistoryEntry.ColumnNameWorkflowId
                                                  + " FROM " + Core.Db.HistoryEntry.DocumentName
                                                  + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%';", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    HistoryEntry entry = new()
                    {
                        Id = (long)reader[HistoryEntry.ColumnNameId],
                        Name = (string)reader[HistoryEntry.ColumnNameName],
                        Description = (string)reader[HistoryEntry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[HistoryEntry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[HistoryEntry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[HistoryEntry.ColumnNameWorkflowId]
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
                List<HistoryEntry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + HistoryEntry.ColumnNameId + ", "
                                                  + HistoryEntry.ColumnNameName + ", "
                                                  + HistoryEntry.ColumnNameDescription + ", "
                                                  + HistoryEntry.ColumnNameLaunchType + ", "
                                                  + HistoryEntry.ColumnNameStatus + ", "
                                                  + HistoryEntry.ColumnNameStatusDate + ", "
                                                  + HistoryEntry.ColumnNameWorkflowId
                                                  + " FROM " + Core.Db.HistoryEntry.DocumentName
                                                  + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " LIMIT " + entriesCount + " OFFSET " + ((page - 1) * entriesCount) + ";"
                    , conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    HistoryEntry entry = new()
                    {
                        Id = (long)reader[HistoryEntry.ColumnNameId],
                        Name = (string)reader[HistoryEntry.ColumnNameName],
                        Description = (string)reader[HistoryEntry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[HistoryEntry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[HistoryEntry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[HistoryEntry.ColumnNameWorkflowId]
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
                List<HistoryEntry> entries = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                StringBuilder sqlBuilder = new("SELECT "
                                               + HistoryEntry.ColumnNameId + ", "
                                               + HistoryEntry.ColumnNameName + ", "
                                               + HistoryEntry.ColumnNameDescription + ", "
                                               + HistoryEntry.ColumnNameLaunchType + ", "
                                               + HistoryEntry.ColumnNameStatus + ", "
                                               + HistoryEntry.ColumnNameStatusDate + ", "
                                               + HistoryEntry.ColumnNameWorkflowId
                                               + " FROM " + Core.Db.HistoryEntry.DocumentName
                                               + " WHERE " + "(LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                               + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                               + " AND (" + HistoryEntry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "')"
                                               + " ORDER BY ");

                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameStatusDate).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameStatusDate).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameWorkflowId).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameWorkflowId).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameName).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameName).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameLaunchType).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameLaunchType).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameDescription).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameDescription).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameStatus).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        _ = sqlBuilder.Append(HistoryEntry.ColumnNameStatus).Append(" DESC");
                        break;
                }

                _ = sqlBuilder.Append(" LIMIT ").Append(entriesCount).Append(" OFFSET ").Append((page - 1) * entriesCount).Append(';');

                using SQLiteCommand command = new(sqlBuilder.ToString(), conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    HistoryEntry entry = new()
                    {
                        Id = (long)reader[HistoryEntry.ColumnNameId],
                        Name = (string)reader[HistoryEntry.ColumnNameName],
                        Description = (string)reader[HistoryEntry.ColumnNameDescription],
                        LaunchType = (LaunchType)(long)reader[HistoryEntry.ColumnNameLaunchType],
                        Status = (Status)(long)reader[HistoryEntry.ColumnNameStatus],
                        StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]),
                        WorkflowId = (int)(long)reader[HistoryEntry.ColumnNameWorkflowId]
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%';", conn);

                var count = (long)command.ExecuteScalar();

                return count;
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "(LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                    + " AND (" + HistoryEntry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "');", conn);

                var count = (long)command.ExecuteScalar();

                return count;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (Padlock)
            {
                using (SQLiteConnection conn = new(_connectionString))
                {
                    conn.Open();

                    using SQLiteCommand command = new("SELECT " + HistoryEntry.ColumnNameStatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " ORDER BY " + HistoryEntry.ColumnNameStatusDate + " DESC LIMIT 1;", conn);

                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var statusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]);

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
                using (SQLiteConnection conn = new(_connectionString))
                {
                    conn.Open();

                    using SQLiteCommand command = new("SELECT " + HistoryEntry.ColumnNameStatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " ORDER BY " + HistoryEntry.ColumnNameStatusDate + " ASC LIMIT 1;", conn);

                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var statusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnNameStatusDate]);

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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNamePassword
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''") + "'"
                    + ";", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var password = (string)reader[User.ColumnNamePassword];

                    return password;
                }

                return null;
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + StatusCount.ColumnNameId + ", "
                    + StatusCount.ColumnNamePendingCount + ", "
                    + StatusCount.ColumnNameRunningCount + ", "
                    + StatusCount.ColumnNameDoneCount + ", "
                    + StatusCount.ColumnNameFailedCount + ", "
                    + StatusCount.ColumnNameWarningCount + ", "
                    + StatusCount.ColumnNameDisabledCount + ", "
                    + StatusCount.ColumnNameStoppedCount + ", "
                    + StatusCount.ColumnNameRejectedCount
                    + " FROM " + Core.Db.StatusCount.DocumentName
                    + ";", conn);
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    StatusCount statusCount = new()
                    {
                        Id = (long)reader[StatusCount.ColumnNameId],
                        PendingCount = (int)(long)reader[StatusCount.ColumnNamePendingCount],
                        RunningCount = (int)(long)reader[StatusCount.ColumnNameRunningCount],
                        DoneCount = (int)(long)reader[StatusCount.ColumnNameDoneCount],
                        FailedCount = (int)(long)reader[StatusCount.ColumnNameFailedCount],
                        WarningCount = (int)(long)reader[StatusCount.ColumnNameWarningCount],
                        DisabledCount = (int)(long)reader[StatusCount.ColumnNameDisabledCount],
                        StoppedCount = (int)(long)reader[StatusCount.ColumnNameStoppedCount],
                        RejectedCount = (int)(long)reader[StatusCount.ColumnNameRejectedCount]
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNameId + ", "
                    + User.ColumnNameUsername + ", "
                    + User.ColumnNamePassword + ", "
                    + User.ColumnNameEmail + ", "
                    + User.ColumnNameUserProfile + ", "
                    + User.ColumnNameCreatedOn + ", "
                    + User.ColumnNameModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''") + "'"
                    + ";", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    User user = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
                    };

                    return user;
                }

                return null;
            }
        }

        public override Core.Db.User GetUserById(string userId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNameId + ", "
                    + User.ColumnNameUsername + ", "
                    + User.ColumnNamePassword + ", "
                    + User.ColumnNameEmail + ", "
                    + User.ColumnNameUserProfile + ", "
                    + User.ColumnNameCreatedOn + ", "
                    + User.ColumnNameModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnNameId + " = '" + int.Parse(userId) + "'"
                    + ";", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    User user = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
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
                List<User> users = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNameId + ", "
                                                  + User.ColumnNameUsername + ", "
                                                  + User.ColumnNamePassword + ", "
                                                  + User.ColumnNameEmail + ", "
                                                  + User.ColumnNameUserProfile + ", "
                                                  + User.ColumnNameCreatedOn + ", "
                                                  + User.ColumnNameModifiedOn
                                                  + " FROM " + Core.Db.User.DocumentName
                                                  + ";", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
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
                List<User> users = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + User.ColumnNameId + ", "
                                                  + User.ColumnNameUsername + ", "
                                                  + User.ColumnNamePassword + ", "
                                                  + User.ColumnNameEmail + ", "
                                                  + User.ColumnNameUserProfile + ", "
                                                  + User.ColumnNameCreatedOn + ", "
                                                  + User.ColumnNameModifiedOn
                                                  + " FROM " + Core.Db.User.DocumentName
                                                  + " WHERE " + "LOWER(" + User.ColumnNameUsername + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " ORDER BY " + User.ColumnNameUsername + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                                                  + ";", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
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
                List<string> workflowIds = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + UserWorkflow.ColumnNameId + ", "
                                                  + UserWorkflow.ColumnNameUserId + ", "
                                                  + UserWorkflow.ColumnNameWorkflowId
                                                  + " FROM " + Core.Db.UserWorkflow.DocumentName
                                                  + " WHERE " + UserWorkflow.ColumnNameUserId + " = " + int.Parse(userId)
                                                  + ";", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var workflowId = (long)reader[UserWorkflow.ColumnNameWorkflowId];

                    workflowIds.Add(workflowId.ToString());
                }

                return workflowIds;
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + Workflow.ColumnNameId + ", "
                    + Workflow.ColumnNameXml
                    + " FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnNameId + " = " + int.Parse(id) + ";", conn);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Workflow workflow = new()
                    {
                        Id = (long)reader[Workflow.ColumnNameId],
                        Xml = (string)reader[Workflow.ColumnNameXml]
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
                List<Core.Db.Workflow> workflows = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + Workflow.ColumnNameId + ", "
                                                  + Workflow.ColumnNameXml
                                                  + " FROM " + Core.Db.Workflow.DocumentName + ";", conn);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Workflow workflow = new()
                    {
                        Id = (long)reader[Workflow.ColumnNameId],
                        Xml = (string)reader[Workflow.ColumnNameXml]
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " + 1;", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void IncrementDisabledCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameDisabledCount);
        }

        public override void IncrementRejectedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameRejectedCount);
        }

        public override void IncrementDoneCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameDoneCount);
        }

        public override void IncrementFailedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameFailedCount);
        }

        public override void IncrementPendingCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNamePendingCount);
        }

        public override void IncrementRunningCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameRunningCount);
        }

        public override void IncrementStoppedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameStoppedCount);
        }

        public override void IncrementWarningCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnNameWarningCount);
        }

        private static void DecrementStatusCountColumn(string statusCountColumnName)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " - 1;", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DecrementPendingCount()
        {
            DecrementStatusCountColumn(StatusCount.ColumnNamePendingCount);
        }

        public override void DecrementRunningCount()
        {
            DecrementStatusCountColumn(StatusCount.ColumnNameRunningCount);
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Entry.DocumentName + "("
                    + Entry.ColumnNameName + ", "
                    + Entry.ColumnNameDescription + ", "
                    + Entry.ColumnNameLaunchType + ", "
                    + Entry.ColumnNameStatusDate + ", "
                    + Entry.ColumnNameStatus + ", "
                    + Entry.ColumnNameWorkflowId + ", "
                    + Entry.ColumnNameJobId + ", "
                    + Entry.ColumnNameLogs + ") VALUES("
                    + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + "'" + (entry.JobId ?? "") + "', "
                    + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ");"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.HistoryEntry.DocumentName + "("
                    + HistoryEntry.ColumnNameName + ", "
                    + HistoryEntry.ColumnNameDescription + ", "
                    + HistoryEntry.ColumnNameLaunchType + ", "
                    + HistoryEntry.ColumnNameStatusDate + ", "
                    + HistoryEntry.ColumnNameStatus + ", "
                    + HistoryEntry.ColumnNameWorkflowId + ", "
                    + HistoryEntry.ColumnNameLogs + ") VALUES("
                    + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ");"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.User.DocumentName + "("
                    + User.ColumnNameUsername + ", "
                    + User.ColumnNamePassword + ", "
                    + User.ColumnNameUserProfile + ", "
                    + User.ColumnNameEmail + ", "
                    + User.ColumnNameCreatedOn + ", "
                    + User.ColumnNameModifiedOn + ") VALUES("
                    + "'" + (user.Username ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (user.Password ?? "").Replace("'", "''") + "'" + ", "
                    + (int)user.UserProfile + ", "
                    + "'" + (user.Email ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", "
                    + (user.ModifiedOn == DateTime.MinValue ? "NULL" : "'" + user.ModifiedOn.ToString(DATE_TIME_FORMAT) + "'") + ");"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.UserWorkflow.DocumentName + "("
                    + UserWorkflow.ColumnNameUserId + ", "
                    + UserWorkflow.ColumnNameWorkflowId + ") VALUES("
                    + int.Parse(userWorkflow.UserId) + ", "
                    + int.Parse(userWorkflow.WorkflowId) + ");"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Workflow.DocumentName + "("
                    + Workflow.ColumnNameXml + ") VALUES("
                    + "'" + (workflow.Xml ?? "").Replace("'", "''") + "'" + "); SELECT last_insert_rowid(); "
                    , conn);

                var id = (long)command.ExecuteScalar();

                return id.ToString();
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.Entry.DocumentName + " SET "
                    + Entry.ColumnNameName + " = '" + (entry.Name ?? "").Replace("'", "''") + "', "
                    + Entry.ColumnNameDescription + " = '" + (entry.Description ?? "").Replace("'", "''") + "', "
                    + Entry.ColumnNameLaunchType + " = " + (int)entry.LaunchType + ", "
                    + Entry.ColumnNameStatusDate + " = '" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "', "
                    + Entry.ColumnNameStatus + " = " + (int)entry.Status + ", "
                    + Entry.ColumnNameWorkflowId + " = " + entry.WorkflowId + ", "
                    + Entry.ColumnNameJobId + " = '" + (entry.JobId ?? "") + "', "
                    + Entry.ColumnNameLogs + " = '" + (entry.Logs ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + Entry.ColumnNameId + " = " + int.Parse(id) + ";"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnNamePassword + " = '" + (password ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''") + "';"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnNameUsername + " = '" + (user.Username ?? "").Replace("'", "''") + "', "
                    + User.ColumnNamePassword + " = '" + (user.Password ?? "").Replace("'", "''") + "', "
                    + User.ColumnNameUserProfile + " = " + (int)user.UserProfile + ", "
                    + User.ColumnNameEmail + " = '" + (user.Email ?? "").Replace("'", "''") + "', "
                    + User.ColumnNameCreatedOn + " = '" + user.CreatedOn.ToString(DATE_TIME_FORMAT) + "', "
                    + User.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'"
                    + " WHERE "
                    + User.ColumnNameId + " = " + int.Parse(id) + ";"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''") + "', "
                    + User.ColumnNameUserProfile + " = " + (int)up + ", "
                    + User.ColumnNameEmail + " = '" + (email ?? "").Replace("'", "''") + "', "
                    + User.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'"
                    + " WHERE "
                    + User.ColumnNameId + " = " + int.Parse(userId) + ";"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.Workflow.DocumentName + " SET "
                    + Workflow.ColumnNameXml + " = '" + (workflow.Xml ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + User.ColumnNameId + " = " + int.Parse(dbId) + ";"
                    , conn);

                _ = command.ExecuteNonQuery();
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + Entry.ColumnNameLogs
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE "
                    + Entry.ColumnNameId + " = " + int.Parse(entryId) + ";"
                    , conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var logs = (string)reader[Entry.ColumnNameLogs];
                    return logs;
                }

                return null;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT " + HistoryEntry.ColumnNameLogs
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE "
                    + HistoryEntry.ColumnNameId + " = " + int.Parse(entryId) + ";"
                    , conn);
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var logs = (string)reader[HistoryEntry.ColumnNameLogs];
                    return logs;
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (Padlock)
            {
                List<User> users = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + User.ColumnNameId + ", "
                                                  + User.ColumnNameUsername + ", "
                                                  + User.ColumnNamePassword + ", "
                                                  + User.ColumnNameEmail + ", "
                                                  + User.ColumnNameUserProfile + ", "
                                                  + User.ColumnNameCreatedOn + ", "
                                                  + User.ColumnNameModifiedOn
                                                  + " FROM " + Core.Db.User.DocumentName
                                                  + " WHERE (" + User.ColumnNameUserProfile + " = " + (int)UserProfile.SuperAdministrator
                                                  + " OR " + User.ColumnNameUserProfile + " = " + (int)UserProfile.Administrator + ")"
                                                  + " ORDER BY " + User.ColumnNameUsername
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User admin = new()
                    {
                        Id = (long)reader[User.ColumnNameId],
                        Username = (string)reader[User.ColumnNameUsername],
                        Password = (string)reader[User.ColumnNamePassword],
                        Email = (string)reader[User.ColumnNameEmail],
                        UserProfile = (UserProfile)(long)reader[User.ColumnNameUserProfile],
                        CreatedOn = DateTime.Parse((string)reader[User.ColumnNameCreatedOn]),
                        ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnNameModifiedOn])
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Record.DocumentName + "("
                    + Record.ColumnNameName + ", "
                    + Record.ColumnNameDescription + ", "
                    + Record.ColumnNameApproved + ", "
                    + Record.ColumnNameStartDate + ", "
                    + Record.ColumnNameEndDate + ", "
                    + Record.ColumnNameComments + ", "
                    + Record.ColumnNameManagerComments + ", "
                    + Record.ColumnNameCreatedBy + ", "
                    + Record.ColumnNameCreatedOn + ", "
                    + Record.ColumnNameModifiedBy + ", "
                    + Record.ColumnNameModifiedOn + ", "
                    + Record.ColumnNameAssignedTo + ", "
                    + Record.ColumnNameAssignedOn + ")"
                    + " VALUES("
                    + "'" + (record.Name ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (record.Description ?? "").Replace("'", "''") + "'" + ", "
                    + (record.Approved ? "1" : "0") + ", "
                    + (record.StartDate == null ? "NULL" : "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                    + (record.EndDate == null ? "NULL" : "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                    + "'" + (record.Comments ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + (record.ManagerComments ?? "").Replace("'", "''") + "'" + ", "
                    + int.Parse(record.CreatedBy) + ", "
                    + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", "
                    + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                    + (record.ModifiedOn == null ? "NULL" : "'" + record.ModifiedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                     + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                    + (record.AssignedOn == null ? "NULL" : "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ")"
                    + ";"
                    + " SELECT last_insert_rowid();"
                    , conn);
                var id = (long)command.ExecuteScalar();
                return id.ToString();
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.Record.DocumentName + " SET "
                    + Record.ColumnNameName + " = '" + (record.Name ?? "").Replace("'", "''") + "', "
                    + Record.ColumnNameDescription + " = '" + (record.Description ?? "").Replace("'", "''") + "', "
                    + Record.ColumnNameApproved + " = " + (record.Approved ? "1" : "0") + ", "
                    + Record.ColumnNameStartDate + " = " + (record.StartDate == null ? "NULL" : "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                    + Record.ColumnNameEndDate + " = " + (record.EndDate == null ? "NULL" : "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                    + Record.ColumnNameComments + " = '" + (record.Comments ?? "").Replace("'", "''") + "', "
                    + Record.ColumnNameManagerComments + " = '" + (record.ManagerComments ?? "").Replace("'", "''") + "', "
                    + Record.ColumnNameCreatedBy + " = " + int.Parse(record.CreatedBy) + ", "
                    + Record.ColumnNameModifiedBy + " = " + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                    + Record.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "', "
                    + Record.ColumnNameAssignedTo + " = " + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                    + Record.ColumnNameAssignedOn + " = " + (record.AssignedOn == null ? "NULL" : "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'")
                    + " WHERE "
                    + Record.ColumnNameId + " = " + int.Parse(recordId) + ";"
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
                    using SQLiteConnection conn = new(_connectionString);
                    conn.Open();

                    StringBuilder builder = new("(");

                    for (var i = 0; i < recordIds.Length; i++)
                    {
                        var id = recordIds[i];
                        _ = builder.Append(id);
                        _ = i < recordIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using SQLiteCommand command = new("DELETE FROM " + Core.Db.Record.DocumentName
                        + " WHERE " + Record.ColumnNameId + " IN " + builder + ";", conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                    + Record.ColumnNameId + ", "
                    + Record.ColumnNameName + ", "
                    + Record.ColumnNameDescription + ", "
                    + Record.ColumnNameApproved + ", "
                    + Record.ColumnNameStartDate + ", "
                    + Record.ColumnNameEndDate + ", "
                    + Record.ColumnNameComments + ", "
                    + Record.ColumnNameManagerComments + ", "
                    + Record.ColumnNameCreatedBy + ", "
                    + Record.ColumnNameCreatedOn + ", "
                    + Record.ColumnNameModifiedBy + ", "
                    + Record.ColumnNameModifiedOn + ", "
                    + Record.ColumnNameAssignedTo + ", "
                    + Record.ColumnNameAssignedOn
                    + " FROM " + Core.Db.Record.DocumentName
                    + " WHERE " + Record.ColumnNameId + " = " + int.Parse(id)
                    + ";", conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Record record = new()
                    {
                        Id = (long)reader[Record.ColumnNameId],
                        Name = (string)reader[Record.ColumnNameName],
                        Description = (string)reader[Record.ColumnNameDescription],
                        Approved = (long)reader[Record.ColumnNameApproved] == 1,
                        StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameStartDate]),
                        EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameEndDate]),
                        Comments = (string)reader[Record.ColumnNameComments],
                        ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                        CreatedBy = ((long)reader[Record.ColumnNameCreatedBy]).ToString(),
                        CreatedOn = DateTime.Parse((string)reader[Record.ColumnNameCreatedOn]),
                        ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameModifiedBy]).ToString(),
                        ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameModifiedOn]),
                        AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameAssignedTo]).ToString(),
                        AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameAssignedOn])
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
                List<Record> records = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Record.ColumnNameId + ", "
                                                  + Record.ColumnNameName + ", "
                                                  + Record.ColumnNameDescription + ", "
                                                  + Record.ColumnNameApproved + ", "
                                                  + Record.ColumnNameStartDate + ", "
                                                  + Record.ColumnNameEndDate + ", "
                                                  + Record.ColumnNameComments + ", "
                                                  + Record.ColumnNameManagerComments + ", "
                                                  + Record.ColumnNameCreatedBy + ", "
                                                  + Record.ColumnNameCreatedOn + ", "
                                                  + Record.ColumnNameModifiedBy + ", "
                                                  + Record.ColumnNameModifiedOn + ", "
                                                  + Record.ColumnNameAssignedTo + ", "
                                                  + Record.ColumnNameAssignedOn
                                                  + " FROM " + Core.Db.Record.DocumentName
                                                  + " WHERE " + "LOWER(" + Record.ColumnNameName + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " OR " + "LOWER(" + Record.ColumnNameDescription + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " ORDER BY " + Record.ColumnNameCreatedOn + " DESC"
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Record record = new()
                    {
                        Id = (long)reader[Record.ColumnNameId],
                        Name = (string)reader[Record.ColumnNameName],
                        Description = (string)reader[Record.ColumnNameDescription],
                        Approved = (long)reader[Record.ColumnNameApproved] == 1,
                        StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameStartDate]),
                        EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameEndDate]),
                        Comments = (string)reader[Record.ColumnNameComments],
                        ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                        CreatedBy = ((long)reader[Record.ColumnNameCreatedBy]).ToString(),
                        CreatedOn = DateTime.Parse((string)reader[Record.ColumnNameCreatedOn]),
                        ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameModifiedBy]).ToString(),
                        ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameModifiedOn]),
                        AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameAssignedTo]).ToString(),
                        AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameAssignedOn])
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
                List<Record> records = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Record.ColumnNameId + ", "
                                                  + Record.ColumnNameName + ", "
                                                  + Record.ColumnNameDescription + ", "
                                                  + Record.ColumnNameApproved + ", "
                                                  + Record.ColumnNameStartDate + ", "
                                                  + Record.ColumnNameEndDate + ", "
                                                  + Record.ColumnNameComments + ", "
                                                  + Record.ColumnNameManagerComments + ", "
                                                  + Record.ColumnNameCreatedBy + ", "
                                                  + Record.ColumnNameCreatedOn + ", "
                                                  + Record.ColumnNameModifiedBy + ", "
                                                  + Record.ColumnNameModifiedOn + ", "
                                                  + Record.ColumnNameAssignedTo + ", "
                                                  + Record.ColumnNameAssignedOn
                                                  + " FROM " + Core.Db.Record.DocumentName
                                                  + " WHERE " + Record.ColumnNameCreatedBy + " = " + int.Parse(createdBy)
                                                  + " ORDER BY " + Record.ColumnNameName + " ASC"
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Record record = new()
                    {
                        Id = (long)reader[Record.ColumnNameId],
                        Name = (string)reader[Record.ColumnNameName],
                        Description = (string)reader[Record.ColumnNameDescription],
                        Approved = (long)reader[Record.ColumnNameApproved] == 1,
                        StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameStartDate]),
                        EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameEndDate]),
                        Comments = (string)reader[Record.ColumnNameComments],
                        ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                        CreatedBy = ((long)reader[Record.ColumnNameCreatedBy]).ToString(),
                        CreatedOn = DateTime.Parse((string)reader[Record.ColumnNameCreatedOn]),
                        ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameModifiedBy]).ToString(),
                        ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameModifiedOn]),
                        AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameAssignedTo]).ToString(),
                        AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameAssignedOn])
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
                List<Record> records = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Record.ColumnNameId + ", "
                                                  + Record.ColumnNameName + ", "
                                                  + Record.ColumnNameDescription + ", "
                                                  + Record.ColumnNameApproved + ", "
                                                  + Record.ColumnNameStartDate + ", "
                                                  + Record.ColumnNameEndDate + ", "
                                                  + Record.ColumnNameComments + ", "
                                                  + Record.ColumnNameManagerComments + ", "
                                                  + Record.ColumnNameCreatedBy + ", "
                                                  + Record.ColumnNameCreatedOn + ", "
                                                  + Record.ColumnNameModifiedBy + ", "
                                                  + Record.ColumnNameModifiedOn + ", "
                                                  + Record.ColumnNameAssignedTo + ", "
                                                  + Record.ColumnNameAssignedOn
                                                  + " FROM " + Core.Db.Record.DocumentName
                                                  + " WHERE " + "(LOWER(" + Record.ColumnNameName + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " OR " + "LOWER(" + Record.ColumnNameDescription + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                                                  + " AND (" + Record.ColumnNameCreatedBy + " = " + int.Parse(createdBy) + " OR " + Record.ColumnNameAssignedTo + " = " + int.Parse(assingedTo) + ")"
                                                  + " ORDER BY " + Record.ColumnNameCreatedOn + " DESC"
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Record record = new()
                    {
                        Id = (long)reader[Record.ColumnNameId],
                        Name = (string)reader[Record.ColumnNameName],
                        Description = (string)reader[Record.ColumnNameDescription],
                        Approved = (long)reader[Record.ColumnNameApproved] == 1,
                        StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameStartDate]),
                        EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameEndDate]),
                        Comments = (string)reader[Record.ColumnNameComments],
                        ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                        CreatedBy = ((long)reader[Record.ColumnNameCreatedBy]).ToString(),
                        CreatedOn = DateTime.Parse((string)reader[Record.ColumnNameCreatedOn]),
                        ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameModifiedBy]).ToString(),
                        ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameModifiedOn]),
                        AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((long)reader[Record.ColumnNameAssignedTo]).ToString(),
                        AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Record.ColumnNameAssignedOn])
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Version.DocumentName + "("
                    + Version.ColumnNameRecordId + ", "
                    + Version.ColumnNameFilePath + ", "
                    + Version.ColumnNameCreatedOn + ")"
                    + " VALUES("
                    + int.Parse(version.RecordId) + ", "
                    + "'" + (version.FilePath ?? "").Replace("'", "''") + "'" + ", "
                    + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ")"
                    + ";"
                    + " SELECT last_insert_rowid();"
                    , conn);
                var id = (long)command.ExecuteScalar();
                return id.ToString();
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.Version.DocumentName + " SET "
                    + Version.ColumnNameRecordId + " = " + int.Parse(version.RecordId) + ", "
                    + Version.ColumnNameFilePath + " = '" + (version.FilePath ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + Version.ColumnNameId + " = " + int.Parse(versionId) + ";"
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
                    using SQLiteConnection conn = new(_connectionString);
                    conn.Open();

                    StringBuilder builder = new("(");

                    for (var i = 0; i < versionIds.Length; i++)
                    {
                        var id = versionIds[i];
                        _ = builder.Append(id);
                        _ = i < versionIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using SQLiteCommand command = new("DELETE FROM " + Core.Db.Version.DocumentName
                        + " WHERE " + Version.ColumnNameId + " IN " + builder + ";", conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (Padlock)
            {
                List<Version> versions = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Version.ColumnNameId + ", "
                                                  + Version.ColumnNameRecordId + ", "
                                                  + Version.ColumnNameFilePath + ", "
                                                  + Version.ColumnNameCreatedOn
                                                  + " FROM " + Core.Db.Version.DocumentName
                                                  + " WHERE " + Version.ColumnNameRecordId + " = " + int.Parse(recordId)
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Version version = new()
                    {
                        Id = (long)reader[Version.ColumnNameId],
                        RecordId = ((long)reader[Version.ColumnNameRecordId]).ToString(),
                        FilePath = (string)reader[Version.ColumnNameFilePath],
                        CreatedOn = DateTime.Parse((string)reader[Version.ColumnNameCreatedOn])
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                    + Version.ColumnNameId + ", "
                    + Version.ColumnNameRecordId + ", "
                    + Version.ColumnNameFilePath + ", "
                    + Version.ColumnNameCreatedOn
                    + " FROM " + Core.Db.Version.DocumentName
                    + " WHERE " + Version.ColumnNameRecordId + " = " + int.Parse(recordId)
                    + " ORDER BY " + Version.ColumnNameCreatedOn + " DESC"
                    + " LIMIT 1"
                    + ";", conn);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Version version = new()
                    {
                        Id = (long)reader[Version.ColumnNameId],
                        RecordId = ((long)reader[Version.ColumnNameRecordId]).ToString(),
                        FilePath = (string)reader[Version.ColumnNameFilePath],
                        CreatedOn = DateTime.Parse((string)reader[Version.ColumnNameCreatedOn])
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Notification.DocumentName + "("
                    + Notification.ColumnNameAssignedBy + ", "
                    + Notification.ColumnNameAssignedOn + ", "
                    + Notification.ColumnNameAssignedTo + ", "
                    + Notification.ColumnNameMessage + ", "
                    + Notification.ColumnNameIsRead + ")"
                    + " VALUES("
                    + (!string.IsNullOrEmpty(notification.AssignedBy) ? int.Parse(notification.AssignedBy).ToString() : "NULL") + ", "
                    + "'" + notification.AssignedOn.ToString(DATE_TIME_FORMAT) + "'" + ", "
                    + (!string.IsNullOrEmpty(notification.AssignedTo) ? int.Parse(notification.AssignedTo).ToString() : "NULL") + ", "
                    + "'" + (notification.Message ?? "").Replace("'", "''") + "'" + ", "
                    + (notification.IsRead ? "1" : "0") + ")"
                    + ";"
                    + " SELECT last_insert_rowid();"
                    , conn);
                var id = (long)command.ExecuteScalar();
                return id.ToString();
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                StringBuilder builder = new("(");

                for (var i = 0; i < notificationIds.Length; i++)
                {
                    var id = notificationIds[i];
                    _ = builder.Append(id);
                    _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using SQLiteCommand command = new("UPDATE " + Core.Db.Notification.DocumentName
                    + " SET " + Notification.ColumnNameIsRead + " = " + "1"
                    + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                StringBuilder builder = new("(");

                for (var i = 0; i < notificationIds.Length; i++)
                {
                    var id = notificationIds[i];
                    _ = builder.Append(id);
                    _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                }

                using SQLiteCommand command = new("UPDATE " + Core.Db.Notification.DocumentName
                    + " SET " + Notification.ColumnNameIsRead + " = " + "0"
                    + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (Padlock)
            {
                if (notificationIds.Length > 0)
                {
                    using SQLiteConnection conn = new(_connectionString);
                    conn.Open();

                    StringBuilder builder = new("(");

                    for (var i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        _ = builder.Append(id);
                        _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using SQLiteCommand command = new("DELETE FROM " + Core.Db.Notification.DocumentName
                        + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn);
                    _ = command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (Padlock)
            {
                List<Notification> notifications = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Notification.ColumnNameId + ", "
                                                  + Notification.ColumnNameAssignedBy + ", "
                                                  + Notification.ColumnNameAssignedOn + ", "
                                                  + Notification.ColumnNameAssignedTo + ", "
                                                  + Notification.ColumnNameMessage + ", "
                                                  + Notification.ColumnNameIsRead
                                                  + " FROM " + Core.Db.Notification.DocumentName
                                                  + " WHERE " + "(LOWER(" + Notification.ColumnNameMessage + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                                                  + " AND " + Notification.ColumnNameAssignedTo + " = " + int.Parse(assignedTo) + ")"
                                                  + " ORDER BY " + Notification.ColumnNameAssignedOn + " DESC"
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Notification notification = new()
                    {
                        Id = (long)reader[Notification.ColumnNameId],
                        AssignedBy = ((long)reader[Notification.ColumnNameAssignedBy]).ToString(),
                        AssignedOn = DateTime.Parse((string)reader[Notification.ColumnNameAssignedOn]),
                        AssignedTo = ((long)reader[Notification.ColumnNameAssignedTo]).ToString(),
                        Message = (string)reader[Notification.ColumnNameMessage],
                        IsRead = (long)reader[Notification.ColumnNameIsRead] == 1
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
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT COUNT(*)"
                    + " FROM " + Core.Db.Notification.DocumentName
                    + " WHERE (" + Notification.ColumnNameAssignedTo + " = " + int.Parse(assignedTo)
                    + " AND " + Notification.ColumnNameIsRead + " = " + "0" + ")"
                    + ";", conn);
                var count = (long)command.ExecuteScalar();
                var hasNotifications = count > 0;
                return hasNotifications;
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("INSERT INTO " + Core.Db.Approver.DocumentName + "("
                    + Approver.ColumnNameUserId + ", "
                    + Approver.ColumnNameRecordId + ", "
                    + Approver.ColumnNameApproved + ", "
                    + Approver.ColumnNameApprovedOn + ") VALUES("
                    + int.Parse(approver.UserId) + ", "
                    + int.Parse(approver.RecordId) + ", "
                    + (approver.Approved ? "1" : "0") + ", "
                    + (approver.ApprovedOn == null ? "NULL" : "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ");"
                    + " SELECT last_insert_rowid();"
                    , conn);
                var id = (long)command.ExecuteScalar();
                return id.ToString();
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("UPDATE " + Core.Db.Approver.DocumentName + " SET "
                    + Approver.ColumnNameUserId + " = " + int.Parse(approver.UserId) + ", "
                    + Approver.ColumnNameRecordId + " = " + int.Parse(approver.RecordId) + ", "
                    + Approver.ColumnNameApproved + " = " + (approver.Approved ? "1" : "0") + ", "
                    + Approver.ColumnNameApprovedOn + " = " + (approver.ApprovedOn == null ? "NULL" : "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'")
                    + " WHERE "
                    + Approver.ColumnNameId + " = " + int.Parse(approverId) + ";"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Approver.DocumentName
                    + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId) + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Approver.DocumentName
                    + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId)
                    + " AND " + Approver.ColumnNameApproved + " = " + "1"
                    + ";"
                    , conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (Padlock)
            {
                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("DELETE FROM " + Core.Db.Approver.DocumentName
                    + " WHERE " + Approver.ColumnNameUserId + " = " + int.Parse(userId) + ";", conn);
                _ = command.ExecuteNonQuery();
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (Padlock)
            {
                List<Approver> approvers = new();

                using SQLiteConnection conn = new(_connectionString);
                conn.Open();

                using SQLiteCommand command = new("SELECT "
                                                  + Approver.ColumnNameId + ", "
                                                  + Approver.ColumnNameUserId + ", "
                                                  + Approver.ColumnNameRecordId + ", "
                                                  + Approver.ColumnNameApproved + ", "
                                                  + Approver.ColumnNameApprovedOn
                                                  + " FROM " + Core.Db.Approver.DocumentName
                                                  + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId)
                                                  + ";", conn);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Approver approver = new()
                    {
                        Id = (long)reader[Approver.ColumnNameId],
                        UserId = ((long)reader[Approver.ColumnNameUserId]).ToString(),
                        RecordId = ((long)reader[Approver.ColumnNameRecordId]).ToString(),
                        Approved = (long)reader[Approver.ColumnNameApproved] == 1,
                        ApprovedOn = reader[Approver.ColumnNameApprovedOn] == DBNull.Value ? null : DateTime.Parse((string)reader[Approver.ColumnNameApprovedOn])
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
