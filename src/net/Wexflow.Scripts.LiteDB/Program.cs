using System;
using System.Configuration;
using System.IO;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Helper.InsertWorkflowsAndUser(db);
                Helper.InsertRecords(db, "litedb");
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
                "samples", "net", "Wexflow", "Database", "Wexflow.db");
            var path2 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..",
                "samples", "net", "Wexflow", "Database", "Wexflow-log.db");

            var connString = $"Filename={path1}; Connection=direct";

            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            if (File.Exists(path2))
            {
                File.Delete(path2);
            }

            var db = new Db(connString);
            Helper.InsertWorkflowsAndUser(db);
            Helper.InsertRecords(db, "litedb");
            db.Dispose();
        }
    }
}
