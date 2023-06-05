using LiteDB;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.Db.LiteDB
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object padlock = new object();
        private static LiteDatabase db;

        public Db(string connectionString) : base(connectionString)
        {
            db = new LiteDatabase(ConnectionString);
            db.Rebuild(new RebuildOptions { Collation = new Collation("/None") }); // /IgnoreCase, en-US/None, en-US/IgnoreCase, en-US/IgnoreCase,IgnoreSymbols
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            ILiteCollection<StatusCount> statusCountCol = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);

            StatusCount statusCount = new StatusCount
            {
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            statusCountCol.Insert(statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            ILiteCollection<User> usersCol = db.GetCollection<User>(Core.Db.User.DocumentName);
            if (usersCol.Count() == 0)
            {
                InsertDefaultUser();
            }
        }

        public override void ClearStatusCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                IEnumerable<StatusCount> statusCount = col.FindAll();
                col.DeleteMany(s => statusCount.Where(ss => ss.Id == s.Id).Count() > 0);
            }
        }

        public override void ClearEntries()
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                IEnumerable<Entry> entries = col.FindAll();
                col.DeleteMany(e => entries.Where(ee => ee.Id == e.Id).Count() > 0);
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                return statusCount;
            }
        }

        public override void IncrementPendingCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount++;
                    col.Update(statusCount);
                }
            }
        }

        public override void DecrementPendingCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementRunningCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount++;
                    col.Update(statusCount);
                }
            }
        }

        public override void DecrementRunningCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementDoneCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementDoneCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementFailedCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount++;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementRejectedCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RejectedCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementFailedCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementWarningCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementWarningCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementDisabledCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementDisabledCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount--;
                    col.Update(statusCount);
                }
            }
        }

        public override void IncrementStoppedCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementStoppedCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void ResetStatusCount()
        {
            lock (padlock)
            {
                ILiteCollection<StatusCount> col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                StatusCount statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount = 0;
                    statusCount.RunningCount = 0;
                    statusCount.DoneCount = 0;
                    statusCount.FailedCount = 0;
                    statusCount.WarningCount = 0;
                    statusCount.DisabledCount = 0;
                    statusCount.RejectedCount = 0;
                    col.Update(statusCount);
                }
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.FindAll();
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.FindOne(e => e.WorkflowId == workflowId);
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId, Guid jobId)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.FindOne(e => e.WorkflowId == workflowId && e.JobId == jobId.ToString());
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                Entry ie = new Entry
                {
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId,
                    JobId = entry.JobId,
                    Logs = entry.Logs
                };
                col.Insert(ie);
                col.EnsureIndex(e => e.WorkflowId);
                col.EnsureIndex(e => e.JobId);
                //col.EnsureIndex(e => e.Name, "LOWER($.Name)");
                col.EnsureIndex(e => e.Name);
                col.EnsureIndex(e => e.LaunchType);
                //col.EnsureIndex(e => e.Description, "LOWER($.Name)");
                col.EnsureIndex(e => e.Description);
                col.EnsureIndex(e => e.Status);
                col.EnsureIndex(e => e.StatusDate);
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                Entry e = new Entry
                {
                    Id = int.Parse(id),
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId,
                    JobId = entry.JobId,
                    Logs = entry.Logs
                };
                col.Update(e);
            }
        }

        public void DeleteEntry(int workflowId)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                col.DeleteMany(e => e.WorkflowId == workflowId);
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                user.CreatedOn = DateTime.Now;
                col.Insert(new User
                {
                    CreatedOn = user.CreatedOn,
                    Email = user.Email,
                    ModifiedOn = user.ModifiedOn,
                    Password = user.Password,
                    Username = user.Username,
                    UserProfile = user.UserProfile
                });
                //col.EnsureIndex(u => u.Username, "LOWER($.Username)");
                col.EnsureIndex(u => u.Username);
                col.EnsureIndex(u => u.UserProfile);
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                User dbUser = col.FindOne(u => u.Username == username);
                dbUser.Password = password;
                col.Update(dbUser);
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                int i = int.Parse(id);
                User dbUser = col.FindOne(u => u.Id == i);
                dbUser.ModifiedOn = DateTime.Now;
                dbUser.Username = user.Username;
                dbUser.Password = user.Password;
                dbUser.UserProfile = user.UserProfile;
                dbUser.Email = user.Email;
                col.Update(dbUser);
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                int i = int.Parse(userId);
                User dbUser = col.FindOne(u => u.Id == i);
                dbUser.ModifiedOn = DateTime.Now;
                dbUser.Username = username;
                dbUser.Email = email;
                dbUser.UserProfile = up;
                col.Update(dbUser);
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                User user = col.FindOne(u => u.Username == username);
                if (user != null && user.Password == password)
                {
                    col.DeleteMany(u => u.Username == username);
                    DeleteUserWorkflowRelationsByUserId(user.Id.ToString());
                }
                else
                {
                    throw new Exception("The password is incorrect.");
                }
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                User user = col.FindOne(u => u.Username == username);
                return user;
            }
        }

        public override Core.Db.User GetUserById(string userId)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                int id = int.Parse(userId);
                User user = col.FindOne(u => u.Id == id);
                return user;
            }
        }

        public override string GetPassword(string username)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                User user = col.FindOne(u => u.Username == username);
                return user.Password;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                return col.FindAll();
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                string keywordToLower = keyword.ToLower();
                BsonExpression query = null;

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = Query.Contains("Username", keywordToLower);
                }

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        if (query != null)
                        {
                            //return col.Find(Query.And(Query.All("Username"), query));

                            Query q = Query.All("Username");
                            q.Where.Add(query);
                            return col.Find(q);
                        }
                        else
                        {
                            return col.Find(Query.All("Username"));
                        }

                    case UserOrderBy.UsernameDescending:

                        if (query != null)
                        {
                            //return col.Find(Query.And(Query.All("Username", Query.Descending), query));

                            Query q = Query.All("Username", Query.Descending);
                            q.Where.Add(query);
                            return col.Find(q);
                        }
                        else
                        {
                            return col.Find(Query.All("Username", Query.Descending));
                        }
                }

                return Array.Empty<User>();
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                string keywordToLower = keyword.ToLower();
                BsonExpression query = null;

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = Query.And(Query.EQ("UserProfile", UserProfile.Administrator.ToString()), Query.Contains("Username", keywordToLower));
                }

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        if (query != null)
                        {
                            //return col.Find(Query.And(Query.All("Username"), query));

                            Query q = Query.All("Username");
                            q.Where.Add(query);
                            return col.Find(q);
                        }
                        else
                        {
                            //return col.Find(Query.And(Query.All("Username"), Query.EQ("UserProfile", UserProfile.Administrator.ToString())));

                            Query q = Query.All("Username");
                            q.Where.Add(Query.EQ("UserProfile", UserProfile.Administrator.ToString()));
                            return col.Find(q);
                        }

                    case UserOrderBy.UsernameDescending:
                        if (query != null)
                        {
                            //return col.Find(Query.And(Query.All("Username", Query.Descending), query));

                            Query q = Query.All("Username", Query.Descending);
                            q.Where.Add(query);
                            return col.Find(q);
                        }
                        else
                        {
                            //return col.Find(Query.And(Query.All("Username", Query.Descending), Query.EQ("UserProfile", UserProfile.Administrator.ToString())));

                            Query q = Query.All("Username", Query.Descending);
                            q.Where.Add(Query.EQ("UserProfile", UserProfile.Administrator.ToString()));
                            return col.Find(q);
                        }
                }

                return Array.Empty<User>();
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                HistoryEntry he = new HistoryEntry
                {
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId,
                    Logs = entry.Logs
                };
                col.Insert(he);
                col.EnsureIndex(e => e.WorkflowId);
                //col.EnsureIndex(e => e.Name, "LOWER($.Name)");
                col.EnsureIndex(e => e.Name);
                col.EnsureIndex(e => e.LaunchType);
                //col.EnsureIndex(e => e.Description, "LOWER($.Name)");
                col.EnsureIndex(e => e.Description);
                col.EnsureIndex(e => e.Status);
                col.EnsureIndex(e => e.StatusDate);
            }
        }

        public void UpdateHistoryEntry(HistoryEntry entry)
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                col.Update(entry);
            }
        }

        public void DeleteHistoryEntries(int workflowId)
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                col.DeleteMany(e => e.WorkflowId == workflowId);
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.FindAll();
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            lock (padlock)
            {
                string keywordToUpper = keyword.ToUpper();
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper));
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            lock (padlock)
            {
                string keywordToUpper = keyword.ToUpper();
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper), (page - 1) * entriesCount, entriesCount);
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                string keywordToLower = keyword.ToLower();
                int skip = (page - 1) * entriesCount;
                BsonExpression query = !string.IsNullOrEmpty(keyword)
                    ? Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                                    , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)))
                    : Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("StatusDate")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q1 = Query.All("StatusDate");
                        q1.Where.Add(query);

                        return col.Find(
                            q1
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusDateDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("StatusDate", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q2 = Query.All("StatusDate", Query.Descending);
                        q2.Where.Add(query);

                        return col.Find(
                            q2
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.WorkflowIdAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("WorkflowId")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q3 = Query.All("WorkflowId");
                        q3.Where.Add(query);

                        return col.Find(
                           q3
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.WorkflowIdDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("WorkflowId", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q4 = Query.All("WorkflowId", Query.Descending);
                        q4.Where.Add(query);

                        return col.Find(
                           q4
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.NameAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Name")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q5 = Query.All("Name");
                        q5.Where.Add(query);

                        return col.Find(
                            q5
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.NameDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Name", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q6 = Query.All("Name", Query.Descending);
                        q6.Where.Add(query);

                        return col.Find(
                            q6
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.LaunchTypeAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("LaunchType")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q7 = Query.All("LaunchType");
                        q7.Where.Add(query);

                        return col.Find(
                            q7
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.LaunchTypeDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("LaunchType", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q8 = Query.All("LaunchType", Query.Descending);
                        q8.Where.Add(query);

                        return col.Find(
                            q8
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.DescriptionAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Description")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q9 = Query.All("Description");
                        q9.Where.Add(query);

                        return col.Find(
                           q9
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.DescriptionDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Description", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q10 = Query.All("Description", Query.Descending);
                        q10.Where.Add(query);

                        return col.Find(
                            q10
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Status")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q11 = Query.All("Status");
                        q11.Where.Add(query);

                        return col.Find(
                            q11
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Status", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q12 = Query.All("Status", Query.Descending);
                        q12.Where.Add(query);

                        return col.Find(
                            q12
                            , skip
                            , entriesCount
                        );
                }

                return Array.Empty<HistoryEntry>();
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            lock (padlock)
            {
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                string keywordToLower = keyword.ToLower();
                int skip = (page - 1) * entriesCount;
                BsonExpression query = !string.IsNullOrEmpty(keyword)
                    ? Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                                    , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)))
                    : Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("StatusDate")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q1 = Query.All("StatusDate");
                        q1.Where.Add(query);

                        return col.Find(
                            q1
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusDateDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("StatusDate", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q2 = Query.All("StatusDate", Query.Descending);
                        q2.Where.Add(query);

                        return col.Find(
                            q2
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.WorkflowIdAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("WorkflowId")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q3 = Query.All("WorkflowId");
                        q3.Where.Add(query);

                        return col.Find(
                           q3
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.WorkflowIdDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("WorkflowId", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q4 = Query.All("WorkflowId", Query.Descending);
                        q4.Where.Add(query);

                        return col.Find(
                           q4
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.NameAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Name")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q5 = Query.All("Name");
                        q5.Where.Add(query);

                        return col.Find(
                            q5
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.NameDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Name", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q6 = Query.All("Name", Query.Descending);
                        q6.Where.Add(query);

                        return col.Find(
                            q6
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.LaunchTypeAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("LaunchType")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q7 = Query.All("LaunchType");
                        q7.Where.Add(query);

                        return col.Find(
                            q7
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.LaunchTypeDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("LaunchType", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q8 = Query.All("LaunchType", Query.Descending);
                        q8.Where.Add(query);

                        return col.Find(
                            q8
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.DescriptionAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Description")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q9 = Query.All("Description");
                        q9.Where.Add(query);

                        return col.Find(
                           q9
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.DescriptionDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Description", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q10 = Query.All("Description", Query.Descending);
                        q10.Where.Add(query);

                        return col.Find(
                            q10
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusAscending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Status")
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q11 = Query.All("Status");
                        q11.Where.Add(query);

                        return col.Find(
                            q11
                            , skip
                            , entriesCount
                        );

                    case EntryOrderBy.StatusDescending:

                        //return col.Find(
                        //    Query.And(
                        //        Query.All("Status", Query.Descending)
                        //        , query
                        //    )
                        //    , skip
                        //    , entriesCount
                        //);

                        Query q12 = Query.All("Status", Query.Descending);
                        q12.Where.Add(query);

                        return col.Find(
                            q12
                            , skip
                            , entriesCount
                        );
                }

                return Array.Empty<Entry>();
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            lock (padlock)
            {
                string keywordToUpper = keyword.ToUpper();
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).LongCount();
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                string keywordToLower = keyword.ToLower();
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                BsonExpression query = !string.IsNullOrEmpty(keyword)
                    ? Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                        , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)))
                    : Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
                return col.Find(query).LongCount();
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                string keywordToLower = keyword.ToLower();
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                BsonExpression query = !string.IsNullOrEmpty(keyword)
                    ? Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                        , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)))
                    : Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
                return col.Find(query).LongCount();
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                IEnumerable<HistoryEntry> q = col.Find(Query.All("StatusDate"));
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                IEnumerable<HistoryEntry> q = col.Find(Query.All("StatusDate", Query.Descending));
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.Entry.DocumentName);
                IEnumerable<HistoryEntry> q = col.Find(Query.All("StatusDate"));
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            lock (padlock)
            {
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.Entry.DocumentName);
                IEnumerable<HistoryEntry> q = col.Find(Query.All("StatusDate", Query.Descending));
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                Workflow wf = new Workflow { Xml = workflow.Xml };
                BsonValue res = col.Insert(wf);
                return res.AsInt32.ToString();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                Workflow wf = new Workflow { Id = int.Parse(dbId), Xml = workflow.Xml };
                col.Update(wf);
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                int i = int.Parse(id);
                col.DeleteMany(e => e.Id == i);
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                col.DeleteMany(e => ids.Contains(e.Id.ToString()));
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                return col.FindAll();
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (padlock)
            {
                ILiteCollection<Workflow> col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                return col.FindById(int.Parse(id));
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (padlock)
            {
                ILiteCollection<UserWorkflow> col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                UserWorkflow uw = new UserWorkflow
                {
                    UserId = userWorkflow.UserId,
                    WorkflowId = userWorkflow.WorkflowId
                };
                col.Insert(uw);
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (padlock)
            {
                ILiteCollection<UserWorkflow> col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                col.DeleteMany(uw => uw.UserId == userId);
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowId)
        {
            lock (padlock)
            {
                ILiteCollection<UserWorkflow> col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                col.DeleteMany(uw => uw.WorkflowId == workflowId);
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            lock (padlock)
            {
                ILiteCollection<UserWorkflow> col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                return col.Find(uw => uw.UserId == userId).Select(uw => uw.WorkflowId.ToString());
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            lock (padlock)
            {
                ILiteCollection<UserWorkflow> col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                UserWorkflow res = col.FindOne(uw => uw.UserId == userId && uw.WorkflowId == workflowId);
                return res != null;
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (padlock)
            {
                int id = int.Parse(entryId);
                ILiteCollection<Entry> col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                Entry entry = col.FindOne(e => e.Id == id);
                return entry.Logs;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (padlock)
            {
                int id = int.Parse(entryId);
                ILiteCollection<HistoryEntry> col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                HistoryEntry entry = col.FindOne(e => e.Id == id);
                return entry.Logs;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (padlock)
            {
                ILiteCollection<User> col = db.GetCollection<User>(Core.Db.User.DocumentName);
                IOrderedEnumerable<User> users = col.Find(u => u.UserProfile == UserProfile.SuperAdministrator || u.UserProfile == UserProfile.Administrator).OrderBy(u => u.Username);
                return users;
            }
        }

        public override string InsertRecord(Core.Db.Record record)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                Record r = new Record
                {
                    Approved = record.Approved,
                    AssignedOn = record.AssignedOn,
                    AssignedTo = record.AssignedTo,
                    Comments = record.Comments,
                    CreatedBy = record.CreatedBy,
                    CreatedOn = DateTime.Now,
                    Description = record.Description,
                    EndDate = record.EndDate,
                    ManagerComments = record.ManagerComments,
                    Name = record.Name,
                    StartDate = record.StartDate
                };
                string recordId = col.Insert(r).AsInt32.ToString();

                return recordId;
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                int bsonId = int.Parse(recordId);
                Record recordFromDb = col.FindById(bsonId);
                Record r = new Record
                {
                    Id = bsonId,
                    Approved = record.Approved,
                    AssignedOn = record.AssignedOn,
                    AssignedTo = record.AssignedTo,
                    Comments = record.Comments,
                    CreatedBy = record.CreatedBy,
                    //CreatedOn = record.CreatedOn,
                    CreatedOn = recordFromDb.CreatedOn,
                    Description = record.Description,
                    EndDate = record.EndDate,
                    ManagerComments = record.ManagerComments,
                    Name = record.Name,
                    StartDate = record.StartDate,
                    ModifiedBy = record.ModifiedBy,
                    ModifiedOn = DateTime.Now
                };
                col.Update(r);
            }
        }

        public override void DeleteRecords(string[] recordIds)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                col.DeleteMany(r => recordIds.Contains(r.Id.ToString()));
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                int bsonId = int.Parse(id);
                Record record = col.FindById(bsonId);
                return record;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecords(string keyword)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                string keywordToUpper = keyword.ToUpper();
                List<Record> records = col.Find(r => r.Name.ToUpper().Contains(keywordToUpper) || (!string.IsNullOrEmpty(r.Description) && r.Description.ToUpper().Contains(keywordToUpper))).OrderByDescending(r => r.CreatedOn).ToList();
                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedBy(string createdBy)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                List<Record> records = col.Find(r => r.CreatedBy == createdBy).OrderBy(r => r.Name).ToList();
                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword)
        {
            lock (padlock)
            {
                ILiteCollection<Record> col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                string keywordToUpper = keyword.ToUpper();
                List<Record> records = col.Find(r => (r.CreatedBy == createdBy || r.AssignedTo == assingedTo) && (r.Name.ToUpper().Contains(keywordToUpper) || (!string.IsNullOrEmpty(r.Description) && r.Description.ToUpper().Contains(keywordToUpper)))).OrderByDescending(r => r.CreatedOn).ToList();
                return records;
            }
        }

        public override string InsertVersion(Core.Db.Version version)
        {
            lock (padlock)
            {
                ILiteCollection<Version> col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                Version v = new Version
                {
                    RecordId = version.RecordId,
                    CreatedOn = DateTime.Now,
                    FilePath = version.FilePath
                };
                string versionId = col.Insert(v).AsInt32.ToString();
                return versionId;
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (padlock)
            {
                ILiteCollection<Version> col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                int bsonId = int.Parse(versionId);
                Version versionFromDb = col.FindById(bsonId);
                Version v = new Version
                {
                    Id = bsonId,
                    RecordId = version.RecordId,
                    //CreatedOn = version.CreatedOn,
                    CreatedOn = versionFromDb.CreatedOn,
                    FilePath = version.FilePath
                };
                col.Update(v);
            }
        }

        public override void DeleteVersions(string[] versionIds)
        {
            lock (padlock)
            {
                ILiteCollection<Version> col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                col.DeleteMany(v => versionIds.Contains(v.Id.ToString()));
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (padlock)
            {
                ILiteCollection<Version> col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                List<Version> versions = col.Find(v => v.RecordId == recordId).OrderBy(v => v.CreatedOn).ToList();
                return versions;
            }
        }

        public override Core.Db.Version GetLatestVersion(string recordId)
        {
            lock (padlock)
            {
                ILiteCollection<Version> col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                Version version = col.Find(v => v.RecordId == recordId).OrderByDescending(v => v.CreatedOn).FirstOrDefault();
                return version;
            }
        }

        public override string InsertNotification(Core.Db.Notification notification)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                Notification n = new Notification
                {
                    AssignedBy = notification.AssignedBy,
                    AssignedOn = notification.AssignedOn,
                    AssignedTo = notification.AssignedTo,
                    Message = notification.Message,
                    IsRead = notification.IsRead
                };
                string notificationId = col.Insert(n).AsInt32.ToString();
                return notificationId;
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                col.UpdateMany(n => new Notification
                {
                    Id = n.Id,
                    AssignedBy = n.AssignedBy,
                    AssignedOn = n.AssignedOn,
                    AssignedTo = n.AssignedTo,
                    Message = n.Message,
                    IsRead = true
                }, n => notificationIds.Contains(n.Id.ToString()));
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                col.UpdateMany(n => new Notification
                {
                    Id = n.Id,
                    AssignedBy = n.AssignedBy,
                    AssignedOn = n.AssignedOn,
                    AssignedTo = n.AssignedTo,
                    Message = n.Message,
                    IsRead = false
                }, n => notificationIds.Contains(n.Id.ToString()));
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                col.DeleteMany(n => notificationIds.Contains(n.Id.ToString()));
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                string keywordToUpper = keyword.ToUpper();
                List<Notification> notifications = col.Find(n => n.AssignedTo == assignedTo && n.Message.ToUpper().Contains(keywordToUpper)).OrderByDescending(n => n.AssignedOn).ToList();
                return notifications;
            }
        }

        public override bool HasNotifications(string assignedTo)
        {
            lock (padlock)
            {
                ILiteCollection<Notification> col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                IEnumerable<Notification> notifications = col.Find(n => n.AssignedTo == assignedTo && !n.IsRead);
                bool hasNotifications = notifications.Any();
                return hasNotifications;
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                Approver a = new Approver
                {
                    UserId = approver.UserId,
                    RecordId = approver.RecordId,
                    Approved = approver.Approved,
                    ApprovedOn = approver.ApprovedOn
                };
                string approverId = col.Insert(a).AsInt32.ToString();

                return approverId;
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                int bsonId = int.Parse(approverId);

                Approver a = new Approver
                {
                    Id = bsonId,
                    UserId = approver.UserId,
                    RecordId = approver.RecordId,
                    Approved = approver.Approved,
                    ApprovedOn = approver.ApprovedOn
                };

                col.Update(a);
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                col.DeleteMany(a => a.RecordId == recordId);
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                col.DeleteMany(a => a.Approved && a.RecordId == recordId);
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                col.DeleteMany(a => a.UserId == userId);
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (padlock)
            {
                ILiteCollection<Approver> col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                List<Approver> approvers = col.Find(a => a.RecordId == recordId).ToList();
                return approvers;
            }
        }

        public override void Dispose()
        {
            db.Dispose();
        }

    }
}
