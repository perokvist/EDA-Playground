using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var cosmos = builder
.AddAzureCosmosDB("cosmos-db")
.RunAsPreviewEmulator(emulator => 
    emulator
    .WithGatewayPort(8081) //TODO how to set this port in emulator (json)
    .WithDataExplorer()
);
var db = cosmos.AddCosmosDatabase("sampleDB");
db.AddContainer("eventstore", "/id");
db.AddContainer("state", "/id");

#pragma warning restore ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//var sqlite = builder
//    .AddSqlite("sqlite", databasePath : "../../.data/", databaseFileName: "state.db")
//    .WithSqliteWeb();

var stateStore = builder
    .AddDaprStateStore("stateStore", new DaprComponentOptions { 
        LocalPath = Path.Combine("..", "resources/state.cosmos.yml") 
    })
    .WaitFor(cosmos);

var stateEventStore = builder
    .AddDaprStateStore("stateEventStore", new DaprComponentOptions
    {
        LocalPath = Path.Combine("..", "resources/state.eventstore.cosmos.yml")
    })
    .WaitFor(cosmos);


var pubSub = builder
    .AddDaprPubSub("pubSub", new DaprComponentOptions { LocalPath = Path.Combine("..", "resources/pubsub.in-memory.yml") });

var daprOptions = new DaprSidecarOptions();
{
    //ResourcesPaths = [Path.Combine("..", "resources")],
};

var apiservice = builder
            .AddProject<Projects.Sample_App>("sample")
            .WithDaprSidecar(options: daprOptions with { AppId = "sample", LogLevel = "debug" })
            .WithReference(pubSub)
            .WithReference(stateStore)
            .WithReference(stateEventStore)
            .WaitFor(cosmos);

builder.AddProject<Projects.Sample_Proxy>("sample-proxy");

// Workaround for https://github.com/dotnet/aspire/issues/2219
if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
{
    builder.Services.Configure<DaprOptions>(options =>
    {
        options.DaprPath = daprCliPath;
    });
}

builder.Build().Run();
