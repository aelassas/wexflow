using System;
using System.Configuration;
using Wexflow.Core.Db.SQLServer;

namespace Wexflow.Scripts.SQLServer
{
    class Program
    {
        static void Main()
        {
            try
            {
                Db db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
                Core.Helper.InsertRecords(db, "sqlserver");
                db.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
