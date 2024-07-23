using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using Wexflow.Core.PollingFileSystemWatcher;

namespace Wexflow.Tasks.FileSystemWatcher
{
    public class FileSystemWatcher : Task
    {
        public PollingFileSystemWatcher Watcher { get; private set; }
        public static string FolderToWatch { get; private set; }
        public static string Filter { get; private set; }
        public static bool IncludeSubFolders { get; private set; }
        public static string OnFileFound { get; private set; }
        public static string OnFileCreated { get; private set; }
        public static string OnFileChanged { get; private set; }
        public static string OnFileDeleted { get; private set; }
        public static bool SafeMode { get; private set; }
        public static List<string> CurrentLogs { get; private set; }

        private static readonly char[] separator = [','];

        public FileSystemWatcher(XElement xe, Workflow wf) : base(xe, wf)
        {
            FolderToWatch = GetSetting("folderToWatch");
            Filter = GetSetting("filter", "*.*");
            IncludeSubFolders = bool.Parse(GetSetting("includeSubFolders", "false"));
            OnFileFound = GetSetting("onFileFound");
            OnFileCreated = GetSetting("onFileCreated");
            OnFileChanged = GetSetting("onFileChanged");
            OnFileDeleted = GetSetting("onFileDeleted");
            SafeMode = bool.Parse(GetSetting("safeMode", "true"));
            CurrentLogs = [];
        }

        public override TaskStatus Run()
        {
            InfoFormat("Watching the folder {0} ...", FolderToWatch);

            try
            {
                if (!Directory.Exists(FolderToWatch))
                {
                    ErrorFormat("The folder {0} does not exist.", FolderToWatch);
                    return new TaskStatus(Status.Error);
                }

                Info("Checking existing files...");
                var files = GetFiles();

                foreach (var file in files)
                {
                    InfoFormat("FileSystemWatcher.OnFound started for {0}", file);
                    try
                    {
                        if (SafeMode && WexflowEngine.IsFileLocked(file))
                        {
                            Info($"File lock detected on file {file}");
                            while (WexflowEngine.IsFileLocked(file))
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        ClearFiles();
                        Files.Add(new FileInf(file, Id));
                        var tasks = GetTasks(OnFileFound);
                        foreach (var task in tasks)
                        {
                            task.Logs.Clear();
                            _ = task.Run();
                            CurrentLogs.AddRange(task.Logs);
                        }
                        _ = Files.RemoveAll(f => f.Path == file);
                    }
                    catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                    {
                        Logger.InfoFormat("There is a sharing violation for the file {0}.", file);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while triggering FileSystemWatcher.OnFound on the file {0}. Message: {1}", file, ex.Message);
                    }
                    finally
                    {
                        Info("FileSystemWatcher.OnFound finished.");
                        WaitOne();
                    }
                    try
                    {
                        var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                        entry.Logs = string.Join("\r\n", CurrentLogs);
                        Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while updating FileSystemWatcher.OnFound database entry.", ex);
                    }
                }
                Info("Checking existing files finished.");

                Info("Initializing PollingFileSystemWatcher...");
                Watcher = new PollingFileSystemWatcher(FolderToWatch, Filter, new EnumerationOptions { RecurseSubdirectories = IncludeSubFolders });

                // Add event handlers.
                Watcher.ChangedDetailed += OnChanged;

                // Begin watching.
                Watcher.Start();
                InfoFormat("PollingFileSystemWatcher.Path={0}", Watcher.Path);
                InfoFormat("PollingFileSystemWatcher.Filter={0}", Watcher.Filter);
                Info("PollingFileSystemWatcher Initialized.");

                Info("Begin watching ...");
                CurrentLogs.AddRange(Logs);
                while (!IsStopped)
                {
                    Thread.Sleep(1);
                }
                Watcher.Dispose();
            }
            catch (Exception e)
            {
                Watcher?.Dispose();
                ErrorFormat("An error occured while watching the folder {0}. Error: {1}", FolderToWatch, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");
            return new TaskStatus(Status.Success);
        }

        private void OnChanged(object source, PollingFileSystemEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                var path = Path.Combine(change.Directory, change.Name);
                switch (change.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        Info("PollingFileSystemWatcher.OnCreated started.");
                        try
                        {
                            if (SafeMode && WexflowEngine.IsFileLocked(path))
                            {
                                Info($"File lock detected on file {path}");
                                while (WexflowEngine.IsFileLocked(path))
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            ClearFiles();
                            Files.Add(new FileInf(path, Id));
                            var tasks = GetTasks(OnFileCreated);
                            foreach (var task in tasks)
                            {
                                task.Logs.Clear();
                                _ = task.Run();
                                CurrentLogs.AddRange(task.Logs);
                            }
                            _ = Files.RemoveAll(f => f.Path == path);
                        }
                        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                        {
                            Logger.InfoFormat("There is a sharing violation for the file {0}.", path);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while triggering PollingFileSystemWatcher.OnCreated on the file {0}. Message: {1}", path, ex.Message);
                        }
                        Info("PollingFileSystemWatcher.OnCreated finished.");

                        try
                        {
                            var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                            entry.Logs = string.Join("\r\n", CurrentLogs);
                            Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while updating PollingFileSystemWatcher.OnCreated database entry.", ex);
                        }
                        break;
                    case WatcherChangeTypes.Changed:
                        Info("PollingFileSystemWatcher.OnChanged started.");
                        try
                        {
                            if (SafeMode && WexflowEngine.IsFileLocked(path))
                            {
                                Info($"File lock detected on file {path}");
                                while (WexflowEngine.IsFileLocked(path))
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            ClearFiles();
                            Files.Add(new FileInf(path, Id));
                            var tasks = GetTasks(OnFileChanged);
                            foreach (var task in tasks)
                            {
                                task.Logs.Clear();
                                _ = task.Run();
                                CurrentLogs.AddRange(task.Logs);
                            }
                            _ = Files.RemoveAll(f => f.Path == path);
                        }
                        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                        {
                            Logger.InfoFormat("There is a sharing violation for the file {0}.", path);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while triggering PollingFileSystemWatcher.OnChanged on the file {0}. Message: {1}", path, ex.Message);
                        }
                        Info("PollingFileSystemWatcher.OnChanged finished.");

                        try
                        {
                            var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                            entry.Logs = string.Join("\r\n", CurrentLogs);
                            Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while updating PollingFileSystemWatcher.OnChanged database entry.", ex);
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                        Info("PollingFileSystemWatcher.OnDeleted started.");
                        try
                        {
                            ClearFiles();
                            Files.Add(new FileInf(path, Id));
                            var tasks = GetTasks(OnFileDeleted);
                            foreach (var task in tasks)
                            {
                                task.Logs.Clear();
                                _ = task.Run();
                                CurrentLogs.AddRange(task.Logs);
                            }
                            _ = Files.RemoveAll(f => f.Path == path);
                        }
                        catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                        {
                            Logger.InfoFormat("There is a sharing violation for the file {0}.", path);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while triggering PollingFileSystemWatcher.OnDeleted on the file {0}. Message: {1}", path, ex.Message);
                        }
                        Info("PollingFileSystemWatcher.OnDeleted finished.");

                        try
                        {
                            var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                            entry.Logs = string.Join("\r\n", CurrentLogs);
                            Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                        }
                        catch (Exception ex)
                        {
                            ErrorFormat("An error while updating PollingFileSystemWatcher.OnDeleted database entry.", ex);
                        }
                        break;
                    case WatcherChangeTypes.Renamed:
                    case WatcherChangeTypes.All:
                    default:
                        break;
                }
            }
        }

        private static string[] GetFiles()
        {
            return IncludeSubFolders
                ? Directory.GetFiles(FolderToWatch, Filter, SearchOption.AllDirectories)
                : Directory.GetFiles(FolderToWatch, Filter, SearchOption.TopDirectoryOnly);
        }

        private Task[] GetTasks(string evt)
        {
            List<Task> tasks = [];

            if (!string.IsNullOrEmpty(evt))
            {
                var ids = evt.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    var taskId = int.Parse(id.Trim());
                    var task = Workflow.Tasks.First(t => t.Id == taskId);
                    tasks.Add(task);
                }
            }

            return [.. tasks];
        }

        private void ClearFiles()
        {
            foreach (var task in Workflow.Tasks)
            {
                task.Files.Clear();
            }
        }
    }
}
