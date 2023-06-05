using MonoTorrent.Client;
using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Torrent
{
    public class Torrent : Task
    {
        public string SaveFolder { get; set; }

        public Torrent(XElement xe, Workflow wf) : base(xe, wf)
        {
            SaveFolder = GetSetting("saveFolder");
        }

        public override TaskStatus Run()
        {
            Info("Downloading torrents...");

            bool success;
            bool atLeastOneSuccess = false;
            try
            {
                success = Download(ref atLeastOneSuccess);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating tgz.", e);
                success = false;
            }

            Status status = Status.Success;

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
            bool success = true;
            try
            {
                FileInf[] torrents = SelectFiles();
                foreach (FileInf torrent in torrents)
                {
                    success &= DownloadTorrent(torrent.Path);
                    if (!atLeastOneSuccess && success) atLeastOneSuccess = true;
                }

            }
            catch (ThreadAbortException)
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
                ClientEngine engine = new ClientEngine(new EngineSettings());

                TorrentSettingsBuilder settingsBuilder = new TorrentSettingsBuilder
                {
                    MaximumConnections = 60,
                };
                System.Threading.Tasks.Task<TorrentManager> managerTask = engine.AddAsync(path, SaveFolder, settingsBuilder.ToSettings());
                managerTask.Wait();
                TorrentManager manager = managerTask.Result;
                System.Threading.Tasks.Task task = manager.StartAsync();
                task.Wait();

                while (engine.IsRunning)
                {
                    Thread.Sleep(1000);

                    if (manager.Progress == 100.0)
                    {
                        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                        engine.StopAllAsync();
                        break;
                    }
                }

                //MonoTorrent.Torrent torrent = MonoTorrent.Torrent.Load(path);
                //TorrentManager torrentManager = new TorrentManager(torrent, SaveFolder, new TorrentSettings());
                //engine.Register(torrentManager);
                //System.Threading.Tasks.Task task = engine.StartAllAsync();
                //task.Wait();

                //// Keep running while the torrent isn't stopped or paused.
                //while (!IsStopped && torrentManager.State != TorrentState.Stopped && torrentManager.State != TorrentState.Paused)
                //{
                //    Thread.Sleep(1000);

                //    if (torrentManager.Progress == 100.0)
                //    {
                //        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                //        //torrentManager.Stop();
                //        engine.StopAll();
                //        break;
                //    }
                //}

                InfoFormat("The torrent {0} download succeeded.", path);
                return true;
            }
            catch (ThreadAbortException)
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
