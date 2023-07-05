using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesLoader
{
    public class FilesLoader : Task
    {
        public string[] Folders { get; }
        public string[] FlFiles { get; }
        public string RegexPattern { get; }
        public bool Recursive { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesLoader(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folders = GetSettings("folder");
            FlFiles = GetSettings("file");
            RegexPattern = GetSetting("regexPattern", "");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Loading files...");

            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = LoadFiles();
                    }
                }
                else
                {
                    success = LoadFiles();
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

        private bool LoadFiles()
        {
            var success = true;
            if (Recursive)
            {
                foreach (var folder in Folders)
                {
                    var files = GetFilesRecursive(folder);

                    foreach (var file in files)
                    {
                        if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                        {
                            var fi = new FileInf(file, Id);
                            Files.Add(fi);
                            InfoFormat("File loaded: {0}", file);
                        }
                    }
                }
            }
            else
            {
                foreach (var folder in Folders)
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        if (string.IsNullOrEmpty(RegexPattern) || Regex.IsMatch(file, RegexPattern))
                        {
                            var fi = new FileInf(file, Id);
                            Files.Add(fi);
                            InfoFormat("File loaded: {0}", file);
                        }
                    }
                }
            }

            foreach (var file in FlFiles)
            {
                if (File.Exists(file))
                {
                    Files.Add(new FileInf(file, Id));
                    InfoFormat("File loaded: {0}", file);
                }
                else
                {
                    ErrorFormat("File not found: {0}", file);
                    success = false;
                }
            }

            return success;
        }

        private string[] GetFilesRecursive(string dir)
        {
            return Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
        }
    }
}
