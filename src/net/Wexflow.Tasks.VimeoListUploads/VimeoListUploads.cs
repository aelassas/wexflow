using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using VimeoDotNet;
using Wexflow.Core;

namespace Wexflow.Tasks.VimeoListUploads
{
    public class VimeoListUploads : Task
    {
        public string Token { get; }
        public long UserId { get; }

        public VimeoListUploads(XElement xe, Workflow wf) : base(xe, wf)
        {
            Token = GetSetting("token");
            UserId = long.Parse(GetSetting("userId", "0"));
        }

        public override TaskStatus Run()
        {
            Info("Listing uploads...");

            var status = Status.Success;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"VimeoListUploads_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                var xdoc = new XDocument(new XElement("VimeoListUploads"));
                var xvideos = new XElement("Videos");

                var vimeoApi = new VimeoClient(Token);
                var videosTask = vimeoApi.GetVideosAsync(UserId, null, null);
                videosTask.Wait();
                var videos = videosTask.Result.Data;

                foreach (var d in videos)
                {
                    xvideos.Add(new XElement("Video"
                        , new XAttribute("title", SecurityElement.Escape(d.Name) ?? throw new InvalidOperationException())
                        , new XAttribute("uri", SecurityElement.Escape(d.Uri) ?? throw new InvalidOperationException())
                        , new XAttribute("created_time", SecurityElement.Escape(d.CreatedTime.ToString(CultureInfo.InvariantCulture)) ?? throw new InvalidOperationException())
                        , new XAttribute("status", SecurityElement.Escape(d.Status) ?? throw new InvalidOperationException())
                        ));
                }

                if (xdoc.Root == null)
                {
                    throw new InvalidOperationException();
                }

                xdoc.Root.Add(xvideos);
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("Results written in {0}", xmlPath);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while listing uploads: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
