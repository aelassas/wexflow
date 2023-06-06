using System;
using System.Configuration;
using Wexflow.Core.Db.MongoDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.MongoDB
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Helper.InsertWorkflowsAndUser(db);
                Helper.InsertRecords(db, "mongodb");
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
