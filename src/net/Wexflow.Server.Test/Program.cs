using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wexflow.Core.Service.Client;

namespace Wexflow.Server.Test
{
    internal class Program
    {
        const int BATCH_SIZE = 100;
        const int JOBS = 200;

        static int success = 0;
        static int failed = 0;

        private static void Main()
        {
            var stopwatch = Stopwatch.StartNew();

            var client = new WexflowServiceClient(ConfigurationManager.AppSettings["WexflowWebServiceUri"]);
            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];

            string token = null;

            try
            {
                token = client.Login(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login failed: " + ex.Message);
                return;
            }

            var semaphore = new SemaphoreSlim(BATCH_SIZE);
            var tasks = new List<Task>();

            for (int i = 0; i < JOBS; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var payload = BuildPayload();
                        var jobId = client.StartWorkflowWithVariables(payload, token);
                        Interlocked.Increment(ref success);
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Started Job ID: {jobId}");
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failed);
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"Finished: {success} succeeded, {failed} failed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string BuildPayload()
        {
            return @"
            {
                ""WorkflowId"":131,
                ""Variables"":[
                  {
                     ""Name"":""restVar1"",
                     ""Value"":""C:\\WexflowTesting\\file1.txt""
                  },
                  {
                     ""Name"":""restVar2"",
                     ""Value"":""C:\\WexflowTesting\\file2.txt""
                  }
                ]
            }";
        }
    }
}
