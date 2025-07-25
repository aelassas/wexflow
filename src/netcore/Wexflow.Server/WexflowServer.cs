using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Wexflow.Core;
using Wexflow.Core.Db;
using Wexflow.Core.PollingFileSystemWatcher;

namespace Wexflow.Server
{
    public class WexflowServer
    {
        private static string _superAdminUsername;

        public static PollingFileSystemWatcher Watcher { get; set; }
        public static IConfiguration Config { get; set; }
        public static WexflowEngine WexflowEngine { get; set; }

        /// <summary>
        /// A thread-safe collection of currently connected SSE client HTTP responses for status count updates.
        /// Key: HttpResponse of the SSE client connection.
        /// Value: dummy bool just to utilize ConcurrentDictionary as a set.
        /// </summary>
        public static ConcurrentDictionary<HttpResponse, bool> StatusCountClients { get; } = new();

        /// <summary>
        /// Broadcasts the given <see cref="StatusCount"/> object to all connected SSE clients.
        /// Sends the serialized JSON as an SSE event named "statusCount".
        /// </summary>
        /// <param name="statusCount">The current status counts to broadcast.</param>
        public static void BroadcastStatusCount(StatusCount statusCount)
        {
            // Serialize the StatusCount object to JSON using Newtonsoft.Json
            var json = JsonConvert.SerializeObject(statusCount);

            // Format the SSE message with event name and data
            var message = $"event: statusCount\ndata: {json}\n\n";
            var data = Encoding.UTF8.GetBytes(message);

            foreach (var kvp in StatusCountClients)
            {
                var response = kvp.Key;
                if (!response.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    try
                    {
                        // Write asynchronously but fire-and-forget here (consider awaiting if you want)
                        response.Body.WriteAsync(data, 0, data.Length);
                        response.Body.FlushAsync();
                    }
                    catch
                    {
                        // Remove disconnected clients
                        StatusCountClients.TryRemove(response, out _);
                    }
                }
                else
                {
                    // Remove aborted clients
                    StatusCountClients.TryRemove(response, out _);
                }
            }
        }

        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            XmlDocument log4NetConfig = new();
            log4NetConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            _ = XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);

            var port = int.Parse(Config["WexflowServicePort"]);

            var https = bool.TryParse(Config["HTTPS"], out var res) && res;
            var pfxFile = Config["PfxFile"];
            var pfxPassword = Config["PfxPassword"];

            JwtHelper.Initialize(Config); // Inject into JwtHelper

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(Config); // Use existing config
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .ConfigureKestrel(options =>
                        {
                            if (https && File.Exists(pfxFile))
                            {
                                options.ListenAnyIP(port, listenOptions =>
                                {
                                    listenOptions.UseHttps(pfxFile, pfxPassword);
                                });
                            }
                            else
                            {
                                options.ListenAnyIP(port);
                            }
                        })
                        .UseStartup<Startup>();
                })
                .Build();

            // Start Wexflow engine
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                WexflowEngine = services.GetRequiredService<WexflowEngine>();

                _superAdminUsername = Config["SuperAdminUsername"];

                var enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"] ?? throw new InvalidOperationException());
                var enableRecordsHotFolder = bool.Parse(Config["EnableRecordsHotFolder"] ?? throw new InvalidOperationException());

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
                        var recordId = WexflowEngine.SaveRecordFromFile(file, _superAdminUsername);

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

                // Broadcast the updated StatusCount
                Core.Workflow.OnStatusChanged += () =>
                {
                    var statusCount = WexflowEngine.GetStatusCount();
                    BroadcastStatusCount(statusCount);
                };

                await WexflowEngine.Run();
            }

            // Now start the web host and keep it running
            host.Run();

            Console.Write("Press any key to stop Wexflow server...");
            _ = Console.ReadKey();
            await WexflowEngine.Stop(true, true);
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

                            var admin = WexflowEngine.GetUser(_superAdminUsername);
                            _ = WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
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

                            var admin = WexflowEngine.GetUser(_superAdminUsername);
                            _ = WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
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
                    case WatcherChangeTypes.Renamed:
                    case WatcherChangeTypes.All:
                    default:
                        break;
                }
            }
        }

        public static void InitializeRecordsFileSystemWatcher()
        {
            Logger.Info("Initializing records PollingFileSystemWatcher...");
            Watcher = new PollingFileSystemWatcher(WexflowEngine.RecordsHotFolder);

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
                        var recordId = WexflowEngine.SaveRecordFromFile(path, _superAdminUsername);

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
