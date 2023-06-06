using System;
using System.Configuration;
using Wexflow.Core.Db.Firebird;

namespace Wexflow.Scripts.Firebird
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
                Core.Helper.InsertRecords(db, "firebird");
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
