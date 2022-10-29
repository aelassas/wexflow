using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.Ftp
{
    public enum FtpCommad
    {
        List,
        Upload,
        Download,
        Delete,
    }

    public enum EncryptionMode
    {
        Explicit,
        Implicit
    }

    public class Ftp : Task
    {
        private readonly PluginBase _plugin;
        private readonly FtpCommad _cmd;
        private readonly int _retryCount;
        private readonly int _retryTimeout;
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Ftp(XElement xe, Workflow wf) : base(xe, wf)
        {
            var server = GetSetting("server");
            var port = int.Parse(GetSetting("port"));
            var user = GetSetting("user");
            var password = GetSetting("password");
            var path = GetSetting("path");
            var protocol = (Protocol)Enum.Parse(typeof(Protocol), GetSetting("protocol"), true);
            var debugLogs = bool.Parse(GetSetting("debugLogs", "false"));
            switch (protocol)
            {
                case Protocol.Ftp:
                    _plugin = new PluginFtp(this, server, port, user, password, path, debugLogs);
                    break;
                case Protocol.Ftps:
                    var encryptionMode = (EncryptionMode)Enum.Parse(typeof(EncryptionMode), GetSetting("encryption"), true);
                    _plugin = new PluginFtps(this, server, port, user, password, path, encryptionMode, debugLogs);
                    break;
                case Protocol.Sftp:
                    var privateKeyPath = GetSetting("privateKeyPath", string.Empty);
                    var passphrase = GetSetting("passphrase", string.Empty);
                    _plugin = new PluginSftp(this, server, port, user, password, path, privateKeyPath, passphrase);
                    break;
            }
            _cmd = (FtpCommad)Enum.Parse(typeof(FtpCommad), GetSetting("command"), true);
            _retryCount = int.Parse(GetSetting("retryCount", "3"));
            _retryTimeout = int.Parse(GetSetting("retryTimeout", "1500"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Processing files...");

            var success = true;
            var atLeastOneSuccess = false;

            try
            {
                success = DoWork(ref atLeastOneSuccess);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while processing.", e);
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

        private bool DoWork(ref bool atLeastOneSuccess)
        {
            var success = true;
            if (_cmd == FtpCommad.List)
            {
                int r = 0;
                while (r <= _retryCount)
                {
                    try
                    {
                        var files = _plugin.List();
                        Files.AddRange(files);
                        if (!atLeastOneSuccess) atLeastOneSuccess = true;
                        break;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        r++;

                        if (r > _retryCount)
                        {
                            ErrorFormat("An error occured while listing files. Error: {0}", e.Message);
                            success = false;
                        }
                        else
                        {
                            InfoFormat("An error occured while listing files. Error: {0}. The task will tray again.", e.Message);
                            Thread.Sleep(_retryTimeout);
                        }
                    }
                }
            }
            else
            {
                var files = SelectFiles();
                for (int i = files.Length - 1; i > -1; i--)
                {
                    FileInf file = files[i];

                    int r = 0;
                    while (r <= _retryCount)
                    {
                        try
                        {
                            switch (_cmd)
                            {
                                case FtpCommad.Upload:
                                    _plugin.Upload(file);
                                    break;
                                case FtpCommad.Download:
                                    _plugin.Download(file);
                                    break;
                                case FtpCommad.Delete:
                                    _plugin.Delete(file);
                                    Workflow.FilesPerTask[file.TaskId].Remove(file);
                                    break;
                            }

                            if (!atLeastOneSuccess) atLeastOneSuccess = true;
                            break;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            r++;

                            if (r > _retryCount)
                            {
                                ErrorFormat("An error occured while processing the file {0}. Error: {1}", file.Path, e.Message);
                                success = false;
                            }
                            else
                            {
                                InfoFormat("An error occured while processing the file {0}. Error: {1}. The task will tray again.", file.Path, e.Message);
                                Thread.Sleep(_retryTimeout);
                            }
                        }
                    }
                }
            }
            return success;
        }

    }
}
