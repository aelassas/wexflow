using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.XmlToCsv
{
    public class XmlToCsv : Task
    {
        public string Separator { get; set; }
        public string Quote { get; set; }
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public XmlToCsv(XElement xe, Workflow wf) : base(xe, wf)
        {
            Separator = GetSetting("separator", ";");
            Quote = GetSetting("quote", string.Empty);
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Creating csv files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = CreateCsvs(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = CreateCsvs(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating CSVs.", e);
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

        private bool CreateCsvs(ref bool atLeastOneSuccess)
        {
            var success = true;

            foreach (var file in SelectFiles())
            {
                try
                {
                    var csvPath = Path.Combine(Workflow.WorkflowTempFolder,
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.csv");
                    CreateCsv(file.Path, csvPath);
                    InfoFormat("Csv file {0} created from {1}", csvPath, file.Path);
                    Files.Add(new FileInf(csvPath, Id));

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
                    ErrorFormat("An error occured while creating the Csv from the file {0}.", e, file.Path);
                    success = false;
                }
            }

            return success;
        }

        private void CreateCsv(string xmlPath, string csvPath)
        {
            var xdoc = XDocument.Load(xmlPath);

            using (var sw = new StreamWriter(csvPath))
            {
                foreach (var xLine in xdoc.XPathSelectElements("Lines/Line"))
                {
                    foreach (var xColumn in xLine.XPathSelectElements("Column"))
                    {
                        sw.Write(string.Concat(Quote, xColumn.Value, Quote, Separator));
                    }
                    sw.Write("\r\n");
                }
            }
        }
    }
}
