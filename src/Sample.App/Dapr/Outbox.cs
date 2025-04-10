using System.Text.Json;
using Dapr.Client;

namespace Sample.App.Dapr;

public static class Outbox
{
    public static async Task ExecuteWithOutboxProjection<TState, TEvent>(
        this DaprClient dapr,
        string stateStoreName,
        string stateKey,
        TState newState,
        TEvent[] events)
    {
        var options = dapr.JsonSerializerOptions;

        var meta = new Dictionary<string, string>
            {
                { "contentType", "application/json" }
            };

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
