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
        public string DestDir { get; }

        public Untar(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
        }

        public override TaskStatus Run()
        {
            Info("Extracting TAR archives...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = UntarFiles(ref atLeastOneSuccess);
            }
            catch (ThreadInterruptedException)
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
                            , $"{Path.GetFileNameWithoutExtension(tar.Path)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
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
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while extracting of the TAR {0}: {1}", tar.Path, e.Message);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
                }
            }

            return success;
        }

        private static void ExtractTarByEntry(string tarFileName, string targetDir)
        {
            using FileStream fsIn = new(tarFileName, FileMode.Open, FileAccess.Read);

            TarInputStream tarIn = new(fsIn, Encoding.UTF8);
            while (tarIn.GetNextEntry() is { } tarEntry)
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
                    name = name[Path.GetPathRoot(name)!.Length..];
                }

                // Apply further name transformations here as necessary
                var outName = Path.Combine(targetDir, name);

                var directoryName = Path.GetDirectoryName(outName);
                _ = Directory.CreateDirectory(directoryName ?? throw new InvalidOperationException());

                FileStream outStr = new(outName, FileMode.Create);

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
