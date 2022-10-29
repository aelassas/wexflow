using System;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesConcat
{
    public class FilesConcat : Task
    {
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public FilesConcat(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Concatenating files...");

            var success = true;
            var atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ConcatFiles(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = ConcatFiles(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while concatenating the files.", e);
                success = false;
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

        private bool ConcatFiles(ref bool atLeastOneSucceed)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    builder.Append(Path.GetFileNameWithoutExtension(file.FileName));
                    if (i < files.Length - 1)
                    {
                        builder.Append("_");
                    }
                }

                var concatPath = Path.Combine(Workflow.WorkflowTempFolder, builder.ToString());

                if (File.Exists(concatPath))
                {
                    File.Delete(concatPath);
                }

                using (var output = File.Create(concatPath))
                {
                    foreach (FileInf file in files)
                    {
                        try
                        {
                            using (var input = File.OpenRead(file.Path))
                            {
                                input.CopyTo(output);
                            }

                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while concatenating the file {0}", e, file.Path);
                            success = false;
                        }
                    }
                }

                if (success)
                {
                    InfoFormat("Concatenation file generated: {0}", builder);
                }

                Files.Add(new FileInf(concatPath, Id));
            }
            return success;
        }

    }
}