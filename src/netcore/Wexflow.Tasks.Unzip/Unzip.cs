using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Unzip
{
    public class Unzip : Task
    {
        public string DestDir { get; }
        public bool CreateSubDirectoryWithDateTime { get; }
        public bool Overwrite { get; }

        public Unzip(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
            CreateSubDirectoryWithDateTime = GetSettingBool("createSubDirectoryWithDateTime", true);
            Overwrite = GetSettingBool("overwrite", false);
        }

        public override TaskStatus Run()
        {
            Info("Extracting ZIP archives...");

            var success = true;
            var atLeastOneSucceed = false;

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

                        ZipFile.ExtractToDirectory(zip.Path, destFolder, Overwrite);

                        foreach (var file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("ZIP {0} extracted to {1}", zip.Path, destFolder);

                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while extracting of the ZIP {0}", e, zip.Path);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
                }
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
