using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Zip
{
    public class Zip : Task
    {
        public string ZipFileName { get; private set; }

        public Zip(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ZipFileName = GetSetting("zipFileName");
        }

        public override TaskStatus Run()
        {
            Info("Zipping files...");

            bool success = true;

            FileInf[] files = SelectFiles();
            if (files.Length > 0)
            {
                string zipPath = Path.Combine(Workflow.WorkflowTempFolder, ZipFileName);

                try
                {
                    using ZipArchive zip = new(File.Create(zipPath), ZipArchiveMode.Create, false);

                    byte[] buffer = new byte[4096];

                    foreach (FileInf file in files)
                    {
                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        ZipArchiveEntry entry = zip.CreateEntry(file.RenameToOrName);

                        using FileStream fs = File.OpenRead(file.Path);
                        using Stream entryStream = entry.Open();
                        // Using a fixed size buffer here makes no noticeable difference for output
                        // but keeps a lid on memory usage.
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            entryStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }

                    InfoFormat("Zip {0} created.", zipPath);
                    Files.Add(new FileInf(zipPath, Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the Zip {0}", e, zipPath);
                    success = false;
                }
            }

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
