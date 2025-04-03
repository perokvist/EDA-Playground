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
        //await dapr.BulkPublishEventAsync(pubsubName, topicName, events);
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

    public static async Task Execute<TState>(this DaprClient dapr, string stateStoreName, string stateKey, TState defaultState, Func<TState, (TState, Event[])> f)
    {
        var options = dapr.JsonSerializerOptions;

        var meta = new Dictionary<string, string>
            {
                { "contentType", "application/json" }
            };

        var state = await dapr.GetStateAsync<TState>(stateStoreName, stateKey);

        var currentState = state ?? defaultState;
        var (newState, events) = f(currentState);

        var stateOperation = new StateTransactionRequest(
            key: stateKey,
            value: JsonSerializer.SerializeToUtf8Bytes(newState, options: options),
            operationType: StateOperationType.Upsert,
            metadata: meta);

        var json = JsonSerializer.Serialize(events, options: options);

        var eventsOperation = new StateTransactionRequest(
            key: stateKey,
            value: JsonSerializer.SerializeToUtf8Bytes(events, options: options),
            operationType: StateOperationType.Upsert,
            metadata: new Dictionary<string, string>
            {
                { "outbox.projection", "true" },
                { "contentType", "application/json" },
                { "datacontenttype", "application/json" }
            }
        );

        // Create the list of state operations
        var ops = new List<StateTransactionRequest> { stateOperation, eventsOperation };

        // Execute the state transaction
        await dapr.ExecuteStateTransactionAsync(stateStoreName, ops, metadata: meta);
    }
}
