# Event Consumer Template

This template helps you create event consumers/handlers in the EDA-Playground.

## Overview

Event consumers in this project use **Dapr Subscriptions** to listen to topics and handle incoming events. The project follows an inbox pattern for handling external integration events.

## Step 1: Define Event Handler Method

Create a method to handle your specific event type:

```csharp
public class YourEventHandler
{
    public async Task Handle(YourIntegrationEvent @event)
    {
        // Your event handling logic here
        Console.WriteLine($"Received event: {@event.Data}");
        
        // Example: Update state, call external APIs, trigger other events
        await ProcessEvent(@event);
    }
    
    private async Task ProcessEvent(YourIntegrationEvent @event)
    {
        // Implementation specific to your business logic
    }
}
```

## Step 2: Register Subscription in Program.cs

Configure the subscription in your `Program.cs`:

```csharp
// Add to your service registration
builder.Services.AddSingleton<YourEventHandler>();

// Map the subscription endpoint
app.MapGroup("subscriptions")
    .MapPost("/your-event-topic", async (YourIntegrationEvent @event, YourEventHandler handler) =>
    {
        await handler.Handle(@event);
        return Results.Ok();
    })
    .WithTopic("pubSub", "your-event-topic"); // Dapr component name and topic
```

## Step 3: Alternative - Using the Inbox Pattern

For more complex scenarios, use the existing inbox infrastructure:

```csharp
// In your module's When method
public class YourModule
{
    public Task When(Event @event)
        => @event switch
        {
            YourIntegrationEvent e => HandleYourEvent(e),
            _ => Task.CompletedTask
        };
    
    private async Task HandleYourEvent(YourIntegrationEvent @event)
    {
        // Your handling logic
        await ProcessBusinessLogic(@event);
    }
}

// Map the inbox subscription (if not already done)
app.MapGroup("subscriptions")
    .Inbox("inbox-topic", "pubSub"); // This handles the inbox pattern
```

## Step 4: Add Event to Type Resolver

Ensure your event is registered in the polymorphic type resolver:

```csharp
// In PolymorphicEventTypeResolver class in Infra/Extensions.cs
public class PolymorphicEventTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
        
        if (jsonTypeInfo.Type == typeof(Event))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.IgnoreAlways,
                DerivedTypes =
                {
                    // ... existing events
                    new JsonDerivedType(typeof(YourIntegrationEvent), "YourIntegrationEvent")
                }
            };
        }
        return jsonTypeInfo;
    }
}
```

## Step 5: Create Dapr Subscription Configuration

Create a subscription configuration file (optional, can also be done via attributes):

```yaml
apiVersion: dapr.io/v2alpha1
kind: Subscription
metadata:
  name: your-event-subscription
spec:
  topic: your-event-topic
  route: /subscriptions/your-event-topic
  pubsubname: pubSub
  metadata:
    rawPayload: "true"
```

## Step 6: Error Handling and Retries

Implement proper error handling:

```csharp
public class YourEventHandler
{
    private readonly ILogger<YourEventHandler> _logger;
    
    public YourEventHandler(ILogger<YourEventHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task Handle(YourIntegrationEvent @event)
    {
        try
        {
            await ProcessEvent(@event);
            _logger.LogInformation("Successfully processed event {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process event {EventId}", @event.EventId);
            
            // Decide whether to retry or send to dead letter queue
            throw; // Let Dapr handle retries
        }
    }
}
```

## Step 7: Testing Event Consumers

Create tests for your event handlers:

```csharp
[Fact]
public async Task ShouldHandleEventCorrectly()
{
    // Arrange
    var handler = new YourEventHandler();
    var testEvent = new YourIntegrationEvent("test data");
    
    // Act
    await handler.Handle(testEvent);
    
    // Assert
    // Verify the expected side effects occurred
}

[Fact]
public async Task ShouldHandleEventFailureGracefully()
{
    // Test error handling scenarios
}
```

## Integration Testing with TestContainers

For integration tests, use the existing test infrastructure:

```csharp
public class EventConsumerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task ShouldConsumeEventFromTopic()
    {
        // Use the test setup to publish events and verify consumption
    }
}
```

## Best Practices

- **Idempotency**: Make your handlers idempotent (safe to retry)
- **Error Handling**: Implement proper error handling and logging
- **Performance**: Use async/await for I/O operations
- **Monitoring**: Add metrics and observability
- **Testing**: Write both unit and integration tests
- **State Management**: Use Dapr state store for persisting handler state if needed
- **Dead Letter Queues**: Configure DLQ for failed messages
- **Ordering**: Be aware that events may arrive out of order