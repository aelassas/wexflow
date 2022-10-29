using System;
using System.Collections.Generic;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wexflow.Tasks.FilesLoaderEx
{
    public class FilesLoaderEx : Task
    {
        public string[] Folders { get; private set; }
        public string[] FlFiles { get; private set; }
        public string RegexPattern { get; private set; }
        public bool Recursive { get; private set; }

        public int AddMaxCreateDate { get; private set; }
        public int AddMinCreateDate { get; private set; }
        public int AddMaxModifyDate { get; private set; }
        public int AddMinModifyDate { get; private set; }

        public int RemoveMaxCreateDate { get; private set; }
        public int RemoveMinCreateDate { get; private set; }
        public int RemoveMaxModifyDate { get; private set; }
        public int RemoveMinModifyDate { get; private set; }

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

            bool success = true;
            var folderFiles = new List<FileInf>();

            try
            {
                #region Copy of FileLoader task

                if (Recursive)
                {
                    foreach (string folder in Folders)
                    {
                        var files = GetFilesRecursive(folder);

                        foreach (var file in files)
                        {
                            if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                            {
                                var fi = new FileInf(file, Id);
                                folderFiles.Add(fi);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string folder in Folders)
                    {
                        foreach (string file in Directory.GetFiles(folder).OrderBy(f => f))
                        {
                            if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                            {
                                var fi = new FileInf(file, Id);
                                folderFiles.Add(fi);
                            }
                        }
                    }
                }

                foreach (string file in FlFiles)
                {
                    if (File.Exists(file))
                        folderFiles.Add(new FileInf(file, Id));
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
                    var tmpFiles = new List<FileInf>(folderFiles);
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.CreationTime).Take(RemoveMinCreateDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.CreationTime).TakeLast(RemoveMaxCreateDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).Take(RemoveMinModifyDate));
                    RemoveRange(tmpFiles, folderFiles.OrderBy(f => f.FileInfo.LastWriteTime).TakeLast(RemoveMaxModifyDate));
                    AddFiles(tmpFiles);
                }
            }
            catch (ThreadAbortException)
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

        private string[] GetFilesRecursive(string dir)
        {
            return Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
        }

        private void RemoveRange(List<FileInf> items, IEnumerable<FileInf> remove)
        {
            foreach (var r in remove)
                items.Remove(r);
        }
    }

    public static class MiscExtensions
    {
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
    }
}
