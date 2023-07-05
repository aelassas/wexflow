using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.CsvToXml
{
    public class CsvToXml : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public CsvToXml(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Creating XML files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CreateXmls(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = CreateXmls(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating XMLs.", e);
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

        private bool CreateXmls(ref bool atLeastOneSuccess)
        {
            var success = true;
            foreach (var file in SelectFiles())
            {
                try
                {
                    var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");
                    CreateXml(file.Path, xmlPath);
                    Files.Add(new FileInf(xmlPath, Id));
                    InfoFormat("XML file {0} created from {1}", xmlPath, file.Path);
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
                    ErrorFormat("An error occured while creating the XML from {0} Please check this XML file according to the documentation of the task. Error: {1}", file.Path, e.Message);
                    success = false;
                }
            }
            return success;
        }

        private void CreateXml(string csvPath, string xmlPath)
        {
            var xdoc = new XDocument(new XElement("Lines"));

            foreach (var line in File.ReadAllLines(csvPath))
            {
                var xLine = new XElement("Line");
                foreach (var col in line.Split(';'))
                {
                    if (!string.IsNullOrEmpty(col))
                    {
                        xLine.Add(new XElement("Column", col));
                    }
                }
                if (xdoc.Root == null)
                {
                    throw new Exception("No root node found.");
                }

                xdoc.Root.Add(xLine);
            }

            xdoc.Save(xmlPath);
        }
    }
}
