using System;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.TextToSpeech
{
    public class TextToSpeech : Task
    {
        public TextToSpeech(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Converting text to speech...");
            var status = Status.Success;
            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            foreach (var file in files)
            {
                try
                {
                    using (var synth = new SpeechSynthesizer())
                    using (var stream = new MemoryStream())
                    {
                        synth.SetOutputToWaveStream(stream);
                        var text = File.ReadAllText(file.Path);
                        synth.Speak(text);
                        var bytes = stream.GetBuffer();
                        var destFile = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(file.FileName) + ".wav");
                        File.WriteAllBytes(destFile, bytes);
                        Files.Add(new FileInf(destFile, Id));
                        InfoFormat("The file {0} was converted to speech with success -> {1}", file.Path, destFile);
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
