using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Untgz
{
    public class Untgz : Task
    {
        public string DestDir { get; }

        public Untgz(XElement xe, Workflow wf) : base(xe, wf)
        {
            DestDir = GetSetting("destDir");
        }

        public override TaskStatus Run()
        {
            Info("Extracting TAR.GZ archives...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = ExtractFiles(ref atLeastOneSuccess);
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

        private bool ExtractFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var tgzs = SelectFiles();

            if (tgzs.Length > 0)
            {
                foreach (var tgz in tgzs)
                {
                    try
                    {
                        var destFolder = Path.Combine(DestDir
                            , $"{Path.GetFileNameWithoutExtension(tgz.Path)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}");
                        _ = Directory.CreateDirectory(destFolder);
                        ExtractTgz(tgz.Path, destFolder);

                        foreach (var file in Directory.GetFiles(destFolder, "*.*", SearchOption.AllDirectories))
                        {
                            Files.Add(new FileInf(file, Id));
                        }

                        InfoFormat("TAR.GZ {0} extracted to {1}", tgz.Path, destFolder);

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
                        ErrorFormat("An error occured while extracting of the TAR.GZ {0}", e, tgz.Path);
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

        private static void ExtractTgz(string gzArchiveName, string destFolder)
        {
            var inStream = File.OpenRead(gzArchiveName);
            var gzipStream = new GZipInputStream(inStream);

            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }
    }
}