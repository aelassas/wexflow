using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Wexflow.Core;
using Wexflow.Core.Db;

namespace Wexflow.Server
{
    public class WexflowServer
    {
        private static string superAdminUsername;

        public static PollingFileSystemWatcher Watcher;
        public static IConfiguration Config;
        public static WexflowEngine WexflowEngine;

        public static void Main(string[] args)
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);

            var settingsFile = Config["WexflowSettingsFile"];
            superAdminUsername = Config["SuperAdminUsername"];
            var enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);
            var enableRecordsHotFolder = bool.Parse(Config["EnableRecordsHotFolder"]);
            var enableEmailNotifications = bool.Parse(Config["EnableEmailNotifications"]);
            var smtpHost = Config["Smtp.Host"];
            var smtpPort = int.Parse(Config["Smtp.Port"]);
            var smtpEnableSsl = bool.Parse(Config["Smtp.EnableSsl"]);
            var smtpUser = Config["Smtp.User"];
            var smtpPassword = Config["Smtp.Password"];
            var smtpFrom = Config["Smtp.From"];
            WexflowEngine = new WexflowEngine(settingsFile
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
                foreach (var file in Directory.GetFiles(WexflowEngine.RecordsHotFolder))
                {
                    var recordId = WexflowEngine.SaveRecordFromFile(file, superAdminUsername);

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

            var port = int.Parse(Config["WexflowServicePort"]);

            var host = new WebHostBuilder()
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
            foreach (var change in e.Changes)
            {
                var path = Path.Combine(change.Directory, change.Name);
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

                            var admin = WexflowEngine.GetUser(superAdminUsername);
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

                            var admin = WexflowEngine.GetUser(superAdminUsername);
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
                            var removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.FilePath == path);
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
            foreach (var change in e.Changes)
            {
                var path = Path.Combine(change.Directory, change.Name);
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
                        var recordId = WexflowEngine.SaveRecordFromFile(path, superAdminUsername);

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
