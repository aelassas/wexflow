using Microsoft.Extensions.Configuration;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    internal sealed class Program
    {
        private static IConfiguration? _config;

        private static void Main()
        {
            try
            {
                _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = _config["workflowsFolder"];
                Db db = new(_config["connectionString"]);
                Helper.InsertWorkflowsAndUser(db, workflowsFolder);
                Helper.InsertRecords(db, "litedb", _config["recordsFolder"], _config["documentFile"], _config["invoiceFile"], _config["timesheetFile"]);
                db.Dispose();

                _ = bool.TryParse(_config["buildDevDatabases"], out var buildDevDatabases);

                if (buildDevDatabases)
                {
                    BuildDatabase("Windows", "windows", _config);
                    BuildDatabase("Linux", "linux", _config);
                    BuildDatabase("Mac OS X", "macos", _config);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            _ = Console.ReadKey();
        }

        private static void BuildDatabase(string info, string platformFolder, IConfiguration config)
        {
            Console.WriteLine($"=== Build {info} database ===");
            var path1 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow.db");
            var path2 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow-log.db");
            var connString = $"Filename={path1}; Connection=direct";

            var workflowsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Workflows");

            if (!Directory.Exists(workflowsFolder))
            {
                throw new DirectoryNotFoundException($"Invalid workflows folder: {workflowsFolder}");
            }

            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            if (File.Exists(path2))
            {
                File.Delete(path2);
            }

            Db db = new(connString);
            Helper.InsertWorkflowsAndUser(db, workflowsFolder);
            var recordsFolder = config["recordsFolder"];
            if (platformFolder == "linux")
            {
                recordsFolder = "/opt/wexflow/Wexflow/Records";
            }
            else if (platformFolder == "macos")
            {
                recordsFolder = "/Applications/wexflow/Wexflow/Records";
            }
            var isUnix = platformFolder is "linux" or "macos";
            Helper.InsertRecords(db, "litedb", recordsFolder, config["documentFile"], config["invoiceFile"], config["timesheetFile"], isUnix);
            db.Dispose();
        }
    }
}
