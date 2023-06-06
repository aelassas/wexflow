using System;
using System.Configuration;
using Wexflow.Core.Db.RavenDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.RavenDB
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Helper.InsertWorkflowsAndUser(db);
                Helper.InsertRecords(db, "ravendb");
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
