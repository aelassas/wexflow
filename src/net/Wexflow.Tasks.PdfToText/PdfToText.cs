using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;
using Path = System.IO.Path;

namespace Wexflow.Tasks.PdfToText
{
    public class PdfToText : Task
    {
        public PdfToText(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Extract TEXT from PDF files ...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (var pdfFile in files)
                {
                    try
                    {
                        var textPath = Path.Combine(Workflow.WorkflowTempFolder,
                            $"{Path.GetFileNameWithoutExtension(pdfFile.FileName)}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.txt");

                        //Document doc = new Document();
                        using (var doc = new StreamWriter(textPath))
                        {
                            var pdfReader = new PdfReader(pdfFile.Path);

                            // Add the text file contents
                            for (var page = 1; page <= pdfReader.NumberOfPages; page++)
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                                currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                                doc.Write(currentText);
                            }
                            pdfReader.Close();
                            Files.Add(new FileInf(textPath, Id));
                            InfoFormat("Textfile {0} generated from the file {1}", textPath, pdfFile.Path);
                        }
                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while converting the file {0}", e, pdfFile.Path);
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