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

    async void startWorkflow()
    {
        Thread.CurrentThread.IsBackground = true;
        //var jobId = await client.StartWorkflow(41, username, password);
        //Console.WriteLine(jobId);
        var payload = $@"
        {{
	        ""WorkflowId"":138,
	        ""Variables"":[
	          {{
		         ""Name"":""restVar1"",
		         ""Value"":""C:\\WexflowTesting\\file1.txt""
	          }},
	          {{
		         ""Name"":""restVar2"",
		         ""Value"":""C:\\WexflowTesting\\file2.txt""
	          }}
	        ]
        }}
        ";

        var jobId = await client.StartWorkflowWithVariables(payload, username, password);
        Console.WriteLine(jobId);
    }

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