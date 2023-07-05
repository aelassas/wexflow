using DiscUtils.Iso9660;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.IsoCreator
{
    public class IsoCreator : Task
    {
        public string SrcDir { get; set; }
        public string VolumeIdentifier { get; set; }
        public string IsoFileName { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public IsoCreator(XElement xe, Workflow wf) : base(xe, wf)
        {
            SrcDir = GetSetting("srcDir");
            VolumeIdentifier = GetSetting("volumeIdentifier");
            IsoFileName = GetSetting("isoFileName");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Creating .iso...");
            var status = Status.Success;
            bool success;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CreateIso();
                    }
                }
                else
                {
                    success = CreateIso();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating iso.", e);
                success = false;
            }

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool CreateIso()
        {
            try
            {
                var files = Directory.GetFiles(SrcDir, "*.*", SearchOption.AllDirectories);
                var builder = new CDBuilder
                {
                    UseJoliet = true,
                    VolumeIdentifier = VolumeIdentifier
                };

                foreach (var file in files)
                {
                    var fileIsoPath = file.Replace(SrcDir, string.Empty).TrimStart('\\');
                    _ = builder.AddFile(fileIsoPath, file);
                }

                var isoPath = Path.Combine(Workflow.WorkflowTempFolder, IsoFileName);
                builder.Build(isoPath);

                Files.Add(new FileInf(isoPath, Id));
                InfoFormat("Iso {0} created with success.", isoPath);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating {0}: {1}", IsoFileName, e.Message);
                return false;
            }

            return true;
        }
    }
}
