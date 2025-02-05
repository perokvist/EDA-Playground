using System.Text.Json;
using Dapr;
using Dapr.Client;
using Sample.App;
using Scalar.AspNetCore;
using static Sample.App.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.AddConsole();

builder.Services
    .AddHttpLogging()
    .AddOpenApi()
    .AddDaprClient();

var app = builder.Build();

app
    .UseHttpLogging()
    .UseCloudEvents();

app.MapSubscribeHandler();
app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapScalarApiReference(_ => _.Servers = []); // https://github.com/dotnet/aspnetcore/issues/57332



app.MapGet("/", () => "Hello World!");

app.MapGet("/sample/{id:guid}", async (Guid id, DaprClient dapr) =>
{

    var state = await dapr.GetStateAsync<SampleState>(StateStore, id.ToString());
    return Results.Ok(state);
}).WithOpenApi();

app.MapPost("/sample", async (SampleCommand command, DaprClient dapr) =>
{
    var id = Guid.NewGuid();

    await ApplicationService.Execute<SampleState>(dapr,
     StateStore,
     id.ToString(),
     new(Guid.Empty, "None"),
     state => (state with { Text = command.Text }, [new SampleEvent(command.Text), new SampleEvent(command.Text)]));
    //[new SampleEvent(command.Text)]), PubSub, TopicName);

    return Results.Created($"/sample/{id}", id);

}).WithOpenApi();


app.MapPost("/subscriptions/sample", /*[Topic(PubSub, TopicName)] */(SampleEvent[] e, ILoggerFactory logger) =>
{
    var l = logger.CreateLogger(TopicName);
    l.LogInformation("Got event");
    var json = JsonSerializer.Serialize(e);
    l.LogInformation(json);
    return Results.Ok();
})
    .WithTopic(PubSub, TopicName)
    .WithOpenApi();



app.Run();
