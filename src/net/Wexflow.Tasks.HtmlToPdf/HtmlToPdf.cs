using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HtmlToPdf
{
    public class HtmlToPdf : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public HtmlToPdf(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Generating PDF files from HTML files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = ConvertFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = ConvertFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting files.", e);
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

        private bool ConvertFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var pdfPath = Path.Combine(Workflow.WorkflowTempFolder,
                            $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.pdf");

                        var doc = new Document();
                        _ = PdfWriter.GetInstance(doc, new FileStream(pdfPath, FileMode.Create));
#pragma warning disable CS0612 // Le type ou le membre est obsolète
                        var worker = new HTMLWorker(doc);
#pragma warning restore CS0612 // Le type ou le membre est obsolète
                        doc.Open();
                        worker.StartDocument();
                        worker.Parse(new StreamReader(new FileStream(file.Path, FileMode.Open)));
                        worker.EndDocument();
                        worker.Close();
                        // Close the document
                        doc.Close();

                        Files.Add(new FileInf(pdfPath, Id));
                        InfoFormat("PDF {0} generated from the file {1}", pdfPath, file.Path);

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
                        ErrorFormat("An error occured while generating the PDF of the file {0}", e, file.Path);
                        success = false;
                    }
                }
            }
            return success;
        }
    }
}