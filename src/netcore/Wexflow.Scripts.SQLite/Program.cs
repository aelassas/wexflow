using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Wexflow.Core.Db.SQLite;

namespace Wexflow.Scripts.SQLite
{
    class Program
    {
        private static IConfiguration config;

        static void Main()
        {
            try
            {
                config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = config["workflowsFolder"];
                var db = new Db(config["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db, workflowsFolder);
                Core.Helper.InsertRecords(db, "sqlite", config["recordsFolder"], config["documentFile"], config["invoiceFile"], config["timesheetFile"]);
                db.Dispose();

                // .NET Core
                BuildDatabase("Windows", "windows");
                BuildDatabase("Linux", "linux");
                BuildDatabase("Mac OS X", "macos");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        private static void BuildDatabase(string info, string platformFolder)
        {
            Console.WriteLine($"=== Build {info} database ===");
            var path1 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow.sqlite");
            var connString = "Data Source=" + path1 + ";Version=3;";

            var workflowsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Workflows");

            if (!Directory.Exists(workflowsFolder)) throw new DirectoryNotFoundException("Invalid workflows folder: " + workflowsFolder);
            if (File.Exists(path1)) File.Delete(path1);

            var db = new Db(connString);
            Core.Helper.InsertWorkflowsAndUser(db, workflowsFolder);
            var recordsFolder = config["recordsFolder"];
            if (platformFolder == "linux")
            {
                recordsFolder = "/opt/wexflow/Wexflow/Records";
            }
            else if (platformFolder == "macos")
            {
                recordsFolder = "/Applications/wexflow/Wexflow/Records";
            }
            var isUnix = platformFolder == "linux" || platformFolder == "macos";
            Core.Helper.InsertRecords(db, "sqlite", recordsFolder, config["documentFile"], config["invoiceFile"], config["timesheetFile"], isUnix);
            db.Dispose();
        }
    }
}
