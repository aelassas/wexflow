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

        public async override System.Threading.Tasks.Task<TaskStatus> RunAsync()
        {
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Info("Downloading torrents...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = await Download(atLeastOneSuccess);
            }
            catch (OperationCanceledException)
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

        private async System.Threading.Tasks.Task<bool> Download(bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var torrents = SelectFiles();
                foreach (var torrent in torrents)
                {
                    await DownloadTorrent(torrent.Path, success);
                    if (!atLeastOneSuccess && success)
                    {
                        atLeastOneSuccess = true;
                    }
                }
            }
            catch (OperationCanceledException)
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

        private async System.Threading.Tasks.Task DownloadTorrent(string path, bool res)
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
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await System.Threading.Tasks.Task.Delay(100, Workflow.CancellationTokenSource.Token);

                    if (Math.Abs(manager.Progress - 100.0) < TOLERANCE)
                    {
                        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                        _ = engine.StopAllAsync();
                        break;
                    }

                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
                }

                InfoFormat("The torrent {0} download succeeded.", path);
                res &= true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading the torrent {0}: {1}", path, e.Message);
                res = false;
            }
        }
    }
}
