using System.Text.Json;
using Dapr.Client;
using Sample.App.Core;

namespace Sample.App.Dapr;
public static class ApplicationService
{
    /// <summary>
    /// Simple (2PC) Execute for non outbox usage.
    /// </summary>
    public static async Task Execute<TState>(this DaprClient dapr, string stateStoreName, string stateKey, TState defaultState, Func<TState, (TState, Event[])> f, string pubsubName, string topicName)
    {
        var state = await dapr.GetStateAsync<TState>(stateStoreName, stateKey);
        var currentState = state ?? defaultState;

        var (newState, events) = f(currentState);

        var meta = new Dictionary<string, string>
            {
                { "contentType", "application/json" }
            };

        await dapr.SaveStateAsync(stateStoreName, stateKey, newState, metadata: meta);
        await dapr.PublishEventAsync(pubsubName, topicName, events, meta);
    }

    public static async Task ExecuteTestDouble(
       this DaprClient dapr,
       string stateStoreName,
       string stateEventStoreName,
       Command command,
       Decider decider)
    {
        await dapr.Execute(stateStoreName, command, decider);
        await dapr.Execute<SampleState>(stateEventStoreName, command, decider);
    }

    public static Task Execute(
        this DaprClient dapr,
        string stateStoreName,
        Command command,
        Decider decider)
            => dapr.Execute(stateStoreName, command.Id.ToString(), decider.InitialState with { Id = command.Id }, state =>
            {
                var events = decider.Decide(command, state);
                var newState = events.Aggregate(state, decider.Evolve);
                return (newState, events);
            });


    public static async Task Execute<TState>(
        this DaprClient dapr, 
        string stateStoreName, 
        string stateKey, 
        TState defaultState, 
        Func<TState, (TState, Event[])> f)
    {
        var state = await dapr.GetStateAsync<TState>(stateStoreName, stateKey);

        var currentState = state ?? defaultState;
        var (newState, events) = f(currentState);

        await dapr.ExecuteWithOutboxProjection(stateStoreName, stateKey, newState, events);
    }

    /// <summary>
    /// Uses a Dapr EventStore
    /// </summary>
    public static async Task Execute<TState>(this DaprClient dapr,
        string eventStateStoreName,
        Command command,
        Decider decider)
    {
        var eventStore = new DaprEventStore(dapr)
        {
            StoreName = eventStateStoreName,
            MetaProvider = name =>
                new Dictionary<string, string>
                {
                    { "contentType", "application/json" }
                }
        };

        var streamName = $"{typeof(TState).Name}-{command.Id}";

        var rawHistory = eventStore.LoadEventStreamAsync(streamName, 0);
        var meta = await eventStore.GetStreamMetaData(streamName); //TODO fix
        var version = meta?.Version ?? 0;
        var history = rawHistory.Select(x => x.EventAs<Event>());

        var currentState = await history.AggregateAsync(decider.InitialState, decider.Evolve);

        var events = decider.Decide(command, currentState);

        var eventData = events
                .Select((x, i) => EventData.Create(eventName: x.GetType().Name, data: x))
                .ToArray();

        await eventStore.AppendToStreamAsync(streamName, version, eventData);
    }
}
