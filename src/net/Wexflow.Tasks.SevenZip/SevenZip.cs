using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.SevenZip
{
    public class SevenZip : Task
    {
        public string ZipFileName { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public SevenZip(XElement xe, Workflow wf) : base(xe, wf)
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

            var success = true;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CreateZip();
                    }
                }
                else
                {
                    success = CreateZip();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating 7z.", e);
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

        private bool CreateZip()
        {
            var success = true;
            var files = SelectFiles();
            if (files.Length > 0)
            {
                var sevenZipPath = Path.Combine(Workflow.WorkflowTempFolder, ZipFileName);

                try
                {
                    var assembly = Assembly.GetEntryAssembly();
                    var libraryPath = Path.GetDirectoryName(assembly.Location);
#if DEBUG
                    var processorArch = assembly.GetName().ProcessorArchitecture;
                    var x86 = processorArch == ProcessorArchitecture.X86;
                    libraryPath = Path.Combine(libraryPath, x86 ? "x86" : "x64", "7z.dll");
#else
                    libraryPath = Path.Combine(libraryPath, "7z.dll");
#endif

                    SevenZipBase.SetLibraryPath(libraryPath);
                    SevenZipCompressor sevenZipCompressor = new SevenZipCompressor();
                    sevenZipCompressor.CompressionLevel = CompressionLevel.Ultra;
                    sevenZipCompressor.CompressionMethod = CompressionMethod.Lzma;
                    var filesParam = files.Select(f => f.Path).ToArray();
                    sevenZipCompressor.CompressFiles(sevenZipPath, filesParam);
                    Files.Add(new FileInf(sevenZipPath, Id));
                    InfoFormat("7Z {0} created with success.", sevenZipPath);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the 7Z {0}", e, sevenZipPath);
                    success = false;
                }
            }
            return success;
        }
    }
}
