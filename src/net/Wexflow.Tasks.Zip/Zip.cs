using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Zip
{
    public class Zip : Task
    {
        public string ZipFileName { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public Zip(XElement xe, Workflow wf) : base(xe, wf)
        {
            ZipFileName = GetSetting("zipFileName");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Zipping files...");

            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ZipFiles();
                    }
                }
                else
                {
                    success = ZipFiles();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while zipping files.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool ZipFiles()
        {
            var success = true;
            var files = SelectFiles();
            if (files.Length > 0)
            {
                var zipPath = Path.Combine(Workflow.WorkflowTempFolder, ZipFileName);

                try
                {
                    using (var s = new ZipOutputStream(File.Create(zipPath)))
                    {
                        s.SetLevel(9); // 0 - store only to 9 - means best compression

                        var buffer = new byte[4096];

                        foreach (var file in files)
                        {
                            // Using GetFileName makes the result compatible with XP
                            // as the resulting path is not absolute.
                            var entry = new ZipEntry(file.RenameToOrName) { DateTime = DateTime.Now };

                            // Setup the entry data as required.

                            // Crc and size are handled by the library for seakable streams
                            // so no need to do them here.

                            // Could also use the last write time or similar for the file.
                            s.PutNextEntry(entry);

                            using (var fs = File.OpenRead(file.Path))
                            {
                                // Using a fixed size buffer here makes no noticeable difference for output
                                // but keeps a lid on memory usage.
                                int sourceBytes;
                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    s.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }
                        }

                        // Finish/Close arent needed strictly as the using statement does this automatically

                        // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                        // the created file would be invalid.
                        s.Finish();

                        // Close is important to wrap things up and unlock the file.
                        s.Close();

                        InfoFormat("Zip {0} created.", zipPath);
                        Files.Add(new FileInf(zipPath, Id));
                    }
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
            return success;
        }
    }
}
