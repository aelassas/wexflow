using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using IO = System.IO;

namespace Wexflow.Tasks.FileSystemWatcher
{
    public class FileSystemWatcher : Task
    {
        public IO.FileSystemWatcher Watcher { get; private set; }
        public static string FolderToWatch { get; private set; }
        public static string Filter { get; private set; }
        public static bool IncludeSubFolders { get; private set; }
        public static string OnFileFound { get; private set; }
        public static string OnFileCreated { get; private set; }
        public static string OnFileChanged { get; private set; }
        public static string OnFileDeleted { get; private set; }
        public static bool SafeMode { get; private set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }
        public static List<string> CurrentLogs { get; private set; }

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
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
            CurrentLogs = new List<string>();
        }

        public override TaskStatus Run()
        {
            InfoFormat("Watching the folder {0} ...", FolderToWatch);

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        if (!Directory.Exists(FolderToWatch))
                        {
                            ErrorFormat("The folder {0} does not exist.", FolderToWatch);
                            return new TaskStatus(Status.Error);
                        }

                        InitFileSystemWatcher();
                    }
                }
                else
                {
                    if (!Directory.Exists(FolderToWatch))
                    {
                        ErrorFormat("The folder {0} does not exist.", FolderToWatch);
                        return new TaskStatus(Status.Error);
                    }

                    InitFileSystemWatcher();
                }
            }
            catch (ThreadAbortException)
            {
                if (Watcher != null)
                {
                    Watcher.EnableRaisingEvents = false;
                    Watcher.Dispose();
                }
                throw;
            }
            catch (Exception e)
            {
                if (Watcher != null)
                {
                    Watcher.EnableRaisingEvents = false;
                    Watcher.Dispose();
                }
                ErrorFormat("An error occured while initializing FileSystemWatcher.", e);
            }

            Info("Task finished");
            return new TaskStatus(Status.Success);
        }

        private void InitFileSystemWatcher()
        {
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
                Info("FileSystemWatcher.OnFound finished.");
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
            Info("Checking existing files finished.");

            Info("Initializing FileSystemWatcher...");
            Watcher = new IO.FileSystemWatcher
            {
                Path = FolderToWatch,
                Filter = Filter,
                IncludeSubdirectories = IncludeSubFolders
            };

            // Add event handlers.
            Watcher.Created += OnCreated;
            Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
            InfoFormat("FileSystemWatcher.Path={0}", Watcher.Path);
            InfoFormat("FileSystemWatcher.Filter={0}", Watcher.Filter);
            InfoFormat("FileSystemWatcher.EnableRaisingEvents={0}", Watcher.EnableRaisingEvents);
            Info("FileSystemWatcher Initialized.");

            Info("Begin watching ...");
            CurrentLogs.AddRange(Logs);
            while (true)
            {
                Thread.Sleep(1);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(200);

                if (File.Exists(e.FullPath) && !IsDirectory(e.FullPath))
                {
                    Info("FileSystemWatcher.OnCreated started.");
                    if (SafeMode && WexflowEngine.IsFileLocked(e.FullPath))
                    {
                        Info($"File lock detected on file {e.FullPath}");

                        while (WexflowEngine.IsFileLocked(e.FullPath))
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    try
                    {
                        ClearFiles();
                        Files.Add(new FileInf(e.FullPath, Id));
                        var tasks = GetTasks(OnFileCreated);
                        foreach (var task in tasks)
                        {
                            task.Logs.Clear();
                            _ = task.Run();
                            CurrentLogs.AddRange(task.Logs);
                        }
                        _ = Files.RemoveAll(f => f.Path == e.FullPath);
                    }
                    catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                    {
                        Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while triggering FileSystemWatcher.OnCreated on the file {0}. Message: {1}", e.FullPath, ex.Message);
                    }
                    Info("FileSystemWatcher.OnCreated finished.");

                    try
                    {
                        var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                        entry.Logs = string.Join("\r\n", CurrentLogs);
                        Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while updating FileSystemWatcher.OnCreated database entry.", ex);
                    }
                }
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
            {
                Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
            }
            catch (Exception ex)
            {
                ErrorFormat("An error while triggering FileSystemWatcher.OnCreated on the file {0}. Message: {1}", e.FullPath, ex.Message);
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(500);

                if (File.Exists(e.FullPath) && !IsDirectory(e.FullPath))
                {
                    Info("FileSystemWatcher.OnChanged started.");
                    if (SafeMode && WexflowEngine.IsFileLocked(e.FullPath))
                    {
                        Info($"File lock detected on file {e.FullPath}");

                        while (WexflowEngine.IsFileLocked(e.FullPath))
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    try
                    {
                        ClearFiles();
                        Files.Add(new FileInf(e.FullPath, Id));
                        var tasks = GetTasks(OnFileChanged);
                        foreach (var task in tasks)
                        {
                            task.Logs.Clear();
                            _ = task.Run();
                            CurrentLogs.AddRange(task.Logs);
                        }
                        _ = Files.RemoveAll(f => f.Path == e.FullPath);
                    }
                    catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                    {
                        Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while triggering FileSystemWatcher.OnChanged on the file {0}. Message: {1}", e.FullPath, ex.Message);
                    }
                    Info("FileSystemWatcher.OnChanged finished.");

                    try
                    {
                        var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                        entry.Logs = string.Join("\r\n", CurrentLogs);
                        Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
                    }
                    catch (Exception ex)
                    {
                        ErrorFormat("An error while updating FileSystemWatcher.OnChanged database entry.", ex);
                    }
                }
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
            {
                Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
            }
            catch (Exception ex)
            {
                ErrorFormat("An error while triggering FileSystemWatcher.OnChanged on the file {0}. Message: {1}", e.FullPath, ex.Message);
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            Info("FileSystemWatcher.OnDeleted started.");
            try
            {
                ClearFiles();
                Files.Add(new FileInf(e.FullPath, Id));
                var tasks = GetTasks(OnFileDeleted);
                foreach (var task in tasks)
                {
                    task.Logs.Clear();
                    _ = task.Run();
                    CurrentLogs.AddRange(task.Logs);
                }
                _ = Files.RemoveAll(f => f.Path == e.FullPath);
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
            {
                Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
            }
            catch (Exception ex)
            {
                ErrorFormat("An error while triggering FileSystemWatcher.OnDeleted on the file {0}. Message: {1}", e.FullPath, ex.Message);
            }
            Info("FileSystemWatcher.OnDeleted finished.");

            try
            {
                var entry = Workflow.Database.GetEntry(Workflow.Id, Workflow.InstanceId);
                entry.Logs = string.Join("\r\n", CurrentLogs);
                Workflow.Database.UpdateEntry(entry.GetDbId(), entry);
            }
            catch (Exception ex)
            {
                ErrorFormat("An error while updating FileSystemWatcher.OnDeleted database entry.", ex);
            }
        }

        private string[] GetFiles()
        {
            return IncludeSubFolders
                ? Directory.GetFiles(FolderToWatch, Filter, SearchOption.AllDirectories)
                : Directory.GetFiles(FolderToWatch, Filter, SearchOption.TopDirectoryOnly);
        }

        private Task[] GetTasks(string evt)
        {
            var tasks = new List<Task>();

            if (!string.IsNullOrEmpty(evt))
            {
                var ids = evt.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    var taskId = int.Parse(id.Trim());
                    var task = Workflow.Tasks.First(t => t.Id == taskId);
                    tasks.Add(task);
                }
            }

            return tasks.ToArray();
        }

        private void ClearFiles()
        {
            foreach (var task in Workflow.Tasks)
            {
                task.Files.Clear();
            }
        }

        private bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);

            var isDir = attr.HasFlag(FileAttributes.Directory);

            return isDir;
        }
    }
}
