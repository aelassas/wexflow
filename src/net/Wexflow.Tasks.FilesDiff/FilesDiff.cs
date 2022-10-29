using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesDiff
{
    public class FilesDiff : Task
    {
        public string OldFile { get; set; }
        public string NewFile { get; set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public FilesDiff(XElement xe, Workflow wf) : base(xe, wf)
        {
            OldFile = GetSetting("oldFile");
            NewFile = GetSetting("newFile");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Checking...");

            TaskStatus ts;
            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        CheckFiles();
                    }
                }
                else
                {
                    CheckFiles();
                }
                ts = new TaskStatus(Status.Success);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the files: {0}", e.Message);
                ts = new TaskStatus(Status.Error);
            }

            Info("Task finished.");
            return ts;

        }

        private void CheckFiles()
        {
            string oldText = File.ReadAllText(OldFile);
            string newText = File.ReadAllText(NewFile);

            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(oldText, newText);

            var resultPath = Path.Combine(Workflow.WorkflowTempFolder,
                   string.Format("FilesDiff_{0:yyyy-MM-dd-HH-mm-ss-fff}.diff", DateTime.Now));

            using (StreamWriter sw = new StreamWriter(resultPath))
            {
                foreach (var line in diff.Lines)
                {
                    switch (line.Type)
                    {
                        case ChangeType.Inserted:
                            sw.Write("+ ");
                            break;
                        case ChangeType.Deleted:
                            sw.Write("- ");
                            break;
                        case ChangeType.Modified:
                            sw.Write("~ ");
                            break;
                        default:
                            sw.Write("  ");
                            break;
                    }

                    sw.WriteLine(line.Text);
                }
            }

            Files.Add(new FileInf(resultPath, Id));
            InfoFormat("The diff of the old file '{0}' and the new file '{1}' was calculated with success.", Path.GetFileName(OldFile), Path.GetFileName(NewFile));

        }
    }
}
