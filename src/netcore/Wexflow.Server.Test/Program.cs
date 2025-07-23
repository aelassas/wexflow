using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Wexflow.Core.Service.Client;

class Program
{
    const int BATCH_SIZE = 100;
    const int JOBS = 200;

    static async Task Main()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var client = new WexflowServiceClient(config["WexflowWebServiceUri"]);
            var username = config["Username"];
            var password = config["Password"];

            var token = await client.Login(username, password);

            var payloads = new List<string>(JOBS);
            for (int i = 0; i < JOBS; i++)
            {
                payloads.Add(BuildPayload());
            }

            int success = 0, failed = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = BATCH_SIZE
            };

            await Parallel.ForEachAsync(payloads, options, async (payload, ct) =>
            {
                try
                {
                    var jobId = await client.StartWorkflowWithVariables(payload, token);
                    Interlocked.Increment(ref success);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Started Job ID: {jobId}");
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failed);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error: {ex.Message}");
                }
            });

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"Finished: {success} succeeded, {failed} failed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup error: {ex}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static string BuildPayload()
    {
        return @"
        {
            ""WorkflowId"":138,
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
