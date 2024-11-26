using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core.Db;

namespace Wexflow.Core
{
    /// <summary>
    /// Wexflow engine.
    /// </summary>
    public class WexflowEngine
    {
        /// <summary>
        /// Log level.
        /// </summary>
        public LogLevel LogLevel { get; }
        /// <summary>
        /// Records db folder name.
        /// </summary>
        public string DbFolderName { get; }
        /// <summary>
        /// Super-admin user name.
        /// </summary>
        public string SuperAdminUsername { get; }
        /// <summary>
        /// Settings file path.
        /// </summary>
        public string SettingsFile { get; }
        /// <summary>
        /// Indicates whether workflows hot folder is enabled or not.
        /// </summary>
        public bool EnableWorkflowsHotFolder { get; }
        /// <summary>
        /// Indicates whether email notifications are enabled or not.
        /// </summary>
        public bool EnableEmailNotifications { get; }
        /// <summary>
        /// SMTP host.
        /// </summary>
        public string SmptHost { get; }
        /// <summary>
        /// SMTP port.
        /// </summary>
        public int SmtpPort { get; }
        /// <summary>
        ///  Indicates whether to enable SMTP SSL or not.
        /// </summary>
        public bool SmtpEnableSsl { get; }
        /// <summary>
        /// SMTP user.
        /// </summary>
        public string SmtpUser { get; }
        /// <summary>
        /// SMTP password.
        /// </summary>
        public string SmtpPassword { get; }
        /// <summary>
        /// SMTP from.
        /// </summary>
        public string SmtpFrom { get; }
        /// <summary>
        /// List of the Workflows loaded by Wexflow engine.
        /// </summary>
        public IList<Workflow> Workflows { get; }
        /// <summary>
        /// Database.
        /// </summary>
        public Db.Db Database { get; }
        /// <summary>
        /// Workflows hot folder path.
        /// </summary>
        public string WorkflowsFolder { get; private set; }
        /// <summary>
        /// Records folder path.
        /// </summary>
        public string RecordsFolder { get; private set; }
        /// <summary>
        /// Records hot folder path.
        /// </summary>
        public string RecordsHotFolder { get; private set; }
        /// <summary>
        /// Temp folder path.
        /// </summary>
        public string TempFolder { get; private set; }
        /// <summary>
        /// Workflows temp folder used for global variables parsing.
        /// </summary>
        public string RecordsTempFolder { get; private set; }
        /// <summary>
        /// Tasks folder path.
        /// </summary>
        public string TasksFolder { get; private set; }
        /// <summary>
        /// Approval folder path.
        /// </summary>
        public string ApprovalFolder { get; private set; }
        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; private set; }
        /// <summary>
        /// Tasks names file path.
        /// </summary>
        public string TasksNamesFile { get; private set; }
        /// <summary>
        /// Tasks settings file path.
        /// </summary>
        public string TasksSettingsFile { get; private set; }
        /// <summary>
        /// Database type.
        /// </summary>
        public DbType DbType { get; private set; }
        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Global variables file.
        /// </summary>
        public string GlobalVariablesFile { get; private set; }
        /// <summary>
        /// Global variables.
        /// </summary>
        public Variable[] GlobalVariables { get; private set; }

        //
        // Quartz scheduler
        //
        private static readonly NameValueCollection QuartzProperties = new()
        {
            // JSON serialization is the one supported under .NET Core (binary isn't)
            ["quartz.serializer.type"] = "json"
        };

        private static readonly StdSchedulerFactory SchedulerFactory = new(QuartzProperties);
        private static readonly IScheduler QuartzScheduler = SchedulerFactory.GetScheduler().Result;

        /// <summary>
        /// Creates a new instance of Wexflow engine.
        /// </summary>
        /// <param name="settingsFile">Settings file path.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="enableWorkflowsHotFolder">Indicates whether workflows hot folder is enabled or not.</param>
        /// <param name="superAdminUsername">Super-admin username.</param>
        /// <param name="enableEmailNotifications"></param>
        /// <param name="smtpHost">SMTP host.</param>
        /// <param name="smtpPort">SMTP port.</param>
        /// <param name="smtpEnableSsl">SMTP enable ssl.</param>
        /// <param name="smtpUser">SMTP user.</param>
        /// <param name="smtpPassword">SMTP password.</param>
        /// <param name="smtpFrom">SMTP from.</param>
        public WexflowEngine(string settingsFile
            , LogLevel logLevel
            , bool enableWorkflowsHotFolder
            , string superAdminUsername
            , bool enableEmailNotifications
            , string smtpHost
            , int smtpPort
            , bool smtpEnableSsl
            , string smtpUser
            , string smtpPassword
            , string smtpFrom
            )
        {
            SettingsFile = settingsFile;
            LogLevel = logLevel;
            EnableWorkflowsHotFolder = enableWorkflowsHotFolder;
            SuperAdminUsername = superAdminUsername;
            EnableEmailNotifications = enableEmailNotifications;
            SmptHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpEnableSsl = smtpEnableSsl;
            SmtpUser = smtpUser;
            SmtpPassword = smtpPassword;
            SmtpFrom = smtpFrom;
            Workflows = [];

            Logger.Info("");
            Logger.Info("Starting Wexflow Engine");

            LoadSettings();

            DbFolderName = DbType.ToString().ToLower();

            switch (DbType)
            {
                case DbType.LiteDB:
                    Database = new Db.LiteDB.Db(ConnectionString);
                    break;
                case DbType.MongoDB:
                    Database = new Db.MongoDB.Db(ConnectionString);
                    break;
                case DbType.RavenDB:
                    Database = new Db.RavenDB.Db(ConnectionString);
                    break;
                case DbType.PostgreSQL:
                    Database = new Db.PostgreSQL.Db(ConnectionString);
                    break;
                case DbType.SQLServer:
                    Database = new Db.SQLServer.Db(ConnectionString);
                    break;
                case DbType.MySQL:
                    Database = new Db.MySQL.Db(ConnectionString);
                    break;
                case DbType.SQLite:
                    Database = new Db.SQLite.Db(ConnectionString);
                    break;
                case DbType.Firebird:
                    Database = new Db.Firebird.Db(ConnectionString);
                    break;
                case DbType.Oracle:
                    Database = new Db.Oracle.Db(ConnectionString);
                    break;
                case DbType.MariaDB:
                    Database = new Db.MariaDB.Db(ConnectionString);
                    break;
                default:
                    break;
            }

            Database?.Init();

            LoadGlobalVariables();

            LoadWorkflows();
        }

        /// <summary>
        /// Checks whether a cron expression is valid or not.
        /// </summary>
        /// <param name="expression">Cron expression</param>
        /// <returns></returns>
        public static bool IsCronExpressionValid(string expression)
        {
            var res = CronExpression.IsValidExpression(expression);
            return res;
        }

        private void LoadSettings()
        {
            var xdoc = XDocument.Load(SettingsFile);
            WorkflowsFolder = GetWexflowSetting(xdoc, "workflowsFolder");
            RecordsFolder = GetWexflowSetting(xdoc, "recordsFolder");
            if (!Directory.Exists(RecordsFolder))
            {
                _ = Directory.CreateDirectory(RecordsFolder);
            }

            RecordsHotFolder = GetWexflowSetting(xdoc, "recordsHotFolder");
            if (!Directory.Exists(RecordsHotFolder))
            {
                _ = Directory.CreateDirectory(RecordsHotFolder);
            }

            TempFolder = GetWexflowSetting(xdoc, "tempFolder");
            TasksFolder = GetWexflowSetting(xdoc, "tasksFolder");
            if (!Directory.Exists(TempFolder))
            {
                _ = Directory.CreateDirectory(TempFolder);
            }

            RecordsTempFolder = Path.Combine(TempFolder, "Records");
            if (!Directory.Exists(RecordsTempFolder))
            {
                _ = Directory.CreateDirectory(RecordsTempFolder);
            }

            ApprovalFolder = GetWexflowSetting(xdoc, "approvalFolder");
            XsdPath = GetWexflowSetting(xdoc, "xsd");
            TasksNamesFile = GetWexflowSetting(xdoc, "tasksNamesFile");
            TasksSettingsFile = GetWexflowSetting(xdoc, "tasksSettingsFile");
            DbType = (DbType)Enum.Parse(typeof(DbType), GetWexflowSetting(xdoc, "dbType"), true);
            ConnectionString = GetWexflowSetting(xdoc, "connectionString");
            GlobalVariablesFile = GetWexflowSetting(xdoc, "globalVariablesFile");
        }

        private void LoadGlobalVariables()
        {
            List<Variable> variables = [];
            var xdoc = XDocument.Load(GlobalVariablesFile);

            foreach (var xvariable in xdoc.Descendants("Variable"))
            {
                Variable variable = new()
                {
                    Key = (xvariable.Attribute("name") ?? throw new InvalidOperationException("name attribute of global variable not found")).Value,
                    Value = (xvariable.Attribute("value") ?? throw new InvalidOperationException("value attribute of global variable not found")).Value
                };
                variables.Add(variable);
            }

            GlobalVariables = [.. variables];
        }

        private static string GetWexflowSetting(XDocument xdoc, string name)
        {
            try
            {
                var xValue = (xdoc.XPathSelectElement($"/Wexflow/Setting[@name='{name}']") ?? throw new InvalidOperationException($"setting {name} not found")).Attribute("value");
                return xValue == null ? throw new Exception("Wexflow Setting Value attribute not found.") : xValue.Value;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured when reading Wexflow settings: Setting[@name='{0}']", e, name);
                return string.Empty;
            }
        }

        private void LoadWorkflows()
        {
            // Load workflows from db
            var workflows = Database.GetWorkflows().ToArray();

            foreach (var workflow in workflows)
            {
                var wf = LoadWorkflowFromDatabase(workflow);
                if (wf != null)
                {
                    Workflows.Add(wf);
                }
            }
        }

        /// <summary>
        /// Stops cron jobs.
        /// </summary>
        /// <param name="workflow">Workflow.</param>
        public static void StopCronJobs(Workflow workflow)
        {
            DeleteJob(workflow);
        }

        private Workflow LoadWorkflowFromDatabase(Db.Workflow workflow)
        {
            try
            {
                Workflow wf = new(
                       this
                    , 1
                    , []
                    , workflow.GetDbId()
                    , workflow.Xml
                    , TempFolder
                    , TasksFolder
                    , ApprovalFolder
                    , XsdPath
                    , Database
                    , GlobalVariables);
                Logger.InfoFormat("Workflow loaded: {0}", wf);
                return wf;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading the workflow {0}. Please check the workflow configuration. Xml: {1}", e, workflow.GetDbId(), workflow.Xml);
                return null;
            }
        }

        /// <summary>
        /// Saves a workflow in the database.
        /// </summary>
        /// <param name="xml">XML of the workflow.</param>
        /// <param name="userId">User id.</param>
        /// <param name="userProfile">User profile.</param>
        /// <param name="schedule">Indicates whether to schedule the workflow or not.</param>
        /// <returns>Workflow db id.</returns>
        public string SaveWorkflow(string userId, UserProfile userProfile, string xml, bool schedule)
        {
            try
            {
                using var xmlReader = XmlReader.Create(new StringReader(xml));
                XmlNamespaceManager xmlNamespaceManager = null;
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    xmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }

                var xdoc = XDocument.Parse(xml);
                var id = int.Parse(((xdoc.XPathSelectElement("/wf:Workflow", xmlNamespaceManager) ?? throw new InvalidOperationException()).Attribute("id") ?? throw new InvalidOperationException("id attribute of workflow not found")).Value);
                var workflow = Workflows.FirstOrDefault(w => w.Id == id);

                if (workflow == null) // insert
                {
                    // check the workflow before to save it
                    try
                    {
                        _ = new Workflow(
                         this
                        , 1
                        , []
                        , "-1"
                        , xml
                        , TempFolder
                        , TasksFolder
                        , ApprovalFolder
                        , XsdPath
                        , Database
                        , GlobalVariables
                        );
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("An error occured while saving the workflow {0}:", e, xml);
                        return "-1";
                    }
                    var dbId = Database.InsertWorkflow(new Db.Workflow { Xml = xml });

                    if (userProfile == UserProfile.Administrator)
                    {
                        InsertUserWorkflowRelation(userId, dbId);
                    }

                    var wfFromDb = Database.GetWorkflow(dbId);
                    var newWorkflow = LoadWorkflowFromDatabase(wfFromDb);

                    Logger.InfoFormat("New workflow {0} has been created. The workflow will be loaded.", newWorkflow.Name);
                    Workflows.Add(newWorkflow);
                    if (schedule)
                    {
                        ScheduleWorkflow(newWorkflow);
                    }
                    return dbId;
                }

                // update
                // check the workflow before to save it
                try
                {
                    _ = new Workflow(
                        this
                        , 1
                        , []
                        , "-1"
                        , xml
                        , TempFolder
                        , TasksFolder
                        , ApprovalFolder
                        , XsdPath
                        , Database
                        , GlobalVariables
                    );
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while saving the workflow {0}:", e, xml);
                    return "-1";
                }

                var workflowFromDb = Database.GetWorkflow(workflow.DbId);
                workflowFromDb.Xml = xml;
                Database.UpdateWorkflow(workflow.DbId, workflowFromDb);

                var changedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == workflowFromDb.GetDbId());

                if (changedWorkflow != null)
                {
                    _ = changedWorkflow.Stop(SuperAdminUsername);

                    StopCronJobs(changedWorkflow);
                    _ = Workflows.Remove(changedWorkflow);
                    Logger.InfoFormat("A change in the workflow {0} has been detected. The workflow will be reloaded.", changedWorkflow.Name);

                    var updatedWorkflow = LoadWorkflowFromDatabase(workflowFromDb);
                    Workflows.Add(updatedWorkflow);
                    if (schedule)
                    {
                        ScheduleWorkflow(updatedWorkflow);
                    }
                    return changedWorkflow.DbId;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while saving a workflow: {0}", e.Message);
            }

            return "-1";
        }

        /// <summary>
        /// Get workflow id from xml 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public int GetWorkflowId(string xml)
        {
            try
            {
                using var xmlReader = XmlReader.Create(new StringReader(xml));
                XmlNamespaceManager xmlNamespaceManager = null;
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    xmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }

                var xdoc = XDocument.Parse(xml);
                var id = int.Parse(((xdoc.XPathSelectElement("/wf:Workflow", xmlNamespaceManager) ?? throw new InvalidOperationException()).Attribute("id") ?? throw new InvalidOperationException("id attribute of workflow not found")).Value);
                return id;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving workflow id: {0}", e.Message);
                return -1;
            }
        }

        /// <summary>
        /// Saves a workflow from its file
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="userProfile">User Profile</param>
        /// <param name="filePath">Workflow File Path</param>
        /// <param name="schedule">Indicates whether to schedule the workflow or not.</param>
        /// <returns>Workflow DB Id</returns>
        public string SaveWorkflowFromFile(string userId, UserProfile userProfile, string filePath, bool schedule)
        {
            try
            {
                var xml = File.ReadAllText(filePath);
                var id = SaveWorkflow(userId, userProfile, xml, schedule);
                var workflow = Workflows.First(w => w.DbId == id);
                workflow.FilePath = filePath;
                return id;
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
            {
                Logger.InfoFormat("There is a sharing violation for the file {0}.", filePath);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while saving the workflow {0}", e, filePath);
            }

            return "-1";
        }

        /// <summary>
        /// Deletes a workflow from the database.
        /// </summary>
        /// <param name="dbId">DB ID.</param>
        public void DeleteWorkflow(string dbId)
        {
            try
            {
                Database.DeleteWorkflow(dbId);
                Database.DeleteUserWorkflowRelationsByWorkflowId(dbId);

                var removedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == dbId);
                if (removedWorkflow != null)
                {
                    Logger.InfoFormat("Workflow {0} is stopped and removed.", removedWorkflow.Name);
                    _ = removedWorkflow.Stop(SuperAdminUsername);

                    StopCronJobs(removedWorkflow);
                    lock (Workflows)
                    {
                        _ = Workflows.Remove(removedWorkflow);
                    }

                    if (EnableWorkflowsHotFolder)
                    {
                        if (!string.IsNullOrEmpty(removedWorkflow.FilePath) && File.Exists(removedWorkflow.FilePath))
                        {
                            File.Delete(removedWorkflow.FilePath);
                            Logger.InfoFormat("Workflow file {0} removed.", removedWorkflow.FilePath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting a workflow: {0}", e.Message);
            }
        }

        /// <summary>
        /// Deletes workflows from the database.
        /// </summary>
        /// <param name="dbIds">DB IDs</param>
        public bool DeleteWorkflows(string[] dbIds)
        {
            try
            {
                Database.DeleteWorkflows(dbIds);

                foreach (var dbId in dbIds)
                {
                    var removedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == dbId);
                    if (removedWorkflow != null)
                    {
                        Logger.InfoFormat("Workflow {0} is stopped and removed.", removedWorkflow.Name);
                        _ = removedWorkflow.Stop(SuperAdminUsername);

                        StopCronJobs(removedWorkflow);
                        _ = Workflows.Remove(removedWorkflow);
                        Database.DeleteUserWorkflowRelationsByWorkflowId(removedWorkflow.DbId);

                        if (EnableWorkflowsHotFolder)
                        {
                            if (!string.IsNullOrEmpty(removedWorkflow.FilePath) && File.Exists(removedWorkflow.FilePath))
                            {
                                File.Delete(removedWorkflow.FilePath);
                                Logger.InfoFormat("Workflow file {0} removed.", removedWorkflow.FilePath);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting workflows: {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Inserts a user workflow relation in DB.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        /// <param name="workflowId">Workflow DB ID.</param>
        public void InsertUserWorkflowRelation(string userId, string workflowId)
        {
            try
            {
                Database.InsertUserWorkflowRelation(new UserWorkflow { UserId = userId, WorkflowId = workflowId });
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while inserting user workflow relation: {0}", e.Message);
            }
        }

        /// <summary>
        /// Deletes user workflow relations.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        public void DeleteUserWorkflowRelations(string userId)
        {
            try
            {
                Database.DeleteUserWorkflowRelationsByUserId(userId);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting user workflow relations of user {0}: {1}", userId, e.Message);
            }
        }

        /// <summary>
        /// Returns user workflows.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        /// <returns>User worklofws.</returns>
        public Workflow[] GetUserWorkflows(string userId)
        {
            try
            {
                var userWorkflows = Database.GetUserWorkflows(userId).ToArray();
                var workflows = Workflows.Where(w => userWorkflows.Contains(w.DbId)).ToArray();
                return workflows;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving user workflows of user {0}: {1}", userId, e.Message);
                return [];
            }
        }

        /// <summary>
        /// Checks whether a user have access to a workflow.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="workflowId">Workflow db id.</param>
        /// <returns>true/false.</returns>
        public bool CheckUserWorkflow(string userId, string workflowId)
        {
            try
            {
                return Database.CheckUserWorkflow(userId, workflowId);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while checking user workflows of user {0}: {1}", userId, e.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns administrators search result.
        /// </summary>
        /// <param name="keyword">Keyword.</param>
        /// <param name="uo">User Order By.</param>
        /// <returns>Administrators search result.</returns>
        public User[] GetAdministrators(string keyword, UserOrderBy uo)
        {
            try
            {
                var admins = Database.GetAdministrators(keyword, uo);
                return admins.ToArray();
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving administrators: {0}", e.Message);
                return [];
            }
        }

        /// <summary>
        /// Returns non restricted users.
        /// </summary>
        /// <returns>Non restricted users.</returns>
        public User[] GetNonRestrictedUsers()
        {
            try
            {
                var users = Database.GetNonRestricedUsers();
                return users.ToArray();
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving administrators: {0}", e.Message);
                return [];
            }
        }

        /// <summary>
        /// Starts Wexflow engine.
        /// </summary>
        public void Run()
        {
            if (EnableWorkflowsHotFolder)
            {
                Logger.InfoFormat("Loading workflows from hot folder {0} ...", WorkflowsFolder);
                var workflowFiles = Directory.GetFiles(WorkflowsFolder, "*.xml");
                var admin = GetUser("admin");
                foreach (var worlflowFile in workflowFiles)
                {
                    _ = SaveWorkflowFromFile(admin.GetDbId(), UserProfile.SuperAdministrator, worlflowFile, false);
                }
                Logger.InfoFormat("Loading workflows from hot folder {0} finished.", WorkflowsFolder);
            }

            Logger.InfoFormat("Scheduling {0} workflows...", Workflows.Count);

            foreach (var workflow in Workflows)
            {
                ScheduleWorkflow(workflow);
            }

            if (!QuartzScheduler.IsStarted)
            {
                QuartzScheduler.Start().Wait();
            }

            Logger.InfoFormat("Scheduling {0} workflows finished.", Workflows.Count);
        }

        private static string GetJobIdentity(Workflow wf)
        {
            return $"job_{wf.Id}";
        }

        private static string GetTriggerIdentity(Workflow wf)
        {
            return $"trigger_{wf.Id}";
        }

        private static void DeleteJob(Workflow wf)
        {
            var jobIdentity = GetJobIdentity(wf);
            var jobKey = new JobKey(jobIdentity);
            var deleted = QuartzScheduler.DeleteJob(jobKey).Result;

            if (deleted)
            {
                Logger.InfoFormat("Workflow Job {0} was found and deleted ({1} - {2}).", jobIdentity, wf.Id, wf.Name);
            }
        }

        private void ScheduleWorkflow(Workflow wf)
        {
            try
            {
                if (wf.IsEnabled)
                {
                    if (wf.LaunchType == LaunchType.Startup)
                    {
                        _ = wf.StartAsync(SuperAdminUsername);
                    }
                    else if (wf.LaunchType == LaunchType.Periodic)
                    {
                        IDictionary<string, object> map = new Dictionary<string, object>
                        {
                            { "workflow", wf }
                        };

                        var jobIdentity = GetJobIdentity(wf);
                        var jobDetail = JobBuilder.Create<WorkflowJob>()
                            .WithIdentity(jobIdentity)
                            .SetJobData(new JobDataMap(map))
                            .Build();

                        var triggerIdentity = GetTriggerIdentity(wf);
                        var trigger = TriggerBuilder.Create()
                            .ForJob(jobDetail)
                            .WithSimpleSchedule(x => x.WithInterval(wf.Period).RepeatForever())
                            .WithIdentity(triggerIdentity)
                            .StartNow()
                            .Build();

                        DeleteJob(wf);
                        QuartzScheduler.ScheduleJob(jobDetail, trigger).Wait();
                    }
                    else if (wf.LaunchType == LaunchType.Cron)
                    {
                        IDictionary<string, object> map = new Dictionary<string, object>
                        {
                            { "workflow", wf }
                        };

                        var jobIdentity = GetJobIdentity(wf);
                        var jobDetail = JobBuilder.Create<WorkflowJob>()
                            .WithIdentity(jobIdentity)
                            .SetJobData(new JobDataMap(map))
                            .Build();

                        var triggerIdentity = GetTriggerIdentity(wf);
                        var trigger = TriggerBuilder.Create()
                            .ForJob(jobDetail)
                            .WithCronSchedule(wf.CronExpression)
                            .WithIdentity(triggerIdentity)
                            .StartNow()
                            .Build();

                        DeleteJob(wf);
                        QuartzScheduler.ScheduleJob(jobDetail, trigger).Wait();
                    }
                }
                else
                {
                    DeleteJob(wf);
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while scheduling the workflow {0}: ", e, wf);
            }
        }

        /// <summary>
        /// Stops Wexflow engine.
        /// </summary>
        /// <param name="stopQuartzScheduler">Tells if Quartz scheduler should be stopped or not.</param>
        /// <param name="clearStatusCountAndEntries">Indicates whether to clear statusCount and entries.</param>
        public void Stop(bool stopQuartzScheduler, bool clearStatusCountAndEntries)
        {
            if (stopQuartzScheduler)
            {
                QuartzScheduler.Shutdown().Wait();
            }

            foreach (var workflow in Workflows)
            {
                var innerWorkflows = workflow.Jobs.Values.ToArray();
                for (var i = innerWorkflows.Length - 1; i >= 0; i--)
                {
                    var innerWorkflow = innerWorkflows[i];
                    if (innerWorkflow.IsRunning)
                    {
                        _ = innerWorkflow.Stop(SuperAdminUsername);
                    }
                }
            }
            Logger.Info("Workflows stopped.");

            if (clearStatusCountAndEntries)
            {
                Database.ClearStatusCount();
                Database.ClearEntries();
                Logger.Info("Status count and dashboard entries cleared.");
            }

            Database.Dispose();
            Logger.Info("Database disposed.");
        }

        /// <summary>
        /// Gets a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns></returns>
        public Workflow GetWorkflow(int workflowId)
        {
            return Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        /// <param name="startedBy">Username of the user that started the workflow.</param>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="restVariables">Rest variables</param>
        /// <returns>Instance id.</returns>
        public Guid StartWorkflow(string startedBy, int workflowId, List<Variable> restVariables = null)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled)
                {
                    var instanceId = wf.StartAsync(startedBy, restVariables);
                    return instanceId;
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Stops a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="instanceId">Job instance Id.</param>
        /// <param name="stoppedBy">Username of the user who stopped the workflow.</param>
        /// <returns>Result.</returns>
        public bool StopWorkflow(int workflowId, Guid instanceId, string stoppedBy)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled)
                {
                    var innerWf = wf.Jobs.Where(kvp => kvp.Key.Equals(instanceId)).Select(kvp => kvp.Value).FirstOrDefault();

                    if (innerWf == null)
                    {
                        Logger.ErrorFormat("Instance {0} not found.", instanceId);
                    }
                    else
                    {
                        return innerWf.Stop(stoppedBy);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="instanceId">Job instance Id.</param>
        public bool SuspendWorkflow(int workflowId, Guid instanceId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled)
                {
                    var innerWf = wf.Jobs.Where(kvp => kvp.Key.Equals(instanceId)).Select(kvp => kvp.Value).FirstOrDefault();

                    if (innerWf == null)
                    {
                        Logger.ErrorFormat("Instance {0} not found.", instanceId);
                    }
                    else
                    {
                        return innerWf.Suspend();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="instanceId">Job instance Id.</param>
        public void ResumeWorkflow(int workflowId, Guid instanceId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled)
                {
                    var innerWf = wf.Jobs.Where(kvp => kvp.Key.Equals(instanceId)).Select(kvp => kvp.Value).FirstOrDefault();

                    if (innerWf == null)
                    {
                        Logger.ErrorFormat("Instance {0} not found.", instanceId);
                    }
                    else
                    {
                        innerWf.Resume();
                    }
                }
            }
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="instanceId">Job instance Id.</param>
        /// <param name="approvedBy">Username of the user who approved the workflow.</param>
        /// <returns>Result.</returns>
        public bool ApproveWorkflow(int workflowId, Guid instanceId, string approvedBy)
        {
            try
            {
                var wf = GetWorkflow(workflowId);

                if (wf == null)
                {
                    Logger.ErrorFormat("Workflow {0} not found.", workflowId);
                    return false;
                }

                if (wf.IsApproval)
                {
                    if (wf.IsEnabled)
                    {
                        var innerWf = wf.Jobs.Where(kvp => kvp.Key.Equals(instanceId)).Select(kvp => kvp.Value).FirstOrDefault();

                        if (innerWf == null)
                        {
                            Logger.ErrorFormat("Instance {0} not found.", instanceId);
                        }
                        else
                        {
                            innerWf.Approve(approvedBy);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while approving the workflow {0}.", e, workflowId);
                return false;
            }
        }

        /// <summary>
        /// Rejects a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="instanceId">Job instance Id.</param>
        /// <param name="rejectedBy">Username of the user who rejected the workflow.</param>
        /// <returns>Result.</returns>
        public bool RejectWorkflow(int workflowId, Guid instanceId, string rejectedBy)
        {
            try
            {
                var wf = GetWorkflow(workflowId);

                if (wf == null)
                {
                    Logger.ErrorFormat("Workflow {0} not found.", workflowId);
                    return false;
                }

                if (wf.IsApproval)
                {
                    if (wf.IsEnabled)
                    {
                        var innerWf = wf.Jobs.Where(kvp => kvp.Key.Equals(instanceId)).Select(kvp => kvp.Value).FirstOrDefault();

                        if (innerWf == null)
                        {
                            Logger.ErrorFormat("Instance {0} not found.", instanceId);
                        }
                        else
                        {
                            innerWf.Reject(rejectedBy);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while approving the workflow {0}.", e, workflowId);
                return false;
            }
        }

        /// <summary>
        /// Returns status count
        /// </summary>
        /// <returns>Returns status count</returns>
        public StatusCount GetStatusCount()
        {
            return Database.GetStatusCount();
        }

        /// <summary>
        /// Inserts a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="userProfile">User profile.</param>
        /// <param name="email">Email.</param>
        public void InsertUser(string username, string password, UserProfile userProfile, string email)
        {
            Database.InsertUser(new User
            {
                Username = username,
                Password = password,
                UserProfile = userProfile,
                Email = email
            });
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="userId">User's id.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="userProfile">User's profile.</param>
        /// <param name="email">User's email.</param>
        public void UpdateUser(string userId, string username, string password, UserProfile userProfile, string email)
        {
            var user = Database.GetUserById(userId);
            Database.UpdateUser(userId, new User
            {
                Username = username,
                Password = password,
                UserProfile = userProfile,
                Email = email,
                CreatedOn = user.CreatedOn
            });
        }

        /// <summary>
        /// Updates username and email.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="username">New username.</param>
        /// <param name="email">New email.</param>
        /// <param name="up">User profile.</param>
        public void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, int up)
        {
            Database.UpdateUsernameAndEmailAndUserProfile(userId, username, email, (UserProfile)up);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void DeleteUser(string username, string password)
        {
            var user = Database.GetUser(username);
            Database.DeleteUser(username, password);
            Database.DeleteUserWorkflowRelationsByUserId(user.GetDbId());
            Database.DeleteApproversByUserId(user.GetDbId());
        }

        /// <summary>
        /// Gets a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns></returns>
        public User GetUser(string username)
        {
            return Database.GetUser(username);
        }

        /// <summary>
        /// Gets a user by Id.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>User.</returns>
        public User GetUserById(string userId)
        {
            return Database.GetUserById(userId);
        }

        /// <summary>
        /// Returns all the users.
        /// </summary>
        /// <returns>All the users.</returns>
        public User[] GetUsers()
        {
            var q = Database.GetUsers();
            return q.ToArray();
        }

        /// <summary>
        /// Search for users.
        /// </summary>
        /// <returns>All the users.</returns>
        public User[] GetUsers(string keyword, UserOrderBy uo)
        {
            var q = Database.GetUsers(keyword, uo);
            return q.ToArray();
        }

        /// <summary>
        /// Updates user password.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void UpdatePassword(string username, string password)
        {
            Database.UpdatePassword(username, password);
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date From.</param>
        /// <param name="to">Date To.</param>
        /// <param name="page">Page number.</param>
        /// <param name="entriesCount">Number of entries.</param>
        /// <param name="heo">EntryOrderBy</param>
        /// <returns>Returns all the entries</returns>
        public HistoryEntry[] GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = Database.GetHistoryEntries(keyword, from, to, page, entriesCount, heo);
            return col.ToArray();
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date From.</param>
        /// <param name="to">Date To.</param>
        /// <param name="page">Page number.</param>
        /// <param name="entriesCount">Number of entries.</param>
        /// <param name="heo">EntryOrderBy</param>
        /// <returns>Returns all the entries</returns>
        public Entry[] GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = Database.GetEntries(keyword, from, to, page, entriesCount, heo);
            return col.ToArray();
        }

        /// <summary>
        /// Gets the number of history entries by search keyword and date filter.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date from.</param>
        /// <param name="to">Date to.</param>
        /// <returns></returns>
        public long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            return Database.GetHistoryEntriesCount(keyword, from, to);
        }

        /// <summary>
        /// Gets the number of entries by search keyword and date filter.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date from.</param>
        /// <param name="to">Date to.</param>
        /// <returns></returns>
        public long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            return Database.GetEntriesCount(keyword, from, to);
        }

        /// <summary>
        /// Returns Status Date Min value.
        /// </summary>
        /// <returns>Status Date Min value.</returns>
        public DateTime GetHistoryEntryStatusDateMin()
        {
            return Database.GetHistoryEntryStatusDateMin();
        }

        /// <summary>
        /// Returns Status Date Max value.
        /// </summary>
        /// <returns>Status Date Max value.</returns>
        public DateTime GetHistoryEntryStatusDateMax()
        {
            return Database.GetHistoryEntryStatusDateMax();
        }

        /// <summary>
        /// Returns Status Date Min value.
        /// </summary>
        /// <returns>Status Date Min value.</returns>
        public DateTime GetEntryStatusDateMin()
        {
            return Database.GetEntryStatusDateMin();
        }

        /// <summary>
        /// Returns Status Date Max value.
        /// </summary>
        /// <returns>Status Date Max value.</returns>
        public DateTime GetEntryStatusDateMax()
        {
            return Database.GetEntryStatusDateMax();
        }

        /// <summary>
        /// Returns entry logs.
        /// </summary>
        /// <param name="entryId">Entry id.</param>
        /// <returns>Entry logs.</returns>
        public string GetEntryLogs(string entryId)
        {
            return Database.GetEntryLogs(entryId);
        }

        /// <summary>
        /// Returns entry logs.
        /// </summary>
        /// <param name="entryId">Entry id.</param>
        /// <returns>Entry logs.</returns>
        public string GetHistoryEntryLogs(string entryId)
        {
            return Database.GetHistoryEntryLogs(entryId);
        }

        /// <summary>
        /// Checks if a directory is empty.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <returns>Result.</returns>
        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// Saves a record in the database.
        /// </summary>
        /// <param name="recordId">Record id.</param>
        /// <param name="record">Record.</param>
        /// <param name="versions">Version.</param>
        /// <returns></returns>
        public string SaveRecord(string recordId, Record record, List<Db.Version> versions)
        {
            try
            {
                if (recordId == "-1") // insert
                {
                    var id = Database.InsertRecord(record);

                    foreach (var version in versions)
                    {
                        Db.Version v = new()
                        {
                            RecordId = id,
                            FilePath = version.FilePath,
                        };
                        var versionId = Database.InsertVersion(v);

                        // Move version file from temp folder to Records folder.
                        if (version.FilePath.Contains(RecordsTempFolder))
                        {
                            var fileName = Path.GetFileName(version.FilePath);
                            var destDir = Path.Combine(RecordsFolder, DbFolderName, id, versionId);
                            if (!Directory.Exists(destDir))
                            {
                                _ = Directory.CreateDirectory(destDir);
                            }
                            var destPath = Path.Combine(destDir, fileName);
                            File.Move(version.FilePath, destPath);
                            var parentDir = Path.GetDirectoryName(version.FilePath) ?? throw new InvalidOperationException("parentDir is null");
                            if (IsDirectoryEmpty(parentDir))
                            {
                                Directory.Delete(parentDir);
                                var recordTempDir = (Directory.GetParent(parentDir) ?? throw new InvalidOperationException("parentDir is null")).FullName;
                                if (IsDirectoryEmpty(recordTempDir))
                                {
                                    Directory.Delete(recordTempDir);
                                }
                            }

                            // Update version.
                            v.FilePath = destPath;
                            Database.UpdateVersion(versionId, v);
                        }
                    }

                    return id;
                }
                else // update
                {
                    Database.UpdateRecord(recordId, record);

                    var recordVersions = Database.GetVersions(recordId);

                    List<string> versionsToDelete = [];
                    List<Db.Version> versionsToDeleteObjs = [];
                    foreach (var version in recordVersions)
                    {
                        if (versions.All(v => v.FilePath != version.FilePath))
                        {
                            versionsToDelete.Add(version.GetDbId());
                            versionsToDeleteObjs.Add(version);
                        }
                    }
                    Database.DeleteVersions([.. versionsToDelete]);

                    foreach (var version in versionsToDeleteObjs)
                    {
                        if (File.Exists(version.FilePath))
                        {
                            File.Delete(version.FilePath);
                        }

                        var versionDir = Path.GetDirectoryName(version.FilePath) ?? throw new InvalidOperationException("versionDir is null");
                        if (IsDirectoryEmpty(versionDir))
                        {
                            Directory.Delete(versionDir);
                            var recordDir = (Directory.GetParent(versionDir) ?? throw new InvalidOperationException("versionDir is null")).FullName;
                            if (IsDirectoryEmpty(recordDir))
                            {
                                Directory.Delete(recordDir);
                            }
                        }
                    }

                    foreach (var version in versions)
                    {
                        if (version.FilePath.Contains(RecordsTempFolder))
                        {
                            var versionId = Database.InsertVersion(version);

                            // Move version file from temp folder to Records folder.
                            var fileName = Path.GetFileName(version.FilePath);
                            var destDir = Path.Combine(RecordsFolder, DbFolderName, recordId, versionId);
                            if (!Directory.Exists(destDir))
                            {
                                _ = Directory.CreateDirectory(destDir);
                            }
                            var destPath = Path.Combine(destDir, fileName);
                            File.Move(version.FilePath, destPath);
                            var parentDir = Path.GetDirectoryName(version.FilePath) ?? throw new InvalidOperationException("parentDir is null");
                            if (IsDirectoryEmpty(parentDir))
                            {
                                Directory.Delete(parentDir);
                                var recordTempDir = (Directory.GetParent(parentDir) ?? throw new InvalidOperationException("parentDir is null")).FullName;
                                if (IsDirectoryEmpty(recordTempDir))
                                {
                                    Directory.Delete(recordTempDir);
                                }
                            }

                            // Update version.
                            version.FilePath = destPath;
                            Database.UpdateVersion(versionId, version);
                        }
                    }

                    return recordId;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while saving the record {0}.", e, recordId);
                return "-1";
            }
        }

        /// <summary>
        /// Saves a new record from a file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="createdBy">Created by username.</param>
        /// <returns>Record Id.</returns>
        public string SaveRecordFromFile(string filePath, string createdBy)
        {
            var fileName = Path.GetFileName(filePath);
            var destDir = Path.Combine(RecordsTempFolder, DbFolderName, "-1", Guid.NewGuid().ToString());
            if (!Directory.Exists(destDir))
            {
                _ = Directory.CreateDirectory(destDir);
            }
            var destPath = Path.Combine(destDir, fileName);
            File.Move(filePath, destPath);
            var parentDir = Path.GetDirectoryName(destPath) ?? throw new InvalidOperationException("parentDir is null");
            if (IsDirectoryEmpty(parentDir))
            {
                Directory.Delete(parentDir);
                var recordTempDir = (Directory.GetParent(parentDir) ?? throw new InvalidOperationException("parentDir is null")).FullName;
                if (IsDirectoryEmpty(recordTempDir))
                {
                    Directory.Delete(recordTempDir);
                }
            }

            var admin = GetUser(createdBy);
            Record record = new()
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                CreatedBy = admin.GetDbId()
            };

            Db.Version version = new()
            {
                FilePath = destPath
            };

            List<Db.Version> versions = [version];

            var recordId = SaveRecord("-1", record, versions);
            return recordId;
        }

        /// <summary>
        /// Deletes records.
        /// </summary>
        /// <param name="recordIds">Record ids.</param>
        /// <returns>Result.</returns>
        public bool DeleteRecords(string[] recordIds)
        {
            try
            {
                Database.DeleteRecords(recordIds);
                foreach (var recordId in recordIds)
                {
                    var versions = Database.GetVersions(recordId).ToArray();
                    var versionIds = versions.Select(v => v.GetDbId()).ToArray();
                    Database.DeleteVersions(versionIds);
                    foreach (var version in versions)
                    {
                        if (File.Exists(version.FilePath))
                        {
                            File.Delete(version.FilePath);

                            var versionDir = Path.GetDirectoryName(version.FilePath) ?? throw new InvalidOperationException("versionDir is null");
                            if (IsDirectoryEmpty(versionDir))
                            {
                                Directory.Delete(versionDir);
                                var recordDir = (Directory.GetParent(versionDir) ?? throw new InvalidOperationException("versionDir is null")).FullName;
                                if (IsDirectoryEmpty(recordDir))
                                {
                                    Directory.Delete(recordDir);
                                }
                            }
                        }
                    }

                    Database.DeleteApproversByRecordId(recordId);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while deleting records.", e);
                return false;
            }
        }

        /// <summary>
        /// Returns records by keyword.
        /// </summary>
        /// <param name="keyword">Keyword.</param>
        /// <returns>Records by keyword</returns>
        public Record[] GetRecords(string keyword)
        {
            return Database.GetRecords(keyword).ToArray();
        }

        /// <summary>
        /// Returns the records assigned to a user by keyword.
        /// </summary>
        /// <param name="createdBy">Created by user id.</param>
        /// <param name="assignedTo">Assigned to user id.</param>
        /// <param name="keyword">Keyword.</param>
        /// <returns>Records assigned to a user by keyword.</returns>
        public Record[] GetRecordsCreatedByOrAssignedTo(string createdBy, string assignedTo, string keyword)
        {
            return Database.GetRecordsCreatedByOrAssignedTo(createdBy, assignedTo, keyword).ToArray();
        }

        /// <summary>
        /// Returns the records created by a user.
        /// </summary>
        /// <param name="createdBy">User id.</param>
        /// <returns>Records created by a user.</returns>
        public Record[] GetRecordsCreatedBy(string createdBy)
        {
            return Database.GetRecordsCreatedBy(createdBy).ToArray();
        }

        /// <summary>
        /// returns record versions.
        /// </summary>
        /// <param name="recordId">Record id.</param>
        /// <returns>record versions.</returns>
        public Db.Version[] GetVersions(string recordId)
        {
            return Database.GetVersions(recordId).ToArray();
        }

        /// <summary>
        /// Inserts a notification in the database.
        /// </summary>
        /// <param name="notification">Notification.</param>
        /// <returns>Notification id.</returns>
        public string InsertNotification(Notification notification)
        {
            try
            {
                var id = Database.InsertNotification(notification);
                return id;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while inserting a notification.", e);
                return "-1";
            }
        }

        /// <summary>
        /// Marks notifications as read.
        /// </summary>
        /// <param name="notificationIds">Notification Ids.</param>
        public bool MarkNotificationsAsRead(string[] notificationIds)
        {
            try
            {
                Database.MarkNotificationsAsRead(notificationIds);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while marking notifications as read.", e);
                return false;
            }
        }

        /// <summary>
        /// Marks notifications as unread.
        /// </summary>
        /// <param name="notificationIds">Notification Ids.</param>
        public bool MarkNotificationsAsUnread(string[] notificationIds)
        {
            try
            {
                Database.MarkNotificationsAsUnread(notificationIds);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while marking notifications as read.", e);
                return false;
            }
        }

        /// <summary>
        /// Deletes notifications.
        /// </summary>
        /// <param name="notificationIds">Notification ids.</param>
        /// <returns>Result.</returns>
        public bool DeleteNotifications(string[] notificationIds)
        {
            try
            {
                Database.DeleteNotifications(notificationIds);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while deleting notifications.", e);
                return false;
            }
        }

        /// <summary>
        /// Returns the notifications assigned to a user.
        /// </summary>
        /// <param name="assignedTo">User id.</param>
        /// <param name="keyword">Keyword.</param>
        /// <returns>Notifications assigned to a user.</returns>
        public Notification[] GetNotifications(string assignedTo, string keyword)
        {
            return Database.GetNotifications(assignedTo, keyword).ToArray();
        }

        /// <summary>
        /// Indicates whether the user has notifications or not.
        /// </summary>
        /// <param name="assignedTo">Assigned to user id.</param>
        /// <returns></returns>
        public bool HasNotifications(string assignedTo)
        {
            return Database.HasNotifications(assignedTo);
        }

        /// <summary>
        /// Inserts an approver.
        /// </summary>
        /// <param name="approver">Approver.</param>
        /// <returns>Approver Id.</returns>
        public string InsertApprover(Approver approver)
        {
            try
            {
                var approverId = Database.InsertApprover(approver);
                return approverId;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while inserting an approver.", e);
                return "-1";
            }
        }

        /// <summary>
        /// Inserts an approver.
        /// </summary>
        /// <param name="approverId">Approver Id.</param>
        /// <param name="approver">Approver.</param>
        /// <returns>Result.</returns>
        public bool UpdateApprover(string approverId, Approver approver)
        {
            try
            {
                Database.UpdateApprover(approverId, approver);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while updating an approver.", e);
                return false;
            }
        }

        /// <summary>
        /// Deletes approved approvers of a record.
        /// </summary>
        /// <param name="recordId">Record Id.</param>
        /// <returns>Result.</returns>
        public bool DeleteApprovedApprovers(string recordId)
        {
            try
            {
                Database.DeleteApprovedApprovers(recordId);
                return true;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while deleting approved approvers of the record {0}.", e, recordId);
                return false;
            }
        }

        /// <summary>
        /// Retrieves approvers by record Id.
        /// </summary>
        /// <param name="recordId">Record Id.</param>
        /// <returns>Approvers.</returns>
        public Approver[] GetApprovers(string recordId)
        {
            return Database.GetApprovers(recordId).ToArray();
        }

        /// <summary>
        /// Checks whether a file is locked or no.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Result.</returns>
        public static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }
    }
}
