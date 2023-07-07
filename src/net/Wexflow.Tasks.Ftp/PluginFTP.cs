using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Wexflow.Core;

namespace Wexflow.Tasks.Ftp
{
    public class PluginFtp : PluginBase
    {
        private const int BUFFER_SIZE = 1 * 1024 * 1024; // 1 MB

        public PluginFtp(Task task, string server, int port, string user, string password, string path, bool debugLogs)
            : base(task, server, port, user, password, path, debugLogs)
        {
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

            var client = new FtpClient { Host = Server, Port = Port, Credentials = new NetworkCredential(User, Password) };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.Connect();
            client.SetWorkingDirectory(Path);

            var ftpFiles = ListFiles(client, Task.Id);
            files.AddRange(ftpFiles);

            foreach (var file in files)
            {
                Task.InfoFormat("[PluginFTP] file {0} found on {1}.", file.Path, Server);
            }

            client.Disconnect();

            return files.ToArray();
        }

        public static FileInf[] ListFiles(FtpClient client, int taskId)
        {
            var files = new List<FileInf>();

            var ftpListItems = client.GetListing();

            foreach (var item in ftpListItems)
            {
                if (item.Type == FtpObjectType.File)
                {
                    files.Add(new FileInf(item.FullName, taskId));
                }
            }

            return files.ToArray();
        }

        public override void Upload(FileInf file)
        {
            var client = new FtpClient { Host = Server, Port = Port, Credentials = new NetworkCredential(User, Password) };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.Connect();
            client.SetWorkingDirectory(Path);

            UploadFile(client, file);
            Task.InfoFormat("[PluginFTP] file {0} sent to {1}.", file.Path, Server);

            client.Disconnect();
        }

        public static void UploadFile(FtpClient client, FileInf file)
        {
            using (Stream istream = File.Open(file.Path, FileMode.Open, FileAccess.Read))
            using (var ostream = client.OpenWrite(file.RenameToOrName))
            {
                var buffer = new byte[BUFFER_SIZE];
                int r;

                while ((r = istream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    ostream.Write(buffer, 0, r);
                }
            }
        }

        public override void Download(FileInf file)
        {
            var client = new FtpClient { Host = Server, Port = Port, Credentials = new NetworkCredential(User, Password) };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.Connect();
            client.SetWorkingDirectory(Path);

            DownloadFile(client, file, Task);
            Task.InfoFormat("[PluginFTP] file {0} downloaded from {1}.", file.Path, Server);

            client.Disconnect();
        }

        public static void DownloadFile(FtpClient client, FileInf file, Task task)
        {
            var destFileName = System.IO.Path.Combine(task.Workflow.WorkflowTempFolder, file.FileName);
            using (var istream = client.OpenRead(file.Path))
            using (Stream ostream = File.Create(destFileName))
            {
                // istream.Position is incremented accordingly to the reads you perform
                // istream.Length == file size if the server supports getting the file size
                // also note that file size for the same file can vary between ASCII and Binary
                // modes and some servers won't even give a file size for ASCII files! It is
                // recommended that you stick with Binary and worry about character encodings
                // on your end of the connection.
                const int bufferSize = 8192;
                var buffer = new byte[bufferSize];
                int r;

                while ((r = istream.Read(buffer, 0, bufferSize)) > 0)
                {
                    ostream.Write(buffer, 0, r);
                }
            }
            task.Files.Add(new FileInf(destFileName, task.Id));
        }

        public override void Delete(FileInf file)
        {
            var client = new FtpClient { Host = Server, Port = Port, Credentials = new NetworkCredential(User, Password) };

            if (DebugLogs)
            {
                client.LegacyLogger = OnLogEvent;
            }

            client.Connect();
            client.SetWorkingDirectory(Path);

            client.DeleteFile(file.Path);
            Task.InfoFormat("[PluginFTP] file {0} deleted on {1}.", file.Path, Server);

            client.Disconnect();
        }
    }
}
