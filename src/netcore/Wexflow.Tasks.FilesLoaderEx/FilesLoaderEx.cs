using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesLoaderEx
{
    public class FilesLoaderEx : Task
    {
        public string[] Folders { get; }
        public string[] FlFiles { get; }
        public string RegexPattern { get; }
        public bool Recursive { get; }

        public int AddMaxCreateDate { get; }
        public int AddMinCreateDate { get; }
        public int AddMaxModifyDate { get; }
        public int AddMinModifyDate { get; }

        public int RemoveMaxCreateDate { get; }
        public int RemoveMinCreateDate { get; }
        public int RemoveMaxModifyDate { get; }
        public int RemoveMinModifyDate { get; }

        public FilesLoaderEx(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folders = GetSettings("folder");
            FlFiles = GetSettings("file");
            RegexPattern = GetSetting("regexPattern", "");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
            AddMaxCreateDate = GetSettingInt("addMaxCreateDate", 0);
            AddMinCreateDate = GetSettingInt("addMinCreateDate", 0);
            AddMaxModifyDate = GetSettingInt("addMaxModifyDate", 0);
            AddMinModifyDate = GetSettingInt("addMinModifyDate", 0);
            RemoveMaxCreateDate = GetSettingInt("removeMaxCreateDate", 0);
            RemoveMinCreateDate = GetSettingInt("removeMinCreateDate", 0);
            RemoveMaxModifyDate = GetSettingInt("removeMaxModifyDate", 0);
            RemoveMinModifyDate = GetSettingInt("removeMinModifyDate", 0);
        }

        public override TaskStatus Run()
        {
            Info("Loading files...");

            var success = true;
            List<FileInf> folderFiles = [];

            try
            {
                #region Copy of FileLoader task

                if (Recursive)
                {
                    foreach (var folder in Folders)
                    {
                        var files = GetFilesRecursive(folder);

                        foreach (var file in files)
                        {
                            if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                            {
                                FileInf fi = new(file, Id);
                                folderFiles.Add(fi);
                            }
                            WaitOne();
                        }
                    }
                }
                else
                {
                    foreach (var folder in Folders)
                    {
                        foreach (var file in Directory.GetFiles(folder).OrderBy(f => f))
                        {
                            if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                            {
                                FileInf fi = new(file, Id);
                                folderFiles.Add(fi);
                            }
                            WaitOne();
                        }
                    }
                }

                foreach (var file in FlFiles)
                {
                    if (File.Exists(file))
                    {
                        folderFiles.Add(new FileInf(file, Id));
                    }
                    else
                    {
                        ErrorFormat("File not found: {0}", file);
                        success = false;
                    }
                }

                #endregion

                if (AddMinModifyDate + AddMaxModifyDate + AddMaxCreateDate + AddMinCreateDate > 0)
                {
                    AddFiles(folderFiles.OrderBy(f => f.FileInfo.CreationTime).Take(AddMinCreateDate));
                    AddFiles(folderFiles.OrderBy(f => f.FileInfo.CreationTime).TakeLast(AddMaxCreateDate));
                    AddFiles(folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).Take(AddMinModifyDate));
                    AddFiles(folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).TakeLast(AddMaxModifyDate));
                }

                if (RemoveMaxCreateDate + RemoveMaxModifyDate + RemoveMinCreateDate + RemoveMinModifyDate > 0)
                {
                    List<FileInf> tmpFiles = new(folderFiles);
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.CreationTime).Take(RemoveMinCreateDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.CreationTime).TakeLast(RemoveMaxCreateDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).Take(RemoveMinModifyDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).TakeLast(RemoveMaxModifyDate));
                    AddFiles(tmpFiles);
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while loading files.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private void AddFiles(IEnumerable<FileInf> files)
        {
            foreach (var file in files)
            {
                Files.Add(file);
                InfoFormat("File loaded: {0}", file);
            }
        }

        private static string[] GetFilesRecursive(string dir)
        {
            return [.. Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).OrderBy(f => f)];
        }

        private static void RemoveRange(List<FileInf> items, IEnumerable<FileInf> remove)
        {
            foreach (var r in remove)
            {
                _ = items.Remove(r);
            }
        }
    }
}
