using Microsoft.Extensions.Configuration;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    class Program
    {
        private static IConfiguration? config;

        static void Main()
        {
            try
            {
                config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true, reloadOnChange: true)
                .Build();

                string? workflowsFolder = config["workflowsFolder"];
                Db db = new(config["connectionString"]);
                Helper.InsertWorkflowsAndUser(db, workflowsFolder);
                Helper.InsertRecords(db, "litedb", config["recordsFolder"], config["documentFile"], config["invoiceFile"], config["timesheetFile"]);
                db.Dispose();

                _ = bool.TryParse(config["buildDevDatabases"], out bool buildDevDatabases);

                if (buildDevDatabases && config != null)
                {
                    BuildDatabase("Windows", "windows", config);
                    BuildDatabase("Linux", "linux", config);
                    BuildDatabase("Mac OS X", "macos", config);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        private static void BuildDatabase(string info, string platformFolder, IConfiguration config)
        {
            Console.WriteLine($"=== Build {info} database ===");
            string path1 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow.db");
            string path2 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow-log.db");
            string connString = "Filename=" + path1 + "; Connection=direct";

            string workflowsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Workflows");

            if (!Directory.Exists(workflowsFolder)) throw new DirectoryNotFoundException("Invalid workflows folder: " + workflowsFolder);
            if (File.Exists(path1)) File.Delete(path1);
            if (File.Exists(path2)) File.Delete(path2);

            Db db = new(connString);
            Helper.InsertWorkflowsAndUser(db, workflowsFolder);
            string? recordsFolder = config["recordsFolder"];
            if (platformFolder == "linux")
            {
                recordsFolder = "/opt/wexflow/Wexflow/Records";
            }
            else if (platformFolder == "macos")
            {
                recordsFolder = "/Applications/wexflow/Wexflow/Records";
            }
            bool isUnix = platformFolder == "linux" || platformFolder == "macos";
            Helper.InsertRecords(db, "litedb", recordsFolder, config["documentFile"], config["invoiceFile"], config["timesheetFile"], isUnix);
            db.Dispose();
        }
    }
}
