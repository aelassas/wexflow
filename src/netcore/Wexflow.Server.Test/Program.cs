using Microsoft.Extensions.Configuration;
using Wexflow.Core.Service.Client;

try
{
    var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

    WexflowServiceClient client = new(config["WexflowWebServiceUri"]);
    var username = config["Username"];
    var password = config["Password"];

    Action startWorkflow = async () =>
    {
        Thread.CurrentThread.IsBackground = true;
        var jobId = await client.StartWorkflow(41, username, password);
        Console.WriteLine(jobId);
    };

    new Thread(() =>
    {
        startWorkflow();
    }).Start();

    new Thread(() =>
    {
        startWorkflow();
    }).Start();
}
catch (Exception e)
{
    Console.WriteLine("An error occured: {0}", e);
}

_ = Console.ReadKey();