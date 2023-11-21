using DiscUtils.Iso9660;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.IsoExtractor
{
    public class IsoExtractor : Task
    {
        public string DestDir { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public IsoExtractor(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Extracting ISO files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ExtractIsos(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = ExtractIsos(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while extracting ISOs.", e);
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

        private bool ExtractIsos(ref bool atLeastOneSuccess)
        {
            var success = true;
            var isos = SelectFiles();

            if (isos.Length > 0)
            {
                foreach (var iso in isos)
                {
                    try
                    {
                        var destFolder = Path.Combine(DestDir
                            , $"{Path.GetFileNameWithoutExtension(iso.Path)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
                        _ = Directory.CreateDirectory(destFolder);

                        ExtractIso(iso.Path, destFolder);

                        foreach (var file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("ISO {0} extracted to {1}", iso.Path, destFolder);

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
                        ErrorFormat("An error occured while extracting of the ISO {0}", e, iso.Path);
                        success = false;
                    }
                }
            }
            return success;
        }

        private void ExtractIso(string isoPath, string destDir)
        {
            using (var isoStream = File.Open(isoPath, FileMode.Open))
            {
                var cd = new CDReader(isoStream, true);
                var files = cd.GetFiles("\\", "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    using (var stream = cd.OpenFile(file, FileMode.Open))
                    {
                        var destFile = $"{destDir.TrimEnd('\\')}\\{Regex.Replace(file.Replace("/", "\\"), @";\d*$", "").TrimStart('\\')}";

                        // Create directories
                        var destFolder = Path.GetDirectoryName(destFile) ?? throw new InvalidOperationException();
                        destFolder = destFolder.Equals(destDir) ? string.Empty : destFolder;

                        var finalDestFolder = destDir;
                        if (!string.IsNullOrEmpty(destFolder))
                        {
                            finalDestFolder = Path.Combine(destDir, destFolder);
                        }

                        if (!Directory.Exists(finalDestFolder))
                        {
                            _ = Directory.CreateDirectory(finalDestFolder);
                        }

                        // Create the file
                        using (var sw = new FileStream(destFile, FileMode.CreateNew))
                        {
                            var buffer = new byte[2048];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                sw.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
        }
    }
}
