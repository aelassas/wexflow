using MonoTorrent.Client;
using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Torrent
{
    public class Torrent : Task
    {
        private const double TOLERANCE = 0.001;

        public string SaveFolder { get; set; }

        public Torrent(XElement xe, Workflow wf) : base(xe, wf)
        {
            SaveFolder = GetSetting("saveFolder");
        }

        public override TaskStatus Run()
        {
            Info("Downloading torrents...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = Download(ref atLeastOneSuccess);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating tgz.", e);
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
            return new TaskStatus(status);
        }

        private bool Download(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var torrents = SelectFiles();
                foreach (var torrent in torrents)
                {
                    success &= DownloadTorrent(torrent.Path);
                    if (!atLeastOneSuccess && success)
                    {
                        atLeastOneSuccess = true;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading torrents: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool DownloadTorrent(string path)
        {
            try
            {
                ClientEngine engine = new(new EngineSettings());

                TorrentSettingsBuilder settingsBuilder = new()
                {
                    MaximumConnections = 60,
                };
                var managerTask = engine.AddAsync(path, SaveFolder, settingsBuilder.ToSettings());
                managerTask.Wait();
                var manager = managerTask.Result;
                var task = manager.StartAsync();
                task.Wait();

                while (engine.IsRunning)
                {
                    Thread.Sleep(1000);

                    if (Math.Abs(manager.Progress - 100.0) < TOLERANCE)
                    {
                        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                        _ = engine.StopAllAsync();
                        break;
                    }

                    WaitOne();
                }

                InfoFormat("The torrent {0} download succeeded.", path);
                return true;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading the torrent {0}: {1}", path, e.Message);
                return false;
            }
        }
    }
}
