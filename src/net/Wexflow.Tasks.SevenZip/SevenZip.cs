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

            bool success = true;

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

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool CreateZip()
        {
            bool success = true;
            FileInf[] files = SelectFiles();
            if (files.Length > 0)
            {
                string sevenZipPath = Path.Combine(Workflow.WorkflowTempFolder, ZipFileName);

                try
                {
                    Assembly assembly = Assembly.GetEntryAssembly();
                    string libraryPath = Path.GetDirectoryName(assembly.Location);
#if DEBUG
                    ProcessorArchitecture processorArch = assembly.GetName().ProcessorArchitecture;
                    bool x86 = processorArch == ProcessorArchitecture.X86;
                    libraryPath = Path.Combine(libraryPath, x86 ? "x86" : "x64", "7z.dll");
#else
                    libraryPath = Path.Combine(libraryPath, "7z.dll");
#endif

                    SevenZipBase.SetLibraryPath(libraryPath);
                    SevenZipCompressor sevenZipCompressor = new SevenZipCompressor
                    {
                        CompressionLevel = CompressionLevel.Ultra,
                        CompressionMethod = CompressionMethod.Lzma
                    };
                    string[] filesParam = files.Select(f => f.Path).ToArray();
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
