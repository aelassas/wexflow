using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using MediaInfoDotNet.Models;
using MediaInfoDotNet;

namespace Wexflow.Tasks.MediaInfo
{
    public class MediaInfo : Task
    {
        private bool _success = true;
        private bool _atLeastOneSucceed = false;

        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public MediaInfo(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Generating MediaInfo informations...");


            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        InformFiles();
                    }
                }
                else
                {
                    InformFiles();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while generating MediaInfo information.", e);
                _success = false;
            }

            var status = Core.Status.Success;

            if (!_success && _atLeastOneSucceed)
            {
                status = Core.Status.Warning;
            }
            else if (!_success)
            {
                status = Core.Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private void InformFiles()
        {
            var files = SelectFiles();

            if (files.Length > 0)
            {
                var mediaInfoPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("MediaInfo_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = Inform(files);
                xdoc.Save(mediaInfoPath);
                Files.Add(new FileInf(mediaInfoPath, Id));
            }
        }

        private XDocument Inform(FileInf[] files)
        {
            var xdoc = new XDocument(new XElement("Files"));
            foreach (FileInf file in files)
            {
                try
                {
                    if (xdoc.Root != null)
                    {
                        var xfile = new XElement("File",
                            new XAttribute("path", file.Path),
                            new XAttribute("name", file.FileName));

                        var mediaFile = new MediaFile(file.Path);
                        var info = mediaFile.Inform;
                        var infos = info.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        XElement xgeneral = null;
                        XElement xaudio = null;
                        XElement xvideo = null;

                        // Build xgeneral
                        foreach (var line in infos)
                        {
                            if (line == "General")
                            {
                                xgeneral = new XElement("General");
                                continue;
                            }

                            if (line == "Audio" || line == "Video")
                            {
                                break;
                            }

                            string[] tag = line.Split(':');
                            xgeneral.Add(new XElement("Tag",
                                new XAttribute("name", tag[0].Trim()),
                                new XAttribute("value", tag[1].Trim())));
                        }

                        // Build xvideo
                        var xvideoFound = false;
                        foreach (var line in infos)
                        {
                            if (line == "Video")
                            {
                                xvideoFound = true;
                                xvideo = new XElement("Video");
                                continue;
                            }

                            if (xvideoFound)
                            {
                                if (line == "Audio")
                                {
                                    break;
                                }

                                string[] tag = line.Split(':');
                                xvideo.Add(new XElement("Tag",
                                    new XAttribute("name", tag[0].Trim()),
                                    new XAttribute("value", tag[1].Trim())));
                            }
                        }

                        // Build xaudio
                        var xaudioFound = false;
                        foreach (var line in infos)
                        {
                            if (line == "Audio")
                            {
                                xaudioFound = true;
                                xaudio = new XElement("Audio");
                                continue;
                            }

                            if (xaudioFound)
                            {
                                if (line == "Video")
                                {
                                    break;
                                }

                                string[] tag = line.Split(':');
                                xaudio.Add(new XElement("Tag",
                                    new XAttribute("name", tag[0].Trim()),
                                    new XAttribute("value", tag[1].Trim())));
                            }
                        }

                        if (xgeneral != null)
                        {
                            xfile.Add(xgeneral);
                        }

                        if (xvideo != null)
                        {
                            xfile.Add(xvideo);
                        }

                        if (xaudio != null)
                        {
                            xfile.Add(xaudio);
                        }

                        xdoc.Root.Add(xfile);
                    }
                    InfoFormat("MediaInfo of the file {0} generated.", file.Path);

                    if (!_atLeastOneSucceed) _atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while generating the mediaInfo of the file {0}", e, file.Path);
                    _success = false;
                }
            }
            return xdoc;
        }
    }
}