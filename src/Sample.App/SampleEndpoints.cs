using Dapr.Client;

namespace Sample.App;

public static class SampleEndpoints
{
    public static RouteGroupBuilder MapSampleApi(this RouteGroupBuilder group, 
        string stateStore)
    {
        group.MapGet("/{id:guid}", async (Guid id, DaprClient dapr) =>
        {
            var state = await dapr.GetStateAsync<SampleState>(stateStore, id.ToString());
            return Results.Ok(state);
        }).WithOpenApi();

        group.MapPost("/", async (SampleCommand command, DaprClient dapr, SampleModule module) =>
        {
            var id = Guid.NewGuid();
            
            await module.Dispatch(command with { Id = id });

            return Results.Created($"/sample/{id}", id); //TODO link generator

        }).WithOpenApi();
        
        return group;
    }

}
