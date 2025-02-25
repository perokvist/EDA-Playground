using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIRECOSMOS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//var cosmos = builder
//.AddAzureCosmosDB("cosmos-db")
//.RunAsPreviewEmulator(emulator => emulator.WithDataExplorer());

#pragma warning restore ASPIRECOSMOS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//.AddDatabase("sample")
//.RunAsEmulator(emulator =>
//{
//    emulator
//        .WithPartitionCount(2)
//        .WithHttpsEndpoint(targetPort: 1234, name: "explorer-port")
//        .WithHttpsEndpoint(targetPort: 8081, name: "emulator-port")
//        .WithImageRegistry("mcr.microsoft.com")
//        .WithImage("cosmosdb/linux/azure-cosmos-emulator")
//        .WithImageTag("vnext-preview")
//        .WithLifetime(ContainerLifetime.Persistent);
//        //.WithArgs("--protocol", "https");
//});

var stateStore = builder
    .AddDaprStateStore("stateStore", new DaprComponentOptions { LocalPath = Path.Combine("..", "resources/state.in-memory.yml") });
    //.WaitFor(cosmos);
var pubSub = builder
    .AddDaprPubSub("pubSub", new DaprComponentOptions { LocalPath = Path.Combine("..", "resources/pubsub.in-memory.yml") });

var daprOptions = new DaprSidecarOptions();
{
    //ResourcesPaths = [Path.Combine("..", "resources")],
};

var apiservice = builder
            .AddProject<Projects.Sample_App>("sample")
            .WithDaprSidecar(options: daprOptions with { AppId = "sample" })
            .WithReference(pubSub)
            .WithReference(stateStore);
//.WaitFor(cosmos);

// Workaround for https://github.com/dotnet/aspire/issues/2219
if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
{
    builder.Services.Configure<DaprOptions>(options =>
    {
        options.DaprPath = daprCliPath;
    });
}

builder.Build().Run();
