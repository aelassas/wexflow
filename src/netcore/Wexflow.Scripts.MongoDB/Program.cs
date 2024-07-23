using Microsoft.Extensions.Configuration;
using System;
using Wexflow.Core.Db.MongoDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.MongoDB
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
                Helper.InsertWorkflowsAndUser(db, workflowsFolder);
                Helper.InsertRecords(db, "mongodb", config["recordsFolder"], config["documentFile"], config["invoiceFile"], config["timesheetFile"]);
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
