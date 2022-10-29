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
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

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
            Status status = Status.Success;
            var success = true;

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
            var success = true;
            try
            {
                var files = Directory.GetFiles(SrcDir, "*.*", SearchOption.AllDirectories);
                CDBuilder builder = new CDBuilder();
                builder.UseJoliet = true;
                builder.VolumeIdentifier = VolumeIdentifier;

                foreach (var file in files)
                {
                    var fileIsoPath = file.Replace(SrcDir, string.Empty).TrimStart('\\');
                    builder.AddFile(fileIsoPath, file);
                }

                var isoPath = Path.Combine(Workflow.WorkflowTempFolder, IsoFileName);
                builder.Build(isoPath);

                Files.Add(new FileInf(isoPath, Id));
                InfoFormat("Iso {0} created with success.", isoPath);

                success = true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating {0}: {1}", IsoFileName, e.Message);
            }
            return success;
        }
    }
}
