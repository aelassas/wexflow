using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Ping
{
    public class Ping : Task
    {
        public string Server { get; private set; }

        public Ping(XElement xe, Workflow wf) : base(xe, wf)
        {
            Server = GetSetting("server");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success = false;

            try
            {
                success = PingHost(Server);

                if (success)
                {
                    InfoFormat("The server {0} responded the ping request.", Server);
                }
                else
                {
                    InfoFormat("The server {0} did not respond to the ping request.", Server);
                }

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
            bool pingable = false;

            using (System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping())
            {
                PingReply reply = pinger.Send(server);
                pingable = reply.Status == IPStatus.Success;
            }

            return pingable;
        }

    }
}
