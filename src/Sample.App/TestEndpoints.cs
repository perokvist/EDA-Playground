using System.Text.Json;
using Dapr.Client;
using Sample.App.Modules.Sample;

namespace Sample.App;

public static class TestEndpoints
{
    public static RouteGroupBuilder MapTestApi(this RouteGroupBuilder group,
        string stateStore)
    {

        group.MapPost("/", async (SampleCommand command, DaprClient dapr) =>
        {
            var meta = new Dictionary<string, string>()
            {
                { "contentType", "application/json" }
            };

            var id = Guid.NewGuid();

            var newState = new SampleState(id, "test");
            await dapr.SaveStateAsync(stateStore, $"{id}_save", newState, metadata: meta);

            var stateOperation = new StateTransactionRequest(
                key: $"{id}_trans",
                value: JsonSerializer.SerializeToUtf8Bytes(newState, options: dapr.JsonSerializerOptions),
                operationType: StateOperationType.Upsert,
                metadata: meta);

            await dapr.ExecuteStateTransactionAsync(stateStore, [stateOperation], metadata: meta);

            return Results.Created($"/test/{id}", id);

        }).WithOpenApi();

        return group;
    }

}
