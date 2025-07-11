using FluentFTP;
using FluentFTP.Client.BaseClient;
using System.Collections.Generic;
using System.Net;
using Wexflow.Core;

namespace Wexflow.Tasks.Ftp
{
    public class PluginFtps : PluginBase
    {
        private readonly FtpEncryptionMode _encryptionMode;

        public PluginFtps(Task task, string server, int port, string user, string password, string path, EncryptionMode encryptionMode, bool debugLogs)
            : base(task, server, port, user, password, path, debugLogs)
        {
            switch (encryptionMode)
            {
                case EncryptionMode.Explicit:
                    _encryptionMode = FtpEncryptionMode.Explicit;
                    break;
                case EncryptionMode.Implicit:
                    _encryptionMode = FtpEncryptionMode.Implicit;
                    break;
                default:
                    break;
            }
        }

        private void OnLogEvent(FtpTraceLevel ftpTraceLevel, string logMessage)
        {
            if (DebugLogs)
            {
                switch (ftpTraceLevel)
                {
                    case FtpTraceLevel.Error:
                        Task.Error(logMessage);
                        break;
                    case FtpTraceLevel.Verbose:
                        Task.Info(logMessage);
                        break;
                    case FtpTraceLevel.Warn:
                        Task.Info(logMessage);
                        break;
                    case FtpTraceLevel.Info:
                    default:
                        Task.Info(logMessage);
                        break;
                }
            }
        }

        public override FileInf[] List()
        {
            var files = new List<FileInf>();

            var client = new FtpClient
            {
                Host = Server,
                Port = Port,
                Credentials = new NetworkCredential(User, Password),
            };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.Config.DataConnectionType = FtpDataConnectionType.PASV;
            client.Config.EncryptionMode = _encryptionMode;
            client.Config.ValidateAnyCertificate = true;
            client.ValidateCertificate += OnValidateCertificate;
            client.Connect();
            client.SetWorkingDirectory(Path);

            var ftpFiles = PluginFtp.ListFiles(client, Task.Id);
            files.AddRange(ftpFiles);

            foreach (var file in files)
            {
                Task.InfoFormat("[PluginFTPS] file {0} found on {1}.", file.Path, Server);
            }

            client.Disconnect();

            return files.ToArray();
        }

        public override void Upload(FileInf file)
        {
            var client = new FtpClient { Host = Server, Port = Port, Credentials = new NetworkCredential(User, Password) };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.ValidateCertificate += OnValidateCertificate;
            client.Config.DataConnectionType = FtpDataConnectionType.PASV;
            client.Config.EncryptionMode = _encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(Path);

            PluginFtp.UploadFile(client, file);
            Task.InfoFormat("[PluginFTPS] file {0} sent to {1}.", file.Path, Server);

            client.Disconnect();
        }

        private static void OnValidateCertificate(BaseFtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public override void Download(FileInf file)
        {
            var client = new FtpClient
            {
                Host = Server,
                Port = Port,
                Credentials = new NetworkCredential(User, Password)
            };

            client.Config.EncryptionMode = _encryptionMode;

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.ValidateCertificate += OnValidateCertificate;
            client.Connect();
            client.SetWorkingDirectory(Path);

            PluginFtp.DownloadFile(client, file, Task);
            Task.InfoFormat("[PluginFTPS] file {0} downloaded from {1}.", file.Path, Server);

            client.Disconnect();
        }

        public override void Delete(FileInf file)
        {
            var client = new FtpClient
            {
                Host = Server,
                Port = Port,
                Credentials = new NetworkCredential(User, Password)
            };
            client.Config.EncryptionMode = _encryptionMode;

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.ValidateCertificate += OnValidateCertificate;
            client.Connect();
            client.SetWorkingDirectory(Path);

            client.DeleteFile(file.Path);
            Task.InfoFormat("[PluginFTPS] file {0} deleted on {1}.", file.Path, Server);

            client.Disconnect();
        }
    }
}
