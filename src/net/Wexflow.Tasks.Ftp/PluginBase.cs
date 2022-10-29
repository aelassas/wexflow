using Wexflow.Core;

namespace Wexflow.Tasks.Ftp
{
    public enum Protocol
    {
        Ftp,
        Ftps,
        Sftp
    }

    public abstract class PluginBase
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public string Path { get; private set; }
        public Task Task { get; private set; }
        public bool DebugLogs { get; private set; }

        protected PluginBase(Task task, string server, int port, string user, string password, string path, bool debugLogs)
        {
            Task = task;
            Server = server;
            Port = port;
            User = user;
            Password = password;
            Path = path;
            DebugLogs = debugLogs;
        }

        public abstract FileInf[] List();
        public abstract void Upload(FileInf file);
        public abstract void Download(FileInf file);
        public abstract void Delete(FileInf file);
    }
}
