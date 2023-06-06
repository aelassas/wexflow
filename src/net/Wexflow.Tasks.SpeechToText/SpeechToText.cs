using System;
using System.Globalization;
using System.IO;
using System.Speech.Recognition;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.SpeechToText
{
    public class SpeechToText : Task
    {
        public SpeechToText(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Converting speech to text...");
            var status = Status.Success;
            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            foreach (var file in files)
            {
                try
                {
                    using (var sre = new SpeechRecognitionEngine(new CultureInfo("en-US")))
                    {
                        sre.SetInputToWaveFile(file.Path);
                        sre.LoadGrammar(new DictationGrammar());

                        sre.BabbleTimeout = new TimeSpan(int.MaxValue);
                        sre.InitialSilenceTimeout = new TimeSpan(int.MaxValue);
                        sre.EndSilenceTimeout = new TimeSpan(100000000);
                        sre.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000);

                        var result = sre.Recognize();
                        var text = result.Text;

                        var destFile = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(file.FileName) + ".txt");
                        File.WriteAllText(destFile, text);
                        Files.Add(new FileInf(destFile, Id));
                        InfoFormat("The file {0} was converted to a text file with success -> {1}", file.Path, destFile);
                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
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
