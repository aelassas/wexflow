using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesRenamer
{
    public class FilesRenamer : Task
    {
        public bool Overwrite { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesRenamer(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Renaming files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = RenameFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = RenameFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while renaming files.", e);
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
            return new TaskStatus(status, false);
        }

        private bool RenameFiles(ref bool atLeastOneSucceed)
        {
            var success = true;
            foreach (var file in SelectFiles())
            {
                try
                {
                    if (!string.IsNullOrEmpty(file.RenameTo))
                    {
                        var dirName = Path.GetDirectoryName(file.Path) ?? throw new Exception("File directory is null");
                        var destPath = Path.Combine(dirName, file.RenameTo);

                        if (File.Exists(destPath))
                        {
                            if (Overwrite)
                            {
                                if (file.Path != destPath)
                                {
                                    File.Delete(destPath);
                                }
                                else
                                {
                                    InfoFormat("The file {0} and its new name {1} are the same.", file.Path, file.RenameTo);
                                    continue;
                                }
                            }
                            else
                            {
                                ErrorFormat("The destination file {0} already exists.", destPath);
                                success = false;
                                continue;
                            }
                        }

                        File.Move(file.Path, destPath);
                        InfoFormat("File {0} renamed to {1}", file.Path, file.RenameTo);
                        file.Path = destPath;
                        file.RenameTo = string.Empty;
                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }

                        Files.Add(new FileInf(destPath, file.TaskId));
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while renaming the file {0} to {1}. Error: {2}", file.Path, file.RenameTo, e.Message);
                    success = false;
                }
            }
            return success;
        }
    }
}