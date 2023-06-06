using System;
using System.Configuration;
using System.IO;
using Wexflow.Core.Db.SQLite;

namespace Wexflow.Scripts.SQLite
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
                Core.Helper.InsertRecords(db, "sqlite");
                db.Dispose();

                _ = bool.TryParse(ConfigurationManager.AppSettings["buildDevDatabase"], out var buildDevDatabase);

                if (buildDevDatabase)
                {
                    BuildDatabase("Windows");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            _ = Console.ReadKey();
        }

        private static void BuildDatabase(string info)
        {
            Console.WriteLine($"=== Build {info} database ===");
            var path1 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..",
                "samples", "net", "Wexflow", "Database", "Wexflow.sqlite");

            var connString = $"Data Source={path1};Version=3;";

            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            var db = new Db(connString);
            Core.Helper.InsertWorkflowsAndUser(db);
            Core.Helper.InsertRecords(db, "sqlite");
            db.Dispose();
        }
    }
}
