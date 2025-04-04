﻿using Sample.App.Core;
using Dapr.Client;
using IntegrationPublisher = System.Func<Sample.App.Core.Event[], System.Threading.Tasks.Task>;
using Microsoft.AspNetCore.Mvc;

namespace Sample.App.Dapr;
public static class Subscriptions
{
    public static IServiceCollection IntegrationPublisher(this IServiceCollection services, string topicName, string pubSubName)
        => services.AddSingleton<IntegrationPublisher>(sp => async events =>
        {
            var dapr = sp.GetRequiredService<DaprClient>();
            await dapr.BulkPublishEventAsync(pubSubName, topicName, events);
        });

    public static RouteHandlerBuilder Inbox(this RouteGroupBuilder builder, string name, string pubSubName)
        => Subscription(builder, name)
        .WithTopic(pubSubName, name)
        .WithOpenApi();
        
    public static void Outbox(this RouteGroupBuilder builder, string name, string pubSubName)
        => Subscription(builder, name)
        .WithTopic(pubSubName, name)
        .WithOpenApi();

    private static RouteHandlerBuilder Subscription(RouteGroupBuilder builder, string name)
     => builder.MapPost(name, async ([FromBody] Event[] events, SampleModule module, ILoggerFactory logger) =>
     {
         var l = logger.CreateLogger(name);
         l.LogInformation($"Subscription - {name} - recived : {events.GetType()}");

         foreach (var e in events)
         {
             l.LogInformation("Handling {0}", e.GetType().Name);
             await module.When(e);
         }

         return Results.Ok();
     });

}
