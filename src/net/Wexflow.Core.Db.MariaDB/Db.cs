using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wexflow.Core.Db.MariaDB
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object Padlock = new object();
        private const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        private static string _connectionString;

        public Db(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;

            var server = string.Empty;
            var port = 3306;
            var user = string.Empty;
            var password = string.Empty;
            var database = string.Empty;

            var connectionStringParts = Db._connectionString.Split(';');

            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    var connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("SERVER="))
                    {
                        server = connPart.Replace("SERVER=", string.Empty);
                    }
                    if (connPart.StartsWith("PORT="))
                    {
                        port = int.Parse(connPart.Replace("PORT=", string.Empty));
                    }
                    else if (connPart.StartsWith("USER="))
                    {
                        user = connPart.Replace("USER=", string.Empty);
                    }
                    else if (connPart.StartsWith("PASSWORD="))
                    {
                        password = connPart.Replace("PASSWORD=", string.Empty);
                    }
                    else if (connPart.StartsWith("DATABASE="))
                    {
                        database = connPart.Replace("DATABASE=", string.Empty);
                    }
                }
            }

            var helper = new Helper(Db._connectionString);
            Helper.CreateDatabaseIfNotExists(server, port, user, password, database);
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

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new MySqlCommand("INSERT INTO " + Core.Db.StatusCount.DocumentName + "("
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
                    , conn))
                {
                    _ = command.ExecuteNonQuery();
                }
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new MySqlCommand("SELECT COUNT(*) FROM " + Core.Db.User.DocumentName + ";", conn))
                {
                    var usersCount = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                    if (usersCount == 0)
                    {
                        InsertDefaultUser();
                    }
                }
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnNameUserId + "=" + int.Parse(userId)
                        + " AND " + UserWorkflow.ColumnNameWorkflowId + "=" + int.Parse(workflowId)
                        + ";", conn))
                    {
                        var count = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                        return count > 0;
                    }
                }
            }
        }

        public override void ClearEntries()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Entry.DocumentName + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void ClearStatusCount()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnNameUsername + " = '" + username + "'"
                        + " AND " + User.ColumnNamePassword + " = '" + password + "'"
                        + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnNameUserId + " = " + int.Parse(userId) + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnNameWorkflowId + " = " + int.Parse(workflowDbId) + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnNameId + " = " + int.Parse(id) + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < ids.Length; i++)
                    {
                        var id = ids[i];
                        _ = builder.Append(id);
                        _ = i < ids.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnNameId + " IN " + builder + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (Padlock)
            {
                var admins = new List<User>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNameId + ", "
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + "(LOWER(" + User.ColumnNameUsername + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " AND " + User.ColumnNameUserProfile + " = " + (int)UserProfile.Administrator + ")"
                        + " ORDER BY " + User.ColumnNameUsername + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var admin = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                admins.Add(admin);
                            }
                        }
                    }

                    return admins;
                }
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            lock (Padlock)
            {
                var entries = new List<Entry>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + Entry.ColumnNameId + ", "
                        + Entry.ColumnNameName + ", "
                        + Entry.ColumnNameDescription + ", "
                        + Entry.ColumnNameLaunchType + ", "
                        + Entry.ColumnNameStatus + ", "
                        + Entry.ColumnNameStatusDate + ", "
                        + Entry.ColumnNameWorkflowId + ", "
                        + Entry.ColumnNameJobId
                        + " FROM " + Core.Db.Entry.DocumentName + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = (int)reader[Entry.ColumnNameId],
                                    Name = (string)reader[Entry.ColumnNameName],
                                    Description = (string)reader[Entry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[Entry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[Entry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[Entry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[Entry.ColumnNameWorkflowId],
                                    JobId = (string)reader[Entry.ColumnNameJobId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            lock (Padlock)
            {
                var entries = new List<Entry>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    var sqlBuilder = new StringBuilder("SELECT "
                        + Entry.ColumnNameId + ", "
                        + Entry.ColumnNameName + ", "
                        + Entry.ColumnNameDescription + ", "
                        + Entry.ColumnNameLaunchType + ", "
                        + Entry.ColumnNameStatus + ", "
                        + Entry.ColumnNameStatusDate + ", "
                        + Entry.ColumnNameWorkflowId + ", "
                        + Entry.ColumnNameJobId
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + "(LOWER(" + Entry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + Entry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%')"
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

                    using (var command = new MySqlCommand(sqlBuilder.ToString(), conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = (int)reader[Entry.ColumnNameId],
                                    Name = (string)reader[Entry.ColumnNameName],
                                    Description = (string)reader[Entry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[Entry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[Entry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[Entry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[Entry.ColumnNameWorkflowId],
                                    JobId = (string)reader[Entry.ColumnNameJobId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + "(LOWER(" + Entry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + Entry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%')"
                        + " AND (" + Entry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "');", conn))
                    {
                        var count = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                        return count;
                    }
                }
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + Entry.ColumnNameId + ", "
                        + Entry.ColumnNameName + ", "
                        + Entry.ColumnNameDescription + ", "
                        + Entry.ColumnNameLaunchType + ", "
                        + Entry.ColumnNameStatus + ", "
                        + Entry.ColumnNameStatusDate + ", "
                        + Entry.ColumnNameWorkflowId + ", "
                        + Entry.ColumnNameJobId
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + Entry.ColumnNameWorkflowId + " = " + workflowId + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = (int)reader[Entry.ColumnNameId],
                                    Name = (string)reader[Entry.ColumnNameName],
                                    Description = (string)reader[Entry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[Entry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[Entry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[Entry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[Entry.ColumnNameWorkflowId],
                                    JobId = (string)reader[Entry.ColumnNameJobId]
                                };

                                return entry;
                            }
                        }
                    }

                    return null;
                }
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId, Guid jobId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + " AND " + Entry.ColumnNameJobId + " = '" + jobId + "');", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = (int)reader[Entry.ColumnNameId],
                                    Name = (string)reader[Entry.ColumnNameName],
                                    Description = (string)reader[Entry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[Entry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[Entry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[Entry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[Entry.ColumnNameWorkflowId],
                                    JobId = (string)reader[Entry.ColumnNameJobId]
                                };

                                return entry;
                            }
                        }
                    }

                    return null;
                }
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + Entry.ColumnNameStatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " ORDER BY " + Entry.ColumnNameStatusDate + " DESC LIMIT 1;", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[Entry.ColumnNameStatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + Entry.ColumnNameStatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " ORDER BY " + Entry.ColumnNameStatusDate + " ASC LIMIT 1;", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[Entry.ColumnNameStatusDate];

                                return statusDate;
                            }
                        }
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

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + HistoryEntry.ColumnNameId + ", "
                        + HistoryEntry.ColumnNameName + ", "
                        + HistoryEntry.ColumnNameDescription + ", "
                        + HistoryEntry.ColumnNameLaunchType + ", "
                        + HistoryEntry.ColumnNameStatus + ", "
                        + HistoryEntry.ColumnNameStatusDate + ", "
                        + HistoryEntry.ColumnNameWorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = (int)reader[HistoryEntry.ColumnNameId],
                                    Name = (string)reader[HistoryEntry.ColumnNameName],
                                    Description = (string)reader[HistoryEntry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[HistoryEntry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[HistoryEntry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[HistoryEntry.ColumnNameWorkflowId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + HistoryEntry.ColumnNameId + ", "
                        + HistoryEntry.ColumnNameName + ", "
                        + HistoryEntry.ColumnNameDescription + ", "
                        + HistoryEntry.ColumnNameLaunchType + ", "
                        + HistoryEntry.ColumnNameStatus + ", "
                        + HistoryEntry.ColumnNameStatusDate + ", "
                        + HistoryEntry.ColumnNameWorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%';", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = (int)reader[HistoryEntry.ColumnNameId],
                                    Name = (string)reader[HistoryEntry.ColumnNameName],
                                    Description = (string)reader[HistoryEntry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[HistoryEntry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[HistoryEntry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[HistoryEntry.ColumnNameWorkflowId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + HistoryEntry.ColumnNameId + ", "
                        + HistoryEntry.ColumnNameName + ", "
                        + HistoryEntry.ColumnNameDescription + ", "
                        + HistoryEntry.ColumnNameLaunchType + ", "
                        + HistoryEntry.ColumnNameStatus + ", "
                        + HistoryEntry.ColumnNameStatusDate + ", "
                        + HistoryEntry.ColumnNameWorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " LIMIT " + entriesCount + " OFFSET " + ((page - 1) * entriesCount) + ";"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = (int)reader[HistoryEntry.ColumnNameId],
                                    Name = (string)reader[HistoryEntry.ColumnNameName],
                                    Description = (string)reader[HistoryEntry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[HistoryEntry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[HistoryEntry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[HistoryEntry.ColumnNameWorkflowId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            lock (Padlock)
            {
                var entries = new List<HistoryEntry>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    var sqlBuilder = new StringBuilder("SELECT "
                        + HistoryEntry.ColumnNameId + ", "
                        + HistoryEntry.ColumnNameName + ", "
                        + HistoryEntry.ColumnNameDescription + ", "
                        + HistoryEntry.ColumnNameLaunchType + ", "
                        + HistoryEntry.ColumnNameStatus + ", "
                        + HistoryEntry.ColumnNameStatusDate + ", "
                        + HistoryEntry.ColumnNameWorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "(LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%')"
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

                    using (var command = new MySqlCommand(sqlBuilder.ToString(), conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = (int)reader[HistoryEntry.ColumnNameId],
                                    Name = (string)reader[HistoryEntry.ColumnNameName],
                                    Description = (string)reader[HistoryEntry.ColumnNameDescription],
                                    LaunchType = (LaunchType)(int)reader[HistoryEntry.ColumnNameLaunchType],
                                    Status = (Status)(int)reader[HistoryEntry.ColumnNameStatus],
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate],
                                    WorkflowId = (int)reader[HistoryEntry.ColumnNameWorkflowId]
                                };

                                entries.Add(entry);
                            }
                        }

                        return entries;
                    }
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%';", conn))
                    {
                        var count = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                        return count;
                    }
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "(LOWER(" + HistoryEntry.ColumnNameName + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnNameDescription + ") LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%')"
                        + " AND (" + HistoryEntry.ColumnNameStatusDate + " BETWEEN '" + from.ToString(DATE_TIME_FORMAT) + "' AND '" + to.ToString(DATE_TIME_FORMAT) + "');", conn))
                    {
                        var count = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                        return count;
                    }
                }
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + HistoryEntry.ColumnNameStatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " ORDER BY " + HistoryEntry.ColumnNameStatusDate + " DESC LIMIT 1;", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + HistoryEntry.ColumnNameStatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " ORDER BY " + HistoryEntry.ColumnNameStatusDate + " ASC LIMIT 1;", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[HistoryEntry.ColumnNameStatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override string GetPassword(string username)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNamePassword
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnNameUsername + " = '" + username + "'"
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var password = (string)reader[User.ColumnNamePassword];

                                return password;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + StatusCount.ColumnNameId + ", "
                        + StatusCount.ColumnNamePendingCount + ", "
                        + StatusCount.ColumnNameRunningCount + ", "
                        + StatusCount.ColumnNameDoneCount + ", "
                        + StatusCount.ColumnNameFailedCount + ", "
                        + StatusCount.ColumnNameWarningCount + ", "
                        + StatusCount.ColumnNameDisabledCount + ", "
                        + StatusCount.ColumnNameStoppedCount + ", "
                        + StatusCount.ColumnNameRejectedCount
                        + " FROM " + Core.Db.StatusCount.DocumentName
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusCount = new StatusCount
                                {
                                    Id = (int)reader[StatusCount.ColumnNameId],
                                    PendingCount = (int)reader[StatusCount.ColumnNamePendingCount],
                                    RunningCount = (int)reader[StatusCount.ColumnNameRunningCount],
                                    DoneCount = (int)reader[StatusCount.ColumnNameDoneCount],
                                    FailedCount = (int)reader[StatusCount.ColumnNameFailedCount],
                                    WarningCount = (int)reader[StatusCount.ColumnNameWarningCount],
                                    DisabledCount = (int)reader[StatusCount.ColumnNameDisabledCount],
                                    StoppedCount = (int)reader[StatusCount.ColumnNameStoppedCount],
                                    RejectedCount = (int)reader[StatusCount.ColumnNameRejectedCount]
                                };

                                return statusCount;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNameId + ", "
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'"
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                return user;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.User GetUserById(string userId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNameId + ", "
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnNameId + " = '" + int.Parse(userId) + "'"
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                return user;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNameId + ", "
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                users.Add(user);
                            }
                        }
                    }

                    return users;
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + User.ColumnNameId + ", "
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + "LOWER(" + User.ColumnNameUsername + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").Replace("\\", "\\\\").ToLower() + "%'"
                        + " ORDER BY " + User.ColumnNameUsername + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                users.Add(user);
                            }
                        }
                    }
                }

                return users;
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            lock (Padlock)
            {
                var workflowIds = new List<string>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + UserWorkflow.ColumnNameId + ", "
                        + UserWorkflow.ColumnNameUserId + ", "
                        + UserWorkflow.ColumnNameWorkflowId
                        + " FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnNameUserId + " = " + int.Parse(userId)
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var workflowId = (int)reader[UserWorkflow.ColumnNameWorkflowId];

                                workflowIds.Add(workflowId.ToString());
                            }
                        }
                    }
                }

                return workflowIds;
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + Workflow.ColumnNameId + ", "
                        + Workflow.ColumnNameXml
                        + " FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnNameId + " = " + int.Parse(id) + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var workflow = new Workflow
                                {
                                    Id = (int)reader[Workflow.ColumnNameId],
                                    Xml = (string)reader[Workflow.ColumnNameXml]
                                };

                                return workflow;
                            }
                        }
                    }

                    return null;
                }
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            lock (Padlock)
            {
                var workflows = new List<Core.Db.Workflow>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + Workflow.ColumnNameId + ", "
                        + Workflow.ColumnNameXml
                        + " FROM " + Core.Db.Workflow.DocumentName + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var workflow = new Workflow
                                {
                                    Id = (int)reader[Workflow.ColumnNameId],
                                    Xml = (string)reader[Workflow.ColumnNameXml]
                                };

                                workflows.Add(workflow);
                            }
                        }
                    }

                    return workflows;
                }
            }
        }

        private static void IncrementStatusCountColumn(string statusCountColumnName)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " + 1;", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
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
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " - 1;", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
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
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Entry.DocumentName + "("
                        + Entry.ColumnNameName + ", "
                        + Entry.ColumnNameDescription + ", "
                        + Entry.ColumnNameLaunchType + ", "
                        + Entry.ColumnNameStatusDate + ", "
                        + Entry.ColumnNameStatus + ", "
                        + Entry.ColumnNameWorkflowId + ", "
                        + Entry.ColumnNameJobId + ", "
                        + Entry.ColumnNameLogs + ") VALUES("
                        + "'" + (entry.Name ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + (entry.Description ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + (int)entry.LaunchType + ", "
                        + "'" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", "
                        + (int)entry.Status + ", "
                        + entry.WorkflowId + ", "
                        + "'" + (entry.JobId ?? "") + "', "
                        + "'" + (entry.Logs ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ");"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.HistoryEntry.DocumentName + "("
                        + HistoryEntry.ColumnNameName + ", "
                        + HistoryEntry.ColumnNameDescription + ", "
                        + HistoryEntry.ColumnNameLaunchType + ", "
                        + HistoryEntry.ColumnNameStatusDate + ", "
                        + HistoryEntry.ColumnNameStatus + ", "
                        + HistoryEntry.ColumnNameWorkflowId + ", "
                        + HistoryEntry.ColumnNameLogs + ") VALUES("
                        + "'" + (entry.Name ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + (entry.Description ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + (int)entry.LaunchType + ", "
                        + "'" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "'" + ", "
                        + (int)entry.Status + ", "
                        + entry.WorkflowId + ", "
                        + "'" + (entry.Logs ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ");"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.User.DocumentName + "("
                        + User.ColumnNameUsername + ", "
                        + User.ColumnNamePassword + ", "
                        + User.ColumnNameUserProfile + ", "
                        + User.ColumnNameEmail + ", "
                        + User.ColumnNameCreatedOn + ", "
                        + User.ColumnNameModifiedOn + ") VALUES("
                        + "'" + (user.Username ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + (user.Password ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + (int)user.UserProfile + ", "
                        + "'" + (user.Email ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", "
                        + (user.ModifiedOn == DateTime.MinValue ? "NULL" : "'" + user.ModifiedOn.ToString(DATE_TIME_FORMAT) + "'") + ");"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.UserWorkflow.DocumentName + "("
                        + UserWorkflow.ColumnNameUserId + ", "
                        + UserWorkflow.ColumnNameWorkflowId + ") VALUES("
                        + int.Parse(userWorkflow.UserId) + ", "
                        + int.Parse(userWorkflow.WorkflowId) + ");"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Workflow.DocumentName + "("
                        + Workflow.ColumnNameXml + ") VALUES("
                        + "'" + (workflow.Xml ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + "); SELECT LAST_INSERT_ID(); "
                        , conn))
                    {
                        var id = (ulong)(command.ExecuteScalar() ?? throw new InvalidOperationException());

                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Entry.DocumentName + " SET "
                        + Entry.ColumnNameName + " = '" + (entry.Name ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Entry.ColumnNameDescription + " = '" + (entry.Description ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Entry.ColumnNameLaunchType + " = " + (int)entry.LaunchType + ", "
                        + Entry.ColumnNameStatusDate + " = '" + entry.StatusDate.ToString(DATE_TIME_FORMAT) + "', "
                        + Entry.ColumnNameStatus + " = " + (int)entry.Status + ", "
                        + Entry.ColumnNameWorkflowId + " = " + entry.WorkflowId + ", "
                        + Entry.ColumnNameJobId + " = '" + (entry.JobId ?? "") + "', "
                        + Entry.ColumnNameLogs + " = '" + (entry.Logs ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'"
                        + " WHERE "
                        + Entry.ColumnNameId + " = " + int.Parse(id) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                        + User.ColumnNamePassword + " = '" + (password ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'"
                        + " WHERE "
                        + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''").Replace("\\", "\\\\") + "';"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                        + User.ColumnNameUsername + " = '" + (user.Username ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + User.ColumnNamePassword + " = '" + (user.Password ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + User.ColumnNameUserProfile + " = " + (int)user.UserProfile + ", "
                        + User.ColumnNameEmail + " = '" + (user.Email ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + User.ColumnNameCreatedOn + " = '" + user.CreatedOn.ToString(DATE_TIME_FORMAT) + "', "
                        + User.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'"
                        + " WHERE "
                        + User.ColumnNameId + " = " + int.Parse(id) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                        + User.ColumnNameUsername + " = '" + (username ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + User.ColumnNameUserProfile + " = " + (int)up + ", "
                        + User.ColumnNameEmail + " = '" + (email ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + User.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'"
                        + " WHERE "
                        + User.ColumnNameId + " = " + int.Parse(userId) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Workflow.DocumentName + " SET "
                        + Workflow.ColumnNameXml + " = '" + (workflow.Xml ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'"
                        + " WHERE "
                        + User.ColumnNameId + " = " + int.Parse(dbId) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + Entry.ColumnNameLogs
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE "
                        + Entry.ColumnNameId + " = " + int.Parse(entryId) + ";"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var logs = (string)reader[Entry.ColumnNameLogs];
                                return logs;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT " + HistoryEntry.ColumnNameLogs
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE "
                        + HistoryEntry.ColumnNameId + " = " + int.Parse(entryId) + ";"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var logs = (string)reader[HistoryEntry.ColumnNameLogs];
                                return logs;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (Padlock)
            {
                var users = new List<User>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var admin = new User
                                {
                                    Id = (int)reader[User.ColumnNameId],
                                    Username = (string)reader[User.ColumnNameUsername],
                                    Password = (string)reader[User.ColumnNamePassword],
                                    Email = (string)reader[User.ColumnNameEmail],
                                    UserProfile = (UserProfile)(int)reader[User.ColumnNameUserProfile],
                                    CreatedOn = (DateTime)reader[User.ColumnNameCreatedOn],
                                    ModifiedOn = reader[User.ColumnNameModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnNameModifiedOn]
                                };

                                users.Add(admin);
                            }
                        }
                    }
                }

                return users;
            }
        }

        public override string InsertRecord(Core.Db.Record record)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Record.DocumentName + "("
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
                        + "'" + (record.Name ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + (record.Description ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + (record.Approved ? "1" : "0") + ", "
                        + (record.StartDate == null ? "NULL" : "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                        + (record.EndDate == null ? "NULL" : "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                        + "'" + (record.Comments ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + (record.ManagerComments ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + int.Parse(record.CreatedBy) + ", "
                        + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ", "
                        + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                        + (record.ModifiedOn == null ? "NULL" : "'" + record.ModifiedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                         + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                        + (record.AssignedOn == null ? "NULL" : "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ");"
                        + " SELECT LAST_INSERT_ID();"
                        , conn))
                    {
                        var id = (ulong)(command.ExecuteScalar() ?? throw new InvalidOperationException());
                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Record.DocumentName + " SET "
                        + Record.ColumnNameName + " = '" + (record.Name ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Record.ColumnNameDescription + " = '" + (record.Description ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Record.ColumnNameApproved + " = " + (record.Approved ? "1" : "0") + ", "
                        + Record.ColumnNameStartDate + " = " + (record.StartDate == null ? "NULL" : "'" + record.StartDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                        + Record.ColumnNameEndDate + " = " + (record.EndDate == null ? "NULL" : "'" + record.EndDate.Value.ToString(DATE_TIME_FORMAT) + "'") + ", "
                        + Record.ColumnNameComments + " = '" + (record.Comments ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Record.ColumnNameManagerComments + " = '" + (record.ManagerComments ?? "").Replace("'", "''").Replace("\\", "\\\\") + "', "
                        + Record.ColumnNameCreatedBy + " = " + int.Parse(record.CreatedBy) + ", "
                        + Record.ColumnNameModifiedBy + " = " + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                        + Record.ColumnNameModifiedOn + " = '" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "', "
                        + Record.ColumnNameAssignedTo + " = " + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                        + Record.ColumnNameAssignedOn + " = " + (record.AssignedOn == null ? "NULL" : "'" + record.AssignedOn.Value.ToString(DATE_TIME_FORMAT) + "'")
                        + " WHERE "
                        + Record.ColumnNameId + " = " + int.Parse(recordId) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteRecords(string[] recordIds)
        {
            lock (Padlock)
            {
                if (recordIds.Length > 0)
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (var i = 0; i < recordIds.Length; i++)
                        {
                            var id = recordIds[i];
                            _ = builder.Append(id);
                            _ = i < recordIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                        }

                        using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Record.DocumentName
                            + " WHERE " + Record.ColumnNameId + " IN " + builder + ";", conn))
                        {
                            _ = command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = (int)reader[Record.ColumnNameId],
                                    Name = (string)reader[Record.ColumnNameName],
                                    Description = (string)reader[Record.ColumnNameDescription],
                                    Approved = (ulong)reader[Record.ColumnNameApproved] == 1,
                                    StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameStartDate],
                                    EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameEndDate],
                                    Comments = (string)reader[Record.ColumnNameComments],
                                    ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                                    CreatedBy = ((int)reader[Record.ColumnNameCreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnNameCreatedOn],
                                    ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameModifiedOn],
                                    AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameAssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameAssignedOn]
                                };

                                return record;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecords(string keyword)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = (int)reader[Record.ColumnNameId],
                                    Name = (string)reader[Record.ColumnNameName],
                                    Description = (string)reader[Record.ColumnNameDescription],
                                    Approved = (ulong)reader[Record.ColumnNameApproved] == 1,
                                    StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameStartDate],
                                    EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameEndDate],
                                    Comments = (string)reader[Record.ColumnNameComments],
                                    ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                                    CreatedBy = ((int)reader[Record.ColumnNameCreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnNameCreatedOn],
                                    ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameModifiedOn],
                                    AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameAssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameAssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedBy(string createdBy)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = (int)reader[Record.ColumnNameId],
                                    Name = (string)reader[Record.ColumnNameName],
                                    Description = (string)reader[Record.ColumnNameDescription],
                                    Approved = (ulong)reader[Record.ColumnNameApproved] == 1,
                                    StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameStartDate],
                                    EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameEndDate],
                                    Comments = (string)reader[Record.ColumnNameComments],
                                    ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                                    CreatedBy = ((int)reader[Record.ColumnNameCreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnNameCreatedOn],
                                    ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameModifiedOn],
                                    AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameAssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameAssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword)
        {
            lock (Padlock)
            {
                var records = new List<Record>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = (int)reader[Record.ColumnNameId],
                                    Name = (string)reader[Record.ColumnNameName],
                                    Description = (string)reader[Record.ColumnNameDescription],
                                    Approved = (ulong)reader[Record.ColumnNameApproved] == 1,
                                    StartDate = reader[Record.ColumnNameStartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameStartDate],
                                    EndDate = reader[Record.ColumnNameEndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameEndDate],
                                    Comments = (string)reader[Record.ColumnNameComments],
                                    ManagerComments = (string)reader[Record.ColumnNameManagerComments],
                                    CreatedBy = ((int)reader[Record.ColumnNameCreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnNameCreatedOn],
                                    ModifiedBy = reader[Record.ColumnNameModifiedBy] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnNameModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameModifiedOn],
                                    AssignedTo = reader[Record.ColumnNameAssignedTo] == DBNull.Value ? string.Empty : ((int)reader[Record.ColumnNameAssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnNameAssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnNameAssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override string InsertVersion(Core.Db.Version version)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Version.DocumentName + "("
                        + Version.ColumnNameRecordId + ", "
                        + Version.ColumnNameFilePath + ", "
                        + Version.ColumnNameCreatedOn + ")"
                        + " VALUES("
                        + int.Parse(version.RecordId) + ", "
                        + "'" + (version.FilePath ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + "'" + DateTime.Now.ToString(DATE_TIME_FORMAT) + "'" + ");"
                        + " SELECT LAST_INSERT_ID();"
                        , conn))
                    {
                        var id = (ulong)(command.ExecuteScalar() ?? throw new InvalidOperationException());
                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Version.DocumentName + " SET "
                        + Version.ColumnNameRecordId + " = " + int.Parse(version.RecordId) + ", "
                        + Version.ColumnNameFilePath + " = '" + (version.FilePath ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'"
                        + " WHERE "
                        + Version.ColumnNameId + " = " + int.Parse(versionId) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteVersions(string[] versionIds)
        {
            lock (Padlock)
            {
                if (versionIds.Length > 0)
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (var i = 0; i < versionIds.Length; i++)
                        {
                            var id = versionIds[i];
                            _ = builder.Append(id);
                            _ = i < versionIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                        }

                        using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Version.DocumentName
                            + " WHERE " + Version.ColumnNameId + " IN " + builder + ";", conn))
                        {
                            _ = command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (Padlock)
            {
                var versions = new List<Version>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + Version.ColumnNameId + ", "
                        + Version.ColumnNameRecordId + ", "
                        + Version.ColumnNameFilePath + ", "
                        + Version.ColumnNameCreatedOn
                        + " FROM " + Core.Db.Version.DocumentName
                        + " WHERE " + Version.ColumnNameRecordId + " = " + int.Parse(recordId)
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var version = new Version
                                {
                                    Id = (int)reader[Version.ColumnNameId],
                                    RecordId = ((int)reader[Version.ColumnNameRecordId]).ToString(),
                                    FilePath = (string)reader[Version.ColumnNameFilePath],
                                    CreatedOn = (DateTime)reader[Version.ColumnNameCreatedOn]
                                };

                                versions.Add(version);
                            }
                        }
                    }
                }

                return versions;
            }
        }

        public override Core.Db.Version GetLatestVersion(string recordId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + Version.ColumnNameId + ", "
                        + Version.ColumnNameRecordId + ", "
                        + Version.ColumnNameFilePath + ", "
                        + Version.ColumnNameCreatedOn
                        + " FROM " + Core.Db.Version.DocumentName
                        + " WHERE " + Version.ColumnNameRecordId + " = " + int.Parse(recordId)
                        + " ORDER BY " + Version.ColumnNameCreatedOn + " DESC"
                        + " LIMIT 1"
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var version = new Version
                                {
                                    Id = (int)reader[Version.ColumnNameId],
                                    RecordId = ((int)reader[Version.ColumnNameRecordId]).ToString(),
                                    FilePath = (string)reader[Version.ColumnNameFilePath],
                                    CreatedOn = (DateTime)reader[Version.ColumnNameCreatedOn]
                                };

                                return version;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override string InsertNotification(Core.Db.Notification notification)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Notification.DocumentName + "("
                        + Notification.ColumnNameAssignedBy + ", "
                        + Notification.ColumnNameAssignedOn + ", "
                        + Notification.ColumnNameAssignedTo + ", "
                        + Notification.ColumnNameMessage + ", "
                        + Notification.ColumnNameIsRead + ")"
                        + " VALUES("
                        + (!string.IsNullOrEmpty(notification.AssignedBy) ? int.Parse(notification.AssignedBy).ToString() : "NULL") + ", "
                        + "'" + notification.AssignedOn.ToString(DATE_TIME_FORMAT) + "'" + ", "
                        + (!string.IsNullOrEmpty(notification.AssignedTo) ? int.Parse(notification.AssignedTo).ToString() : "NULL") + ", "
                        + "'" + (notification.Message ?? "").Replace("'", "''").Replace("\\", "\\\\") + "'" + ", "
                        + (notification.IsRead ? "1" : "0") + ");"
                        + " SELECT LAST_INSERT_ID();"
                        , conn))
                    {
                        var id = (ulong)(command.ExecuteScalar() ?? throw new InvalidOperationException());
                        return id.ToString();
                    }
                }
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        _ = builder.Append(id);
                        _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Notification.DocumentName
                        + " SET " + Notification.ColumnNameIsRead + " = " + "1"
                        + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (var i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        _ = builder.Append(id);
                        _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                    }

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Notification.DocumentName
                        + " SET " + Notification.ColumnNameIsRead + " = " + "0"
                        + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (Padlock)
            {
                if (notificationIds.Length > 0)
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (var i = 0; i < notificationIds.Length; i++)
                        {
                            var id = notificationIds[i];
                            _ = builder.Append(id);
                            _ = i < notificationIds.Length - 1 ? builder.Append(", ") : builder.Append(')');
                        }

                        using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Notification.DocumentName
                            + " WHERE " + Notification.ColumnNameId + " IN " + builder + ";", conn))
                        {
                            _ = command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (Padlock)
            {
                var notifications = new List<Notification>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
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
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var notification = new Notification
                                {
                                    Id = (int)reader[Notification.ColumnNameId],
                                    AssignedBy = ((int)reader[Notification.ColumnNameAssignedBy]).ToString(),
                                    AssignedOn = (DateTime)reader[Notification.ColumnNameAssignedOn],
                                    AssignedTo = ((int)reader[Notification.ColumnNameAssignedTo]).ToString(),
                                    Message = (string)reader[Notification.ColumnNameMessage],
                                    IsRead = (ulong)reader[Notification.ColumnNameIsRead] == 1
                                };

                                notifications.Add(notification);
                            }
                        }
                    }
                }

                return notifications;
            }
        }

        public override bool HasNotifications(string assignedTo)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.Notification.DocumentName
                        + " WHERE (" + Notification.ColumnNameAssignedTo + " = " + int.Parse(assignedTo)
                        + " AND " + Notification.ColumnNameIsRead + " = " + "0" + ")"
                        + ";", conn))
                    {
                        var count = (long)(command.ExecuteScalar() ?? throw new InvalidOperationException());
                        var hasNotifications = count > 0;
                        return hasNotifications;
                    }
                }
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("INSERT INTO " + Core.Db.Approver.DocumentName + "("
                        + Approver.ColumnNameUserId + ", "
                        + Approver.ColumnNameRecordId + ", "
                        + Approver.ColumnNameApproved + ", "
                        + Approver.ColumnNameApprovedOn + ") VALUES("
                        + int.Parse(approver.UserId) + ", "
                        + int.Parse(approver.RecordId) + ", "
                        + (approver.Approved ? "1" : "0") + ", "
                        + (approver.ApprovedOn == null ? "NULL" : "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'") + ");"
                        + " SELECT LAST_INSERT_ID();"
                        , conn))
                    {
                        var id = (ulong)(command.ExecuteScalar() ?? throw new InvalidOperationException());
                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("UPDATE " + Core.Db.Approver.DocumentName + " SET "
                        + Approver.ColumnNameUserId + " = " + int.Parse(approver.UserId) + ", "
                        + Approver.ColumnNameRecordId + " = " + int.Parse(approver.RecordId) + ", "
                        + Approver.ColumnNameApproved + " = " + (approver.Approved ? "1" : "0") + ", "
                        + Approver.ColumnNameApprovedOn + " = " + (approver.ApprovedOn == null ? "NULL" : "'" + approver.ApprovedOn.Value.ToString(DATE_TIME_FORMAT) + "'")
                        + " WHERE "
                        + Approver.ColumnNameId + " = " + int.Parse(approverId) + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId) + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId)
                        + " AND " + Approver.ColumnNameApproved + " = " + "1"
                        + ";"
                        , conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (Padlock)
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnNameUserId + " = " + int.Parse(userId) + ";", conn))
                    {
                        _ = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (Padlock)
            {
                var approvers = new List<Approver>();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var command = new MySqlCommand("SELECT "
                        + Approver.ColumnNameId + ", "
                        + Approver.ColumnNameUserId + ", "
                        + Approver.ColumnNameRecordId + ", "
                        + Approver.ColumnNameApproved + ", "
                        + Approver.ColumnNameApprovedOn
                        + " FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnNameRecordId + " = " + int.Parse(recordId)
                        + ";", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var approver = new Approver
                                {
                                    Id = (int)reader[Approver.ColumnNameId],
                                    UserId = ((int)reader[Approver.ColumnNameUserId]).ToString(),
                                    RecordId = ((int)reader[Approver.ColumnNameRecordId]).ToString(),
                                    Approved = (ulong)reader[Approver.ColumnNameApproved] == 1,
                                    ApprovedOn = reader[Approver.ColumnNameApprovedOn] == DBNull.Value ? null : (DateTime?)reader[Approver.ColumnNameApprovedOn]
                                };

                                approvers.Add(approver);
                            }
                        }
                    }
                }

                return approvers;
            }
        }

        public override void Dispose()
        {
        }
    }
}
