using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesCopier
{
    public class FilesCopier : Task
    {
        public string DestFolder { get; private set; }
        public bool Overwrite { get; private set; }
        public string PreserveFolderStructFrom { get; private set; }
        public bool AllowCreateDirectory { get; private set; }

        public FilesCopier(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            DestFolder = GetSetting("destFolder");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
            PreserveFolderStructFrom = GetSetting("preserveFolderStructFrom");
            AllowCreateDirectory = GetSettingBool("allowCreateDirectory", true);
        }

        public override TaskStatus Run()
        {
            Info("Copying files...");

            bool success = true;
            bool atLeastOneSucceed = false;
            var files = SelectFiles();

            foreach (FileInf file in files)
            {
                string destPath;
                if (!string.IsNullOrWhiteSpace(PreserveFolderStructFrom) &&
                    file.Path.StartsWith(PreserveFolderStructFrom, StringComparison.InvariantCultureIgnoreCase))
                {
                    var preservedFolderStruct = Path.GetDirectoryName(file.Path);
                    preservedFolderStruct = preservedFolderStruct.Length > PreserveFolderStructFrom.Length
                        ? preservedFolderStruct.Remove(0, PreserveFolderStructFrom.Length)
                        : string.Empty;

                    if (preservedFolderStruct.StartsWith(Path.DirectorySeparatorChar))
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
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    }

                    File.Copy(file.Path, destPath, Overwrite);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("File copied: {0} -> {1}", file.Path, destPath);
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
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

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
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
    }
}
