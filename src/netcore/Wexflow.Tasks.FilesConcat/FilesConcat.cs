using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesConcat
{
    public class FilesConcat : Task
    {
        public FilesConcat(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Concatenating files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            FileInf[] files = SelectFiles();

            if (files.Length > 0)
            {
                StringBuilder builder = new();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInf file = files[i];
                    builder.Append(Path.GetFileNameWithoutExtension(file.FileName));
                    if (i < files.Length - 1)
                    {
                        builder.Append("_");
                    }
                }

                string concatPath = Path.Combine(Workflow.WorkflowTempFolder, builder.ToString());

                if (File.Exists(concatPath))
                {
                    File.Delete(concatPath);
                }

                using (FileStream output = File.Create(concatPath))
                {
                    foreach (FileInf file in files)
                    {
                        try
                        {
                            using (FileStream input = File.OpenRead(file.Path))
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

            Status status = Status.Success;

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