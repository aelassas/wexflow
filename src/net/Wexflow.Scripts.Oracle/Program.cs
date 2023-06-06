using System;
using System.Configuration;
using Wexflow.Core.Db.Oracle;

namespace Wexflow.Scripts.Oracle
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
                Core.Helper.InsertRecords(db, "oracle");
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
