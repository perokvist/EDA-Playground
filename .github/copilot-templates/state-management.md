# State Management Template

This template helps you implement state management patterns using Dapr state stores in the EDA-Playground.

## Overview

The project uses Dapr state management for persisting application state. Multiple state store backends are supported: in-memory, SQLite, Cosmos DB, and others.

## Step 1: Define Your State Model

Create a state record that represents your entity:

```csharp
// In your appropriate model file
public record YourState(Guid Id, string Data, DateTime LastUpdated) : State(Id);

// For more complex state
public record ComplexState(
    Guid Id, 
    string Name, 
    List<string> Items, 
    Dictionary<string, object> Metadata
) : State(Id);
```

## Step 2: Add State to Type Resolver

Register your state type in the polymorphic type resolver:

```csharp
// In PolymorphicStateTypeResolver class in Infra/Extensions.cs
public class PolymorphicStateTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
        
        if (jsonTypeInfo.Type == typeof(State))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.IgnoreAlways,
                DerivedTypes =
                {
                    // ... existing states
                    new JsonDerivedType(typeof(YourState), "YourState"),
                    new JsonDerivedType(typeof(ComplexState), "ComplexState")
                }
            };
        }
        return jsonTypeInfo;
    }
}
```

## Step 3: Create State Operations

Implement state operations using DaprClient:

```csharp
public class YourStateRepository
{
    private readonly DaprClient _daprClient;
    private readonly string _stateStoreName;
    
    public YourStateRepository(DaprClient daprClient, IConfiguration configuration)
    {
        _daprClient = daprClient;
        _stateStoreName = configuration["StateStore"] ?? "stateStore";
    }
    
    public async Task<YourState?> GetAsync(Guid id)
    {
        return await _daprClient.GetStateAsync<YourState>(_stateStoreName, id.ToString());
    }
    
    public async Task SaveAsync(YourState state)
    {
        await _daprClient.SaveStateAsync(_stateStoreName, state.Id.ToString(), state);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        await _daprClient.DeleteStateAsync(_stateStoreName, id.ToString());
    }
}
```

## Step 4: Advanced State Operations

### Conditional Updates (ETag)

```csharp
public class YourStateRepository
{
    public async Task<bool> UpdateConditionallyAsync(YourState state, string etag)
    {
        try
        {
            await _daprClient.SaveStateAsync(
                _stateStoreName, 
                state.Id.ToString(), 
                state, 
                new StateOptions { Consistency = ConsistencyMode.Strong },
                new Dictionary<string, string> { ["etag"] = etag }
            );
            return true;
        }
        catch (DaprException ex) when (ex.ErrorCode == "ERR_STATE_SAVE")
        {
            // ETag mismatch - concurrent modification
            return false;
        }
    }
    
    public async Task<(YourState? state, string etag)> GetWithETagAsync(Guid id)
    {
        var response = await _daprClient.GetStateAndETagAsync<YourState>(_stateStoreName, id.ToString());
        return (response.Value, response.ETag);
    }
}
```

### Bulk Operations

```csharp
public async Task SaveBulkAsync(IEnumerable<YourState> states)
{
    var stateItems = states.Select(state => new StateTransactionRequest(
        state.Id.ToString(),
        JsonSerializer.SerializeToUtf8Bytes(state),
        StateOperationType.Upsert
    ));
    
    await _daprClient.ExecuteStateTransactionAsync(_stateStoreName, stateItems);
}

public async Task<IEnumerable<YourState>> GetBulkAsync(IEnumerable<Guid> ids)
{
    var keys = ids.Select(id => id.ToString()).ToList();
    var bulkResponse = await _daprClient.GetBulkStateAsync(_stateStoreName, keys, parallelism: 10);
    
    return bulkResponse
        .Where(item => item.Value != null)
        .Select(item => JsonSerializer.Deserialize<YourState>(item.Value))
        .Where(state => state != null)
        .Cast<YourState>();
}
```

## Step 5: Integration with Deciders

Use state management in your deciders:

```csharp
public class YourDecider : Decider<YourState, YourCommand, YourDomainEvent>
{
    public override YourState Evolve(YourState state, YourDomainEvent @event) =>
        @event switch
        {
            YourCreatedEvent e => new YourState(e.Id, e.Data, DateTime.UtcNow),
            YourUpdatedEvent e => state with { Data = e.NewData, LastUpdated = DateTime.UtcNow },
            _ => state
        };
    
    public override YourDomainEvent[] Decide(YourCommand command, YourState state) =>
        command switch
        {
            CreateYourCommand cmd when state.Id == Guid.Empty => 
                [new YourCreatedEvent(cmd.Id, cmd.Data)],
            UpdateYourCommand cmd when state.Id != Guid.Empty => 
                [new YourUpdatedEvent(cmd.Id, cmd.NewData)],
            _ => []
        };
    
    public override YourState Initial => new(Guid.Empty, string.Empty, DateTime.MinValue);
}
```

## Step 6: Configure State Store

Configure your state store in `src/resources/`:

### In-Memory (Development)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.in-memory
  version: v1
  metadata: []
```

### SQLite (Local Development)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.sqlite
  version: v1
  metadata:
  - name: connectionString
    value: "Data Source=./data/state.db"
```

### Cosmos DB (Production)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.azure.cosmosdb
  version: v1
  metadata:
  - name: url
    value: "https://your-cosmos-account.documents.azure.com:443/"
  - name: masterKey
    secretKeyRef:
      name: cosmos-key
      key: key
  - name: database
    value: "your-database"
  - name: collection
    value: "your-container"
```

## Step 7: Testing State Management

### Unit Tests
```csharp
[Fact]
public async Task ShouldSaveAndRetrieveState()
{
    // Arrange
    var mockDaprClient = new Mock<DaprClient>();
    var repository = new YourStateRepository(mockDaprClient.Object, configuration);
    var state = new YourState(Guid.NewGuid(), "test data", DateTime.UtcNow);
    
    // Act
    await repository.SaveAsync(state);
    
    // Assert
    mockDaprClient.Verify(x => x.SaveStateAsync(
        "stateStore", 
        state.Id.ToString(), 
        state, 
        It.IsAny<StateOptions>(), 
        It.IsAny<Dictionary<string, string>>(), 
        It.IsAny<CancellationToken>()
    ), Times.Once);
}
```

### Integration Tests
```csharp
public class StateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task ShouldPersistStateAcrossRequests()
    {
        // Test actual state persistence using test containers or in-memory stores
    }
}
```

## Best Practices

- **Consistency**: Choose appropriate consistency levels (eventual vs strong)
- **Partitioning**: Use meaningful partition keys for scalability
- **Transactions**: Use state transactions for atomic operations
- **TTL**: Set time-to-live for temporary state
- **Encryption**: Enable encryption at rest for sensitive data
- **Monitoring**: Monitor state store performance and errors
- **Backup**: Implement backup strategies for production
- **Versioning**: Plan for state schema evolution
- **Caching**: Consider caching frequently accessed state
- **Testing**: Test with different state store backends