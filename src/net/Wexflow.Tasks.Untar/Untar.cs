using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Untar
{
    public class Untar : Task
    {
        public string DestDir { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Untar(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Extracting TAR archives...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = UntarFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = UntarFiles(ref atLeastOneSuccess);
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

        private bool UntarFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var tars = SelectFiles();

            if (tars.Length > 0)
            {
                foreach (var tar in tars)
                {
                    try
                    {
                        var destFolder = Path.Combine(DestDir
                            , Path.GetFileNameWithoutExtension(tar.Path) + "_" + string.Format("{0:yyyy-MM-dd-HH-mm-ss-fff}", DateTime.Now));
                        _ = Directory.CreateDirectory(destFolder);
                        ExtractTarByEntry(tar.Path, destFolder);

                        foreach (var file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("TAR {0} extracted to {1}", tar.Path, destFolder);

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
                        ErrorFormat("An error occured while extracting of the TAR {0}: {1}", tar.Path, e.Message);
                        success = false;
                    }
                }
            }

            return success;
        }

        private void ExtractTarByEntry(string tarFileName, string targetDir)
        {
            using (var fsIn = new FileStream(tarFileName, FileMode.Open, FileAccess.Read))
            {
                var tarIn = new TarInputStream(fsIn, Encoding.UTF8);
                TarEntry tarEntry;
                while ((tarEntry = tarIn.GetNextEntry()) != null)
                {
                    if (tarEntry.IsDirectory)
                    {
                        continue;
                    }
                    // Converts the unix forward slashes in the filenames to windows backslashes
                    //
                    var name = tarEntry.Name.Replace('/', Path.DirectorySeparatorChar);

                    // Remove any root e.g. '\' because a PathRooted filename defeats Path.Combine
                    if (Path.IsPathRooted(name))
                    {
                        name = name.Substring(Path.GetPathRoot(name).Length);
                    }

                    // Apply further name transformations here as necessary
                    var outName = Path.Combine(targetDir, name);

                    var directoryName = Path.GetDirectoryName(outName);
                    _ = Directory.CreateDirectory(directoryName);

                    var outStr = new FileStream(outName, FileMode.Create);

                    tarIn.CopyEntryContents(outStr);

                    outStr.Close();
                    // Set the modification date/time. This approach seems to solve timezone issues.
                    var myDt = DateTime.SpecifyKind(tarEntry.ModTime, DateTimeKind.Utc);
                    File.SetLastWriteTime(outName, myDt);
                }
                tarIn.Close();
            }
        }
    }
}
