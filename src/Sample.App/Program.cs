using System.Text.Json;
using Sample.App;
using Scalar.AspNetCore;
using static Sample.App.Constants;
using Sample.App.Dapr;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddConsole();


builder.Services
    .AddSingleton<SampleModule>()
    .IntegrationPublisher(OutgoingTopicName, OutgoingPubSub)
    .ConfigureHttpJsonOptions(opt => {
        opt.SerializerOptions.TypeInfoResolver = new PolymorphicTypeResolver();
    })
    .AddHttpLogging()
    .AddOpenApi()
    .AddDaprClient(clientBuilder => clientBuilder
            .UseJsonSerializationOptions(new(JsonSerializerDefaults.Web) { TypeInfoResolver = new PolymorphicTypeResolver() }));

var app = builder.Build();

app
    .UseHttpLogging()
    .UseCloudEvents();

app.MapSubscribeHandler();
app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapScalarApiReference(_ => _.Servers = []); // https://github.com/dotnet/aspnetcore/issues/57332

app.MapGet("/", () => "Hello World!");

app
    .MapGroup("/sample")
    .MapSampleApi(StateStore);

app
    .MapGroup("/test")
    .MapTestApi(StateStore);

app.MapGroup("subscriptions")
    .Inbox(InboxTopicName, InboxPubSub);

app.MapGroup("subscriptions")
    .Outbox(OutgoingTopicName, OutgoingPubSub);

app.Run();
