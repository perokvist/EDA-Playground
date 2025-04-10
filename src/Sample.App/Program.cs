using System.Text.Json;
using Sample.App;
using Scalar.AspNetCore;
using static Sample.App.Infra.Constants;
using Sample.App.Dapr;
using Sample.App.Infra;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddConsole();

builder.Services
    .AddSingleton<SampleModule>()
    .IntegrationPublisher(OutgoingTopicName, OutgoingPubSub)
    .ConfigureHttpJsonOptions(opt =>
    {
        opt.SerializerOptions.TypeInfoResolverChain.Clear();
        opt.SerializerOptions.TypeInfoResolverChain.Add(new PolymorphicEventTypeResolver());
        opt.SerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());

    })
    .AddHttpLogging()
    .AddOpenApi()
    .AddDaprClient(clientBuilder => clientBuilder
            .UseJsonSerializationOptions(new(JsonSerializerDefaults.Web)
            {
                TypeInfoResolverChain = {
                    new PolymorphicEventTypeResolver(),
                    new PolymorphicStateTypeResolver()
                }
            }));

var app = builder.Build();

app
    .UseHttpLogging()
    .UseCloudEvents();

app.MapDefaultEndpoints();
app.MapSubscribeHandler();

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
