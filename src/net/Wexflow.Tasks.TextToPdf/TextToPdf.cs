using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Wexflow.Tasks.TextToPdf
{
    public class TextToPdf : Task
    {
        public TextToPdf(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating PDF files from TEXT files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (FileInf file in files)
                {
                    try
                    {
                        string pdfPath = Path.Combine(Workflow.WorkflowTempFolder,
                            string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.pdf", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));
                        Document doc = new Document();
                        PdfWriter.GetInstance(doc, new FileStream(pdfPath, FileMode.Create));
                        doc.Open();
                        // Add the text file contents
                        var content = File.ReadAllText(file.Path);
                        doc.Add(new Paragraph(content));
                        // Close the document
                        doc.Close();
                        Files.Add(new FileInf(pdfPath, Id));
                        InfoFormat("PDF {0} generated from the file {1}", pdfPath, file.Path);

                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while converting the file {0}", e, file.Path);
                        success = false;
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
            return new TaskStatus(status, false);
        }
    }
}