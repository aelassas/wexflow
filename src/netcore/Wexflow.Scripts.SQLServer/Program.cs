using Microsoft.Extensions.Configuration;
using System;
using Wexflow.Core.Db.SQLServer;

namespace Wexflow.Scripts.SQLServer
{
    internal sealed class Program
    {
        private static void Main()
        {
            try
            {
                var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = config["workflowsFolder"];
                Db db = new(config["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db, workflowsFolder);
                Core.Helper.InsertRecords(db, "sqlserver", config["recordsFolder"], config["documentFile"], config["invoiceFile"], config["timesheetFile"]);
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
