using System;
using System.Reflection;
using System.ServiceProcess;

namespace Wexflow.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Initialize the service to start
            var servicesToRun = new ServiceBase[]
            {
                new WexflowServer()
            };

            // In interactive and debug mode ?
            if (Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)
            {
                // Simulate the services execution
                RunInteractiveServices(servicesToRun);
            }
            else
            {
                // Normal service execution
                ServiceBase.Run(servicesToRun);
            }
        }

        /// <summary>
        /// Run services in interactive mode
        /// </summary>
        private static void RunInteractiveServices(ServiceBase[] servicesToRun)
        {
            Console.WriteLine();
            Console.WriteLine("Start the services in interactive mode.");
            Console.WriteLine();

            // Get the method to invoke on each service to start it
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            // Start services loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Starting {0} ... ", service.ServiceName);
                if (onStartMethod != null)
                {
                    onStartMethod.Invoke(service, new object[] {new string[] { }});
                }
                Console.WriteLine("Started");
            }

            // Waiting the end
            Console.WriteLine();
            Console.WriteLine("Press a key to stop services and finish process...");
            Console.ReadKey();
            Console.WriteLine();

            // Get the method to invoke on each service to stop it
            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Stop loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Stopping {0} ... ", service.ServiceName);
                if (onStopMethod != null)
                {
                    onStopMethod.Invoke(service, null);
                }
                Console.WriteLine("Stopped");
            }

            Console.WriteLine();
            Console.WriteLine("All services are stopped.");

            // Waiting a key press to not return to VS directly
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("=== Press a key to quit ===");
                Console.ReadKey();
            }
        }
    }
}
