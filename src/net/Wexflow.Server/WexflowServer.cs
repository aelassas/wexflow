using Microsoft.Owin.Hosting;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Server
{
    public partial class WexflowServer : ServiceBase
    {
        public static NameValueCollection Config = ConfigurationManager.AppSettings;

        private static readonly string SettingsFile = Config["WexflowSettingsFile"];
        private static readonly Core.LogLevel LogLevel = !string.IsNullOrEmpty(Config["LogLevel"]) ? (Core.LogLevel)Enum.Parse(typeof(Core.LogLevel), Config["LogLevel"], true) : Core.LogLevel.All;
        private static readonly string SuperAdminUsername = Config["SuperAdminUsername"];
        private static readonly bool EnableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);
        private static readonly bool EnableRecordsHotFolder = bool.Parse(Config["EnableRecordsHotFolder"]);
        private static readonly bool EnableEmailNotifications = bool.Parse(Config["EnableEmailNotifications"]);
        private static readonly string SmtpHost = Config["Smtp.Host"];
        private static readonly int SmtpPort = int.Parse(Config["Smtp.Port"]);
        private static readonly bool SmtpEnableSsl = bool.Parse(Config["Smtp.EnableSsl"]);
        private static readonly string SmtpUser = Config["Smtp.User"];
        private static readonly string SmtpPassword = Config["Smtp.Password"];
        private static readonly string SmtpFrom = Config["Smtp.From"];

        public static FileSystemWatcher WorkflowsWatcher;
        public static FileSystemWatcher RecordsWatcher;
        public static WexflowEngine WexflowEngine = new WexflowEngine(
            SettingsFile
            , LogLevel
            , EnableWorkflowsHotFolder
            , SuperAdminUsername
            , EnableEmailNotifications
            , SmtpHost
            , SmtpPort
            , SmtpEnableSsl
            , SmtpUser
            , SmtpPassword
            , SmtpFrom
            );

        private IDisposable _webApp;

        public WexflowServer()
        {
            InitializeComponent();
            ServiceName = "Wexflow";
            var startThread = new Thread(StartThread) { IsBackground = true };
            startThread.Start();
        }

        private void StartThread()
        {
            if (EnableWorkflowsHotFolder)
            {
                InitializeWorkflowsFileSystemWatcher();
            }
            else
            {
                Logger.Info("Workflows hot folder is disabled.");
            }

            if (EnableRecordsHotFolder)
            {
                // On file found.
                foreach (var file in Directory.GetFiles(WexflowEngine.RecordsHotFolder))
                {
                    var recordId = WexflowEngine.SaveRecordFromFile(file, SuperAdminUsername);

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
        }

        protected override void OnStart(string[] args)
        {
            _webApp?.Dispose();

            var port = int.Parse(Config["WexflowServicePort"]);
            var url = $"http://+:{port}";
            _webApp = WebApp.Start<Startup>(url);
        }

        protected override void OnStop()
        {
            WexflowEngine.Stop(true, true);

            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
        }

        public static void InitializeWorkflowsFileSystemWatcher()
        {
            Logger.Info("Initializing workflows FileSystemWatcher...");
            WorkflowsWatcher = new FileSystemWatcher
            {
                Path = WexflowEngine.WorkflowsFolder,
                Filter = "*.xml",
                IncludeSubdirectories = false
            };

            // Add event handlers.
            WorkflowsWatcher.Created += OnWorkflowCreated;
            WorkflowsWatcher.Changed += OnWorkflowChanged;
            WorkflowsWatcher.Deleted += OnWorkflowDeleted;

            // Begin watching.
            WorkflowsWatcher.EnableRaisingEvents = true;
            Logger.InfoFormat("Workflow.FileSystemWatcher.Path={0}", WorkflowsWatcher.Path);
            Logger.InfoFormat("Workflow.FileSystemWatcher.Filter={0}", WorkflowsWatcher.Filter);
            Logger.InfoFormat("Workflow.FileSystemWatcher.EnableRaisingEvents={0}", WorkflowsWatcher.EnableRaisingEvents);
            Logger.Info("Workflows FileSystemWatcher Initialized.");
        }

        private static void OnWorkflowCreated(object source, FileSystemEventArgs e)
        {
            try
            {
                if (WexflowEngine.IsFileLocked(e.FullPath))
                {
                    Logger.Info($"File lock detected on file {e.FullPath}");

                    while (WexflowEngine.IsFileLocked(e.FullPath))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (!IsDirectory(e.FullPath))
                {
                    Logger.Info("Workflow.FileSystemWatcher.OnCreated");

                    var admin = WexflowEngine.GetUser(SuperAdminUsername);
                    _ = WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, e.FullPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while creating the workflow {0}", ex, e.FullPath);
            }
        }

        private static void OnWorkflowChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                if (WexflowEngine.IsFileLocked(e.FullPath))
                {
                    Logger.Info($"File lock detected on file {e.FullPath}");

                    while (WexflowEngine.IsFileLocked(e.FullPath))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (!IsDirectory(e.FullPath))
                {
                    Logger.Info("Workflow.FileSystemWatcher.OnChanged");

                    Thread.Sleep(500);
                    var admin = WexflowEngine.GetUser(SuperAdminUsername);
                    _ = WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, e.FullPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while updating the workflow {0}", ex, e.FullPath);
            }
        }

        private static void OnWorkflowDeleted(object source, FileSystemEventArgs e)
        {
            Logger.Info("Workflow.FileSystemWatcher.OnDeleted");
            try
            {
                var removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.FilePath == e.FullPath);
                if (removedWorkflow != null)
                {
                    WexflowEngine.DeleteWorkflow(removedWorkflow.DbId);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while deleting the workflow {0}", ex, e.FullPath);
            }
        }

        public static void InitializeRecordsFileSystemWatcher()
        {
            Logger.Info("Initializing records FileSystemWatcher...");
            RecordsWatcher = new FileSystemWatcher
            {
                Path = WexflowEngine.RecordsHotFolder,
                Filter = "*.*",
                IncludeSubdirectories = false
            };

            // Add event handlers.
            RecordsWatcher.Created += OnRecordCreated;

            // Begin watching.
            RecordsWatcher.EnableRaisingEvents = true;
            Logger.InfoFormat("Record.FileSystemWatcher.Path={0}", RecordsWatcher.Path);
            Logger.InfoFormat("Record.FileSystemWatcher.Filter={0}", RecordsWatcher.Filter);
            Logger.InfoFormat("Record.FileSystemWatcher.EnableRaisingEvents={0}", RecordsWatcher.EnableRaisingEvents);
            Logger.Info("Records FileSystemWatcher Initialized.");
        }

        private static void OnRecordCreated(object source, FileSystemEventArgs e)
        {
            try
            {
                if (WexflowEngine.IsFileLocked(e.FullPath))
                {
                    Logger.Info($"File lock detected on file {e.FullPath}");

                    while (WexflowEngine.IsFileLocked(e.FullPath))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (!IsDirectory(e.FullPath))
                {
                    Logger.Info("Record.FileSystemWatcher.OnCreated");

                    Thread.Sleep(1000);
                    var recordId = WexflowEngine.SaveRecordFromFile(e.FullPath, SuperAdminUsername);
                    if (recordId != "-1")
                    {
                        Logger.Info($"Record inserted from file {e.FullPath}. RecordId: {recordId}");
                    }
                    else
                    {
                        Logger.Error($"An error occured while inserting a record from the file {e.FullPath}.");
                    }
                }
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
            {
                Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while creating the record {0}", ex, e.FullPath);
            }
        }

        /// <summary>
        /// Checks if a path is a directory.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Result.</returns>
        public static bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);

            var isDir = attr.HasFlag(FileAttributes.Directory);

            return isDir;
        }
    }
}
