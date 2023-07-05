using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Unzip
{
    public class Unzip : Task
    {
        public string DestDir { get; }
        public string Password { get; }
        public bool CreateSubDirectoryWithDateTime { get; }
        public bool Overwrite { get; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public Unzip(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
            Password = GetSetting("password", string.Empty);
            CreateSubDirectoryWithDateTime = GetSettingBool("createSubDirectoryWithDateTime", true);
            Overwrite = GetSettingBool("overwrite", false);
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Extracting ZIP archives...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = UnzipFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = UnzipFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while extracting archives.", e);
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

        private bool UnzipFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var zips = SelectFiles();

            if (zips.Length > 0)
            {
                foreach (var zip in zips)
                {
                    try
                    {
                        var destFolder = CreateSubDirectoryWithDateTime
                            ? Path.Combine(DestDir,
                                Path.GetFileNameWithoutExtension(zip.Path) + "_" +
                                $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}")
                            : DestDir;
                        if (!Directory.Exists(destFolder))
                        {
                            _ = Directory.CreateDirectory(destFolder);
                        }

                        ExtractZipFile(zip.Path, Password, destFolder, Overwrite);

                        foreach (var file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("ZIP {0} extracted to {1}", zip.Path, destFolder);

                        if (!atLeastOneSuccess)
                        {
                            atLeastOneSuccess = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while extracting of the ZIP {0}", e, zip.Path);
                        success = false;
                    }
                }
            }

            return success;
        }

        private void ExtractZipFile(string archiveFilenameIn, string password, string outFolder, bool overwrite)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!string.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    var entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];     // 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        _ = Directory.CreateDirectory(directoryName);
                    }

                    if (overwrite && File.Exists(fullZipToPath))
                    {
                        File.Delete(fullZipToPath);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}
