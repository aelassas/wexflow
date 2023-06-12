using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;

namespace Wexflow.Core.Db.MongoDB
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object padlock = new();
        private static IMongoDatabase db;

        public Db(string connectionString) : base(connectionString)
        {
            var database = string.Empty;
            var mongoUrl = string.Empty;
            var enabledSslProtocols = false;
            var sslProtocols = SslProtocols.None;

            var connectionStringParts = ConnectionString.Split(';');
            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    var connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("Database="))
                    {
                        database = connPart.Replace("Database=", string.Empty);
                    }
                    else if (connPart.StartsWith("MongoUrl="))
                    {
                        mongoUrl = connPart.Replace("MongoUrl=", string.Empty);
                    }
                    else if (connPart.StartsWith("EnabledSslProtocols="))
                    {
                        enabledSslProtocols = bool.Parse(connPart.Replace("EnabledSslProtocols=", string.Empty));
                    }
                    else if (connPart.StartsWith("SslProtocols="))
                    {
                        sslProtocols = (SslProtocols)Enum.Parse(typeof(SslProtocols), connPart.Replace("SslProtocols=", string.Empty), true);
                    }
                }
            }

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoUrl));

            if (enabledSslProtocols)
            {
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = sslProtocols };
            }

            MongoClient client = new(settings);
            db = client.GetDatabase(database);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            var statusCountCol = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);

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

            statusCountCol.InsertOne(statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            var usersCol = db.GetCollection<User>(Core.Db.User.DocumentName);
            if (usersCol.CountDocuments(FilterDefinition<User>.Empty) == 0)
            {
                InsertDefaultUser();
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                return col.Find(FilterDefinition<Workflow>.Empty).ToEnumerable();
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                Workflow wf = new() { Xml = workflow.Xml };
                col.InsertOne(wf);
                return wf.Id;
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                return col.Find(w => w.Id == id).FirstOrDefault();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                var update = Builders<Workflow>.Update
                    .Set(w => w.Xml, workflow.Xml);

                _ = col.UpdateOne(w => w.Id == dbId, update);
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                _ = col.DeleteOne(e => e.Id == id);
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                _ = col.DeleteMany(uw => uw.WorkflowId == workflowDbId);
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
                _ = col.DeleteMany(e => ids.Contains(e.Id));
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (padlock)
            {
                var col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                UserWorkflow uw = new()
                {
                    UserId = userWorkflow.UserId,
                    WorkflowId = userWorkflow.WorkflowId
                };
                col.InsertOne(uw);
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                _ = col.DeleteMany(uw => uw.UserId == userId);
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                return col.Find(uw => uw.UserId == userId).ToEnumerable().Select(uw => uw.WorkflowId);
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
                var res = col.Find(uw => uw.UserId == userId && uw.WorkflowId == workflowId).FirstOrDefault();
                return res != null;
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var keywordToLower = keyword.ToLower();

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        return col.Find(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator).Sort(Builders<User>.Sort.Ascending(u => u.Username)).ToEnumerable();
                    case UserOrderBy.UsernameDescending:
                        return col.Find(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator).Sort(Builders<User>.Sort.Descending(u => u.Username)).ToEnumerable();
                    default:
                        break;
                }

                return Array.Empty<User>();
            }
        }

        public override void ClearStatusCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                _ = col.DeleteMany(FilterDefinition<StatusCount>.Empty);
            }
        }

        public override void ClearEntries()
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                _ = col.DeleteMany(FilterDefinition<Entry>.Empty);
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                return statusCount;
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.Find(FilterDefinition<Entry>.Empty).ToEnumerable();
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                user.CreatedOn = DateTime.Now;
                User nu = new()
                {
                    CreatedOn = user.CreatedOn,
                    Email = user.Email,
                    ModifiedOn = user.ModifiedOn,
                    Password = user.Password,
                    Username = user.Username,
                    UserProfile = user.UserProfile
                };
                col.InsertOne(nu);
                _ = col.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.UserProfile)));
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var update = Builders<User>.Update
                    .Set(u => u.ModifiedOn, DateTime.Now)
                    .Set(u => u.Username, user.Username)
                    .Set(u => u.Password, user.Password)
                    .Set(u => u.UserProfile, user.UserProfile)
                    .Set(u => u.Email, user.Email);

                _ = col.UpdateOne(u => u.Id == id, update);
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);

                var update = Builders<User>.Update
                    .Set(u => u.ModifiedOn, DateTime.Now)
                    .Set(u => u.Username, username)
                    .Set(u => u.UserProfile, up)
                    .Set(u => u.Email, email);

                _ = col.UpdateOne(u => u.Id == userId, update);
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var user = col.Find(u => u.Username == username).FirstOrDefault();
                return user;
            }
        }

        public override Core.Db.User GetUserById(string userId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var user = col.Find(u => u.Id == userId).FirstOrDefault();
                return user;
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var user = col.Find(u => u.Username == username).FirstOrDefault();
                if (user != null && user.Password == password)
                {
                    _ = col.DeleteMany(u => u.Username == username);
                    DeleteUserWorkflowRelationsByUserId(user.Id);
                }
                else
                {
                    throw new Exception("The password is incorrect.");
                }
            }
        }

        public override string GetPassword(string username)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var user = col.Find(u => u.Username == username).First();
                return user.Password;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                return col.Find(FilterDefinition<User>.Empty).ToEnumerable();
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var keywordToLower = keyword.ToLower();

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        return col.Find(u => u.Username.ToLower().Contains(keywordToLower)).Sort(Builders<User>.Sort.Ascending(u => u.Username)).ToEnumerable();
                    case UserOrderBy.UsernameDescending:
                        return col.Find(u => u.Username.ToLower().Contains(keywordToLower)).Sort(Builders<User>.Sort.Descending(u => u.Username)).ToEnumerable();
                    default:
                        break;
                }

                return Array.Empty<User>();
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                var dbUser = col.Find(u => u.Username == username).First();
                dbUser.Password = password;

                var update = Builders<User>.Update
                    .Set(u => u.Password, dbUser.Password);

                _ = col.UpdateOne(u => u.Id == dbUser.Id, update);
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(FilterDefinition<HistoryEntry>.Empty).ToEnumerable();
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            lock (padlock)
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).ToEnumerable();
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            lock (padlock)
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                var keywordToLower = keyword.ToLower();
                var skip = (page - 1) * entriesCount;

                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusDateDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.WorkflowIdAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.WorkflowIdDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.NameAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.NameDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.LaunchTypeAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.LaunchTypeDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.DescriptionAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.DescriptionDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusAscending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusDescending:

                        return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
                    default:
                        break;
                }

                return Array.Empty<HistoryEntry>();
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                var keywordToLower = keyword.ToLower();
                var skip = (page - 1) * entriesCount;

                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusDateDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.WorkflowIdAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.WorkflowIdDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.NameAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.NameDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.LaunchTypeAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.LaunchTypeDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.DescriptionAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.DescriptionDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusAscending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                    case EntryOrderBy.StatusDescending:

                        return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
                    default:
                        break;
                }

                return Array.Empty<Entry>();
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            lock (padlock)
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).CountDocuments();
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                var keywordToLower = keyword.ToLower();
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);

                return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).CountDocuments();
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                var keywordToLower = keyword.ToLower();
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);

                return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).CountDocuments();
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                var q = col.Find(FilterDefinition<HistoryEntry>.Empty).Sort(Builders<HistoryEntry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable();
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                var q = col.Find(FilterDefinition<HistoryEntry>.Empty).Sort(Builders<HistoryEntry>.Sort.Descending(e => e.StatusDate)).ToEnumerable();
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                var q = col.Find(FilterDefinition<Entry>.Empty).Sort(Builders<Entry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable();
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                var q = col.Find(FilterDefinition<Entry>.Empty).Sort(Builders<Entry>.Sort.Descending(e => e.StatusDate)).ToEnumerable();
                return q.Any() ? q.Select(e => e.StatusDate).First() : DateTime.Now;
            }
        }

        public override void IncrementDisabledCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.DisabledCount, statusCount.DisabledCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementRunningCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.RunningCount, statusCount.RunningCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.Find(e => e.WorkflowId == workflowId).FirstOrDefault();
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId, Guid jobId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                return col.Find(e => e.WorkflowId == workflowId && e.JobId == jobId.ToString()).FirstOrDefault();
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                Entry ie = new()
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
                col.InsertOne(ie);
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.WorkflowId)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.JobId)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Name)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.LaunchType)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Description)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Status)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.StatusDate)));
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                var update = Builders<Entry>.Update
                    .Set(e => e.Name, entry.Name)
                    .Set(e => e.Description, entry.Description)
                    .Set(e => e.LaunchType, entry.LaunchType)
                    .Set(e => e.Status, entry.Status)
                    .Set(e => e.StatusDate, entry.StatusDate)
                    .Set(e => e.WorkflowId, entry.WorkflowId)
                    .Set(e => e.JobId, entry.JobId)
                    .Set(e => e.Logs, entry.Logs);

                _ = col.UpdateOne(e => e.Id == id, update);
            }
        }

        public override void IncrementRejectedCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RejectedCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.RejectedCount, statusCount.RejectedCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementDoneCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.DoneCount, statusCount.DoneCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementWarningCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.WarningCount, statusCount.WarningCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementFailedCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.FailedCount, statusCount.FailedCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                HistoryEntry he = new()
                {
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId,
                    Logs = entry.Logs
                };
                col.InsertOne(he);
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.WorkflowId)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Name)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.LaunchType)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Description)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Status)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.StatusDate)));
            }
        }

        public override void DecrementRunningCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount--;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.RunningCount, statusCount.RunningCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementStoppedCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.StoppedCount, statusCount.StoppedCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void IncrementPendingCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount++;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.PendingCount, statusCount.PendingCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override void DecrementPendingCount()
        {
            lock (padlock)
            {
                var col = db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
                var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount--;

                    var update = Builders<StatusCount>.Update
                        .Set(sc => sc.PendingCount, statusCount.PendingCount);

                    _ = col.UpdateOne(sc => sc.Id == statusCount.Id, update);
                }
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
                var entry = col.Find(e => e.Id == entryId).First();
                return entry.Logs;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
                var entry = col.Find(e => e.Id == entryId).First();
                return entry.Logs;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (padlock)
            {
                var col = db.GetCollection<User>(Core.Db.User.DocumentName);
                return col.Find(u => u.UserProfile == UserProfile.SuperAdministrator || u.UserProfile == UserProfile.Administrator).Sort(Builders<User>.Sort.Ascending(u => u.Username)).ToList();
            }
        }

        public override string InsertRecord(Core.Db.Record record)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                Record r = new()
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
                col.InsertOne(r);
                _ = col.Indexes.CreateOne(new CreateIndexModel<Record>(Builders<Record>.IndexKeys.Ascending(rec => rec.Name)));
                _ = col.Indexes.CreateOne(new CreateIndexModel<Record>(Builders<Record>.IndexKeys.Ascending(rec => rec.Description)));
                return r.Id;
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                var update = Builders<Record>.Update
                    .Set(r => r.Approved, record.Approved)
                    .Set(r => r.AssignedOn, record.AssignedOn)
                    .Set(r => r.AssignedTo, record.AssignedTo)
                    .Set(r => r.Comments, record.Comments)
                    .Set(r => r.CreatedBy, record.CreatedBy)
                    //.Set(r => r.CreatedOn, record.CreatedOn)
                    .Set(r => r.Description, record.Description)
                    .Set(r => r.EndDate, record.EndDate)
                    .Set(r => r.ManagerComments, record.ManagerComments)
                    .Set(r => r.Name, record.Name)
                    .Set(r => r.StartDate, record.StartDate)
                    .Set(r => r.ModifiedBy, record.ModifiedBy)
                    .Set(r => r.ModifiedOn, DateTime.Now);

                _ = col.UpdateOne(r => r.Id == recordId, update);
            }
        }

        public override void DeleteRecords(string[] recordIds)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                _ = col.DeleteMany(r => recordIds.Contains(r.Id));
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                var record = col.Find(r => r.Id == id).FirstOrDefault();
                return record;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecords(string keyword)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                var keywordToUpper = keyword.ToUpper();
                var records = col.Find(r => r.Name.ToUpper().Contains(keywordToUpper) || (!string.IsNullOrEmpty(r.Description) && r.Description.ToUpper().Contains(keywordToUpper))).Sort(Builders<Record>.Sort.Descending(r => r.CreatedOn)).ToList();
                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedBy(string createdBy)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                var records = col.Find(r => r.CreatedBy == createdBy).Sort(Builders<Record>.Sort.Ascending(r => r.Name)).ToList();
                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Record>(Core.Db.Record.DocumentName);
                var keywordToUpper = keyword.ToUpper();
                var records = col.Find(r => (r.CreatedBy == createdBy || r.AssignedTo == assingedTo) && (r.Name.ToUpper().Contains(keywordToUpper) || (!string.IsNullOrEmpty(r.Description) && r.Description.ToUpper().Contains(keywordToUpper)))).Sort(Builders<Record>.Sort.Descending(r => r.CreatedOn)).ToList();
                return records;
            }
        }

        public override string InsertVersion(Core.Db.Version version)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                Version v = new()
                {
                    RecordId = version.RecordId,
                    CreatedOn = DateTime.Now,
                    FilePath = version.FilePath
                };
                col.InsertOne(v);
                return v.Id;
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                var update = Builders<Version>.Update
                    .Set(v => v.RecordId, version.RecordId)
                    //.Set(v => v.CreatedOn, version.CreatedOn)
                    .Set(v => v.FilePath, version.FilePath);

                _ = col.UpdateOne(v => v.Id == versionId, update);
            }
        }

        public override void DeleteVersions(string[] versionIds)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                _ = col.DeleteMany(v => versionIds.Contains(v.Id));
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                var versions = col.Find(v => v.RecordId == recordId).Sort(Builders<Version>.Sort.Ascending(v => v.CreatedOn)).ToList();
                return versions;
            }
        }

        public override Core.Db.Version GetLatestVersion(string recordId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Version>(Core.Db.Version.DocumentName);
                var version = col.Find(v => v.RecordId == recordId).Sort(Builders<Version>.Sort.Descending(v => v.CreatedOn)).FirstOrDefault();
                return version;
            }
        }

        public override string InsertNotification(Core.Db.Notification notification)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                Notification n = new()
                {
                    AssignedBy = notification.AssignedBy,
                    AssignedOn = notification.AssignedOn,
                    AssignedTo = notification.AssignedTo,
                    Message = notification.Message,
                    IsRead = notification.IsRead
                };
                col.InsertOne(n);
                return n.Id;
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                var notifications = col.Find(n => notificationIds.Contains(n.Id)).ToList();
                foreach (var notification in notifications)
                {
                    var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, true);

                    _ = col.UpdateOne(v => v.Id == notification.Id, update);
                }
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                var notifications = col.Find(n => notificationIds.Contains(n.Id)).ToList();
                foreach (var notification in notifications)
                {
                    var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, false);

                    _ = col.UpdateOne(v => v.Id == notification.Id, update);
                }
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                _ = col.DeleteMany(n => notificationIds.Contains(n.Id));
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                var keywordToUpper = keyword.ToUpper();
                var notifications = col.Find(n => n.AssignedTo == assignedTo && n.Message.ToUpper().Contains(keywordToUpper)).Sort(Builders<Notification>.Sort.Descending(n => n.AssignedOn)).ToList();
                return notifications;
            }
        }

        public override bool HasNotifications(string assignedTo)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Notification>(Core.Db.Notification.DocumentName);
                var notifications = col.Find(n => n.AssignedTo == assignedTo && !n.IsRead);
                var hasNotifications = notifications.Any();
                return hasNotifications;
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                Approver a = new()
                {
                    UserId = approver.UserId,
                    RecordId = approver.RecordId,
                    Approved = approver.Approved,
                    ApprovedOn = approver.ApprovedOn
                };
                col.InsertOne(a);
                return a.Id;
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                var update = Builders<Approver>.Update
                    .Set(u => u.UserId, approver.UserId)
                    .Set(u => u.RecordId, approver.RecordId)
                    .Set(u => u.Approved, approver.Approved)
                    .Set(u => u.ApprovedOn, approver.ApprovedOn);

                _ = col.UpdateOne(a => a.Id == approverId, update);
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                _ = col.DeleteMany(a => a.RecordId == recordId);
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                _ = col.DeleteMany(a => a.Approved && a.RecordId == recordId);
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                _ = col.DeleteMany(a => a.UserId == userId);
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (padlock)
            {
                var col = db.GetCollection<Approver>(Core.Db.Approver.DocumentName);
                return col.Find(a => a.RecordId == recordId).ToList();
            }
        }

        public override void Dispose()
        {
        }
    }
}
