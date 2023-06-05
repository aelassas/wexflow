using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Tar
{
    public class Tar : Task
    {
        public string TarFileName { get; private set; }

        public Tar(XElement xe, Workflow wf) : base(xe, wf)
        {
            TarFileName = GetSetting("tarFileName");
        }

        public override TaskStatus Run()
        {
            Info("Creating tar archive...");

            bool success;
            try
            {
                success = CreateTar();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating tar.", e);
                success = false;
            }

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private bool CreateTar()
        {
            bool success = true;
            FileInf[] files = SelectFiles();
            if (files.Length > 0)
            {
                string tarPath = Path.Combine(Workflow.WorkflowTempFolder, TarFileName);

                try
                {
                    using TarOutputStream tar = new(File.Create(tarPath), Encoding.UTF8);

                    foreach (FileInf file in files)
                    {
                        using (Stream inputStream = File.OpenRead(file.Path))
                        {
                            long fileSize = inputStream.Length;

                            // Create a tar entry named as appropriate. You can set the name to anything,
                            // but avoid names starting with drive or UNC.
                            TarEntry entry = TarEntry.CreateTarEntry(file.RenameToOrName);

                            // Must set size, otherwise TarOutputStream will fail when output exceeds.
                            entry.Size = fileSize;

                            // Add the entry to the tar stream, before writing the data.
                            tar.PutNextEntry(entry);

                            byte[] localBuffer = new byte[32 * 1024];
                            while (true)
                            {
                                int numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);
                                if (numRead <= 0)
                                {
                                    break;
                                }
                                tar.Write(localBuffer, 0, numRead);
                            }
                        }

                        tar.CloseEntry();
                    }

                    // Finish/Close arent needed strictly as the using statement does this automatically

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    tar.Finish();

                    // Close is important to wrap things up and unlock the file.
                    tar.Close();

                    InfoFormat("Tar {0} created.", tarPath);
                    Files.Add(new FileInf(tarPath, Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the Tar {0}", e, tarPath);
                    success = false;
                }
            }
            return success;
        }

    }
}
