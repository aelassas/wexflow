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

        public FilesDiff(XElement xe, Workflow wf) : base(xe, wf)
        {
            OldFile = GetSetting("oldFile");
            NewFile = GetSetting("newFile");
        }

        public override TaskStatus Run()
        {
            Info("Checking...");

            TaskStatus ts;
            try
            {
                CheckFiles();
                WaitOne();
                ts = new TaskStatus(Status.Success);
            }
            catch (ThreadInterruptedException)
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
            var oldText = File.ReadAllText(OldFile);
            var newText = File.ReadAllText(NewFile);

            InlineDiffBuilder diffBuilder = new(new Differ());
            var diff = diffBuilder.BuildDiffModel(oldText, newText);

            var resultPath = Path.Combine(Workflow.WorkflowTempFolder,
                $"FilesDiff_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.diff");

            using (StreamWriter sw = new(resultPath))
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
                        case ChangeType.Unchanged:
                        case ChangeType.Imaginary:
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
