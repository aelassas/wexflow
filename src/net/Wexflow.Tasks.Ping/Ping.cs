using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Ping
{
    public class Ping : Task
    {
        public string Server { get; }

        public Ping(XElement xe, Workflow wf) : base(xe, wf)
        {
            Server = GetSetting("server");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success;
            try
            {
                success = PingHost(Server);

                InfoFormat(
                    success
                        ? "The server {0} responded the ping request."
                        : "The server {0} did not respond to the ping request.", Server);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while pinging the server {0}: {1}", Server, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }

        private bool PingHost(string server)
        {
            bool pingable;

            using (var pinger = new System.Net.NetworkInformation.Ping())
            {
                var reply = pinger.Send(server);
                pingable = reply != null && reply.Status == IPStatus.Success;
            }

            return pingable;
        }
    }
}
