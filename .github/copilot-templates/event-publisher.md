# Event Publisher Template

Create event publishers using Dapr PubSub for event-driven communication.

## Quick Steps

1. **Define Event**
```csharp
public record YourIntegrationEvent(string Data) : IntegrationEvent();
```

2. **Add to Type Resolver** (`Infra/Extensions.cs`)
```csharp
.Add<YourIntegrationEvent>()
```

3. **Publish Event**
```csharp
public class YourService(IntegrationPublisher publisher)
{
    public async Task PublishSomething(string data)
    {
        await publisher([new YourIntegrationEvent(data)]);
    }
}
```

4. **Configure Topic** (`Program.cs`)
```csharp
builder.Services.IntegrationPublisher("your-topic", "pubSub");
```