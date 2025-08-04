# Event Consumer Template

Create event consumers/handlers using Dapr subscriptions.

## Quick Steps

1. **Create Handler**
```csharp
public class YourEventHandler
{
    public async Task Handle(YourIntegrationEvent @event)
    {
        // Your event handling logic
        Console.WriteLine($"Received: {@event.Data}");
    }
}
```

2. **Register Subscription** (`Program.cs`)
```csharp
builder.Services.AddSingleton<YourEventHandler>();

app.MapGroup("subscriptions")
    .MapPost("/your-topic", async (YourIntegrationEvent @event, YourEventHandler handler) =>
    {
        await handler.Handle(@event);
        return Results.Ok();
    })
    .WithTopic("pubSub", "your-topic");
```

3. **Add Event to Type Resolver** (`Infra/Extensions.cs`)
```csharp
.Add<YourIntegrationEvent>()
```

## Using Inbox Pattern

For complex scenarios, use the existing inbox:

```csharp
public class YourModule
{
    public Task When(Event @event) => @event switch
    {
        YourIntegrationEvent e => HandleEvent(e),
        _ => Task.CompletedTask
    };
}

// Map inbox subscription
app.MapGroup("subscriptions").Inbox("inbox-topic", "pubSub");
```