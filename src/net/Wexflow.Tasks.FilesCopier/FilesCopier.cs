using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesCopier
{
    public class FilesCopier : Task
    {
        public string DestFolder { get; }
        public bool Overwrite { get; }
        public string PreserveFolderStructFrom { get; }
        public bool AllowCreateDirectory { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesCopier(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestFolder = GetSetting("destFolder");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
            PreserveFolderStructFrom = GetSetting("preserveFolderStructFrom");
            AllowCreateDirectory = bool.Parse(GetSetting("allowCreateDirectory", "true"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Copying files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CopyFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = CopyFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while copying files.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool CopyFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var files = SelectFiles();
            foreach (var file in files)
            {
                string destPath;
                if (!string.IsNullOrWhiteSpace(PreserveFolderStructFrom) &&
                    file.Path.StartsWith(PreserveFolderStructFrom, StringComparison.InvariantCultureIgnoreCase))
                {
                    var preservedFolderStruct = Path.GetDirectoryName(file.Path) ?? throw new InvalidOperationException();
                    preservedFolderStruct = preservedFolderStruct.Length > PreserveFolderStructFrom.Length
                        ? preservedFolderStruct.Remove(0, PreserveFolderStructFrom.Length)
                        : string.Empty;

                    if (preservedFolderStruct.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        preservedFolderStruct = preservedFolderStruct.Remove(0, 1);
                    }

                    destPath = Path.Combine(DestFolder, preservedFolderStruct, file.FileName);
                }
                else
                {
                    destPath = Path.Combine(DestFolder, file.FileName);
                }

                try
                {
                    if (AllowCreateDirectory && !Directory.Exists(Path.GetDirectoryName(destPath)))
                    {
                        InfoFormat("Creating directory: {0}", Path.GetDirectoryName(destPath));
                        _ = Directory.CreateDirectory(Path.GetDirectoryName(destPath) ?? throw new InvalidOperationException());
                    }

                    File.Copy(file.Path, destPath, Overwrite);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("File copied: {0} -> {1}", file.Path, destPath);
                    if (!atLeastOneSuccess)
                    {
                        atLeastOneSuccess = true;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while copying the file {0} to {1}.", e, file.Path, destPath);
                    success = false;
                }
            }
            return success;
        }
    }
}
