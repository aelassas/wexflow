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

    var workflows = await client.Search(string.Empty, username, password);

    foreach (var workflow in workflows)
    {
        Console.WriteLine($"Starting workflow {workflow.Id} - {workflow.Name} => {await client.StartWorkflow(workflow.Id, username, password)}");
    }
}
catch (Exception e)
{
    Console.WriteLine("An error occured: {0}", e);
}

Console.Write("Press any key to exit...");
_ = Console.ReadKey();