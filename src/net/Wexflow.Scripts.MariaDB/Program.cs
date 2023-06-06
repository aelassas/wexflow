using System;
using System.Configuration;
using Wexflow.Core.Db.MariaDB;

namespace Wexflow.Scripts.MariaDB
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
                Core.Helper.InsertRecords(db, "mariadb");
                db.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            _ = Console.ReadKey();
        }
    }
}
