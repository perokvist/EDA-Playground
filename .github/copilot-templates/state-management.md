# State Management Template

Persist application state using Dapr state stores.

## Quick Steps

1. **Define State Model**
```csharp
public record YourState(Guid Id, string Data, DateTime LastUpdated) : State(Id);
```

2. **Add to Type Resolver** (`Infra/Extensions.cs`)
```csharp
.Add<YourState>()
```

3. **Create Repository**
```csharp
public class YourStateRepository(DaprClient daprClient)
{
    private const string StoreName = "stateStore";
    
    public async Task<YourState?> GetAsync(Guid id) =>
        await daprClient.GetStateAsync<YourState>(StoreName, id.ToString());
    
    public async Task SaveAsync(YourState state) =>
        await daprClient.SaveStateAsync(StoreName, state.Id.ToString(), state);
    
    public async Task DeleteAsync(Guid id) =>
        await daprClient.DeleteStateAsync(StoreName, id.ToString());
}
```

4. **Configure State Store** (`src/resources/statestore.yaml`)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.in-memory  # or state.sqlite, state.azure.cosmosdb
  version: v1
```

## Advanced Operations

**Conditional Updates:**
```csharp
var (state, etag) = await daprClient.GetStateAndETagAsync<YourState>(StoreName, id);
// Modify state...
await daprClient.SaveStateAsync(StoreName, id, state, new StateOptions(), 
    new Dictionary<string, string> { ["etag"] = etag });
```