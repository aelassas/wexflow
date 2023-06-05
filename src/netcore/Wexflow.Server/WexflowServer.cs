using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Wexflow.Core;

namespace Wexflow.Server
{
    public class WexflowServer
    {
        private static string superAdminUsername;

        private static PollingFileSystemWatcher watcher;
        private static IConfiguration config;
        private static WexflowEngine wexflowEngine;

        public static PollingFileSystemWatcher Watcher { get => watcher; set => watcher = value; }
        public static IConfiguration Config { get => config; set => config = value; }
        public static WexflowEngine WexflowEngine { get => wexflowEngine; set => wexflowEngine = value; }

        public static void Main()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            XmlDocument log4NetConfig = new();
            log4NetConfig.Load(File.OpenRead("log4net.config"));
            log4net.Repository.ILoggerRepository repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);

            superAdminUsername = Config["SuperAdminUsername"];

            string settingsFile = Config["WexflowSettingsFile"];
            Core.LogLevel logLevel = !string.IsNullOrEmpty(Config["LogLevel"]) ? (Core.LogLevel)Enum.Parse(typeof(Core.LogLevel), Config["LogLevel"], true) : Core.LogLevel.All;
            bool enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);
            bool enableRecordsHotFolder = bool.Parse(Config["EnableRecordsHotFolder"]);
            bool enableEmailNotifications = bool.Parse(Config["EnableEmailNotifications"]);
            string smtpHost = Config["Smtp.Host"];
            int smtpPort = int.Parse(Config["Smtp.Port"]);
            bool smtpEnableSsl = bool.Parse(Config["Smtp.EnableSsl"]);
            string smtpUser = Config["Smtp.User"];
            string smtpPassword = Config["Smtp.Password"];
            string smtpFrom = Config["Smtp.From"];

            WexflowEngine = new WexflowEngine(
                settingsFile
                , logLevel
                , enableWorkflowsHotFolder
                , superAdminUsername
                , enableEmailNotifications
                , smtpHost
                , smtpPort
                , smtpEnableSsl
                , smtpUser
                , smtpPassword
                , smtpFrom
                );

            if (enableWorkflowsHotFolder)
            {
                InitializeWorkflowsFileSystemWatcher();
            }
            else
            {
                Logger.Info("Workflows hot folder is disabled.");
            }

            if (enableRecordsHotFolder)
            {
                // On file found.
                foreach (string file in Directory.GetFiles(WexflowEngine.RecordsHotFolder))
                {
                    string recordId = WexflowEngine.SaveRecordFromFile(file, superAdminUsername);

                    if (recordId != "-1")
                    {
                        Logger.Info($"Record inserted from file {file}. RecordId: {recordId}");
                    }
                    else
                    {
                        Logger.Error($"An error occured while inserting a record from the file {file}.");
                    }
                }

                // On file created.
                InitializeRecordsFileSystemWatcher();
            }
            else
            {
                Logger.Info("Records hot folder is disabled.");
            }

            WexflowEngine.Run();

            int port = int.Parse(Config["WexflowServicePort"]);

            IWebHost host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel((context, options) =>
                {
                    options.ListenAnyIP(port);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();

            Console.Write("Press any key to stop Wexflow server...");
            Console.ReadKey();
            WexflowEngine.Stop(true, true);
        }

        public static void InitializeWorkflowsFileSystemWatcher()
        {
            Logger.Info("Initializing workflows PollingFileSystemWatcher...");
            Watcher = new PollingFileSystemWatcher(WexflowEngine.WorkflowsFolder, "*.xml");

            // Add event handlers.
            Watcher.ChangedDetailed += OnWorkflowChanged;

            // Begin watching.
            Watcher.Start();
            Logger.InfoFormat("Workflow.PollingFileSystemWatcher.Path={0}", Watcher.Path);
            Logger.InfoFormat("Workflow.PollingFileSystemWatcher.Filter={0}", Watcher.Filter);
            Logger.Info("Workflows PollingFileSystemWatcher Initialized.");
        }

        private static void OnWorkflowChanged(object source, PollingFileSystemEventArgs e)
        {
            foreach (FileChange change in e.Changes)
            {
                string path = Path.Combine(change.Directory, change.Name);
                switch (change.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        try
                        {
                            if (WexflowEngine.IsFileLocked(path))
                            {
                                Logger.Info($"File lock detected on file {path}");
                                while (WexflowEngine.IsFileLocked(path))
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            Logger.Info("Workflow.PollingFileSystemWatcher.OnCreated");

                            Core.Db.User admin = WexflowEngine.GetUser(superAdminUsername);
                            WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while creating the workflow {0}", ex, path);
                        }
                        break;
                    case WatcherChangeTypes.Changed:
                        try
                        {
                            if (WexflowEngine.IsFileLocked(path))
                            {
                                Logger.Info($"File lock detected on file {path}");
                                while (WexflowEngine.IsFileLocked(path))
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            Logger.Info("Workflow.PollingFileSystemWatcher.OnChanged");

                            Core.Db.User admin = WexflowEngine.GetUser(superAdminUsername);
                            WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while updating the workflow {0}", ex, path);
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                        Logger.Info("Workflow.PollingFileSystemWatcher.OnDeleted");
                        try
                        {
                            Workflow removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.FilePath == path);
                            if (removedWorkflow != null)
                            {
                                WexflowEngine.DeleteWorkflow(removedWorkflow.DbId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while deleting the workflow {0}", ex, path);
                        }
                        break;
                }
            }
        }

        public static void InitializeRecordsFileSystemWatcher()
        {
            Logger.Info("Initializing records PollingFileSystemWatcher...");
            Watcher = new PollingFileSystemWatcher(WexflowEngine.RecordsHotFolder, "*");

            // Add event handlers.
            Watcher.ChangedDetailed += OnRecordChanged;

            // Begin watching.
            Watcher.Start();
            Logger.InfoFormat("Record.PollingFileSystemWatcher.Path={0}", Watcher.Path);
            Logger.InfoFormat("Record.PollingFileSystemWatcher.Filter={0}", Watcher.Filter);
            Logger.Info("Records PollingFileSystemWatcher Initialized.");
        }

        private static void OnRecordChanged(object source, PollingFileSystemEventArgs e)
        {
            foreach (FileChange change in e.Changes)
            {
                string path = Path.Combine(change.Directory, change.Name);
                if (change.ChangeType == WatcherChangeTypes.Created)
                {
                    try
                    {
                        if (WexflowEngine.IsFileLocked(path))
                        {
                            Logger.Info($"File lock detected on file {path}");
                            while (WexflowEngine.IsFileLocked(path))
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        Logger.Info("Record.PollingFileSystemWatcher.OnCreated");

                        Thread.Sleep(1000);
                        string recordId = WexflowEngine.SaveRecordFromFile(path, superAdminUsername);

                        if (recordId != "-1")
                        {
                            Logger.Info($"Record inserted from file {path}. RecordId: {recordId}");
                        }
                        else
                        {
                            Logger.Error($"An error occured while inserting a record from the file {path}.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Error while creating the record {0}", ex, path);
                    }
                }
            }
        }

    }
}
