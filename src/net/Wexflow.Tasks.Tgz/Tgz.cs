using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Tgz
{
    public class Tgz : Task
    {
        public string TgzFileName { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Tgz(XElement xe, Workflow wf) : base(xe, wf)
        {
            TgzFileName = GetSetting("tgzFileName");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Creating tgz archive...");

            bool success = true;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CreateTgz();
                    }
                }
                else
                {
                    success = CreateTgz();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating tgz.", e);
                success = false;
            }

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool CreateTgz()
        {
            bool success = true;
            FileInf[] files = SelectFiles();
            if (files.Length > 0)
            {
                string tgzPath = Path.Combine(Workflow.WorkflowTempFolder, TgzFileName);

                try
                {
                    using (GZipOutputStream gz = new GZipOutputStream(File.Create(tgzPath)))
                    using (TarOutputStream tar = new TarOutputStream(gz, Encoding.UTF8))
                    {
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
                        tar.Close();

                        // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                        // the created file would be invalid.
                        gz.Finish();

                        // Close is important to wrap things up and unlock the file.
                        gz.Close();

                        InfoFormat("Tgz {0} created.", tgzPath);
                        Files.Add(new FileInf(tgzPath, Id));
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the Tar {0}", e, tgzPath);
                    success = false;
                }
            }
            return success;
        }

    }
}
