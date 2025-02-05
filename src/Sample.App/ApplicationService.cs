using System.Text.Json;
using Dapr.Client;

namespace Sample.App;
public static class ApplicationService
{
    public static async Task Execute<TState>(this DaprClient dapr, string stateStoreName, string stateKey, TState defaultState, Func<TState, (TState, Event[])> f, string pubsubName, string topicName)
    {
        var state = await dapr.GetStateAsync<TState>(stateStoreName, stateKey);
        var currentState = state ?? defaultState;

        var (newState, events) = f(currentState);

        await dapr.SaveStateAsync(stateStoreName, stateKey, newState);
        await dapr.BulkPublishEventAsync(pubsubName, topicName, events);
    }


    public static async Task Execute<TState>(this DaprClient dapr, string stateStoreName, string stateKey, TState defaultState, Func<TState, (TState, Event[])> f)
    {
        var state = await dapr.GetStateAsync<TState>(stateStoreName, stateKey);

        var currentState = state ?? defaultState;

        var (newState, events) = f(currentState);

        var stateOperation = new StateTransactionRequest(
            key: stateKey,
            value: JsonSerializer.SerializeToUtf8Bytes(newState),
            operationType: StateOperationType.Upsert,
            metadata: new Dictionary<string, string>
            {
                { "contentType", "application/json" }
            });


        //var eventOps = events.Select(x => new StateTransactionRequest(
        //    key: stateKey,
        //    value: JsonSerializer.SerializeToUtf8Bytes(events),
        //    operationType: StateOperationType.Upsert,
        //    metadata: new Dictionary<string, string>
        //    {
        //        { "outbox.projection", "true" },
        //        { "contentType", "application/json" },
        //        { "datacontenttype", "application/json" }
        //    }
        //));

        var eventsOperation = new StateTransactionRequest(
            key: stateKey,
            value: JsonSerializer.SerializeToUtf8Bytes(events),
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
        await dapr.ExecuteStateTransactionAsync(stateStoreName, ops);
    }
}
