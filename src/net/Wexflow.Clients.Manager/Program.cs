using System;
using System.Configuration;
using System.ServiceProcess;
using System.Windows.Forms;

namespace Wexflow.Clients.Manager
{
    public static class Program
    {
        public static string WexflowServiceName = ConfigurationManager.AppSettings["WexflowServiceName"];
        public static bool DebugMode;

        [STAThread]
        private static void Main()
        {
            if (Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)
            {
                DebugMode = true;
            }

            RunForm();
        }

        private static void RunForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }

        public static bool IsWexflowWindowsServiceRunning()
        {
            var sc = new ServiceController(WexflowServiceName);
            return sc.Status == ServiceControllerStatus.Running;
        }
    }
}
