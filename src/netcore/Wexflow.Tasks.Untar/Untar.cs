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

        public Untar(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
        }

        public override TaskStatus Run()
        {
            Info("Extracting TAR archives...");

            bool success;
            bool atLeastOneSuccess = false;
            try
            {
                success = UntarFiles(ref atLeastOneSuccess);
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

            Status status = Status.Success;

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
            bool success = true;
            FileInf[] tars = SelectFiles();

            if (tars.Length > 0)
            {
                foreach (FileInf tar in tars)
                {
                    try
                    {
                        string destFolder = Path.Combine(DestDir
                            , Path.GetFileNameWithoutExtension(tar.Path) + "_" + string.Format("{0:yyyy-MM-dd-HH-mm-ss-fff}", DateTime.Now));
                        Directory.CreateDirectory(destFolder);
                        ExtractTarByEntry(tar.Path, destFolder);

                        foreach (string file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("TAR {0} extracted to {1}", tar.Path, destFolder);

                        if (!atLeastOneSuccess) atLeastOneSuccess = true;
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

        private static void ExtractTarByEntry(string tarFileName, string targetDir)
        {
            using FileStream fsIn = new(tarFileName, FileMode.Open, FileAccess.Read);

            TarInputStream tarIn = new(fsIn, Encoding.UTF8);
            TarEntry tarEntry;
            while ((tarEntry = tarIn.GetNextEntry()) != null)
            {
                if (tarEntry.IsDirectory)
                {
                    continue;
                }
                // Converts the unix forward slashes in the filenames to windows backslashes
                //
                string name = tarEntry.Name.Replace('/', Path.DirectorySeparatorChar);

                // Remove any root e.g. '\' because a PathRooted filename defeats Path.Combine
                if (Path.IsPathRooted(name))
                {
                    name = name[Path.GetPathRoot(name).Length..];
                }

                // Apply further name transformations here as necessary
                string outName = Path.Combine(targetDir, name);

                string directoryName = Path.GetDirectoryName(outName);
                Directory.CreateDirectory(directoryName);

                FileStream outStr = new(outName, FileMode.Create);

                tarIn.CopyEntryContents(outStr);

                outStr.Close();
                // Set the modification date/time. This approach seems to solve timezone issues.
                DateTime myDt = DateTime.SpecifyKind(tarEntry.ModTime, DateTimeKind.Utc);
                File.SetLastWriteTime(outName, myDt);
            }
            tarIn.Close();
        }
    }
}
