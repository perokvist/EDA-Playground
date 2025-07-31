# Event Publisher Template

This template helps you create event publishers in the EDA-Playground.

## Overview

Event publishers in this project use **Dapr PubSub** components to publish events to topics. Events follow the pattern of either **Domain Events** (internal to the service) or **Integration Events** (across service boundaries).

## Step 1: Define Your Event

Create a new event type in the appropriate model file:

```csharp
// For domain events (internal to service)
public record YourDomainEvent(Guid Id, string Data) : DomainEvent(Id);

// For integration events (cross-service communication)  
public record YourIntegrationEvent(string Data) : IntegrationEvent();
```

## Step 2: Add Event to Type Resolver

Add your event to the polymorphic type resolver in `Infra/Extensions.cs`:

```csharp
// In PolymorphicEventTypeResolver class
.Add<YourDomainEvent>()
.Add<YourIntegrationEvent>()
```

## Step 3: Publish Domain Events

For domain events, update your decider to produce the event:

```csharp
public class YourDecider : Decider<YourState, YourCommand, YourDomainEvent>
{
    public override YourDomainEvent[] Decide(YourCommand command, YourState state) =>
        command switch
        {
            YourSpecificCommand cmd => [new YourDomainEvent(cmd.Id, cmd.Data)],
            _ => []
        };
}
```

## Step 4: Handle Domain Events (Convert to Integration Events)

In your module, handle domain events and optionally publish integration events:

```csharp
public class YourModule(DaprClient dapr, IntegrationPublisher pub)
{
    public Task When(Event @event)
        => @event switch
        {
            YourDomainEvent e => pub([new YourIntegrationEvent(e.Data)]),
            _ => Task.CompletedTask
        };
}
```

## Step 5: Direct Integration Event Publishing

For direct integration event publishing, use the IntegrationPublisher:

```csharp
public class YourService(IntegrationPublisher publisher)
{
    public async Task PublishSomething(string data)
    {
        var integrationEvent = new YourIntegrationEvent(data);
        await publisher([integrationEvent]);
    }
}
```

## Step 6: Configure PubSub Topic

Ensure your topic is configured in `Program.cs`:

```csharp
builder.Services
    .IntegrationPublisher("your-topic-name", "pubSub"); // Uses the Dapr component name
```

## Dapr Configuration

Make sure you have a pubsub component configured in `src/resources/`:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.in-memory  # or redis, azure-servicebus, etc.
  version: v1
  metadata: []
```

## Testing Your Publisher

Create tests in the test project:

```csharp
[Fact]
public async Task ShouldPublishEvent()
{
    // Arrange
    var publisher = CreateMockPublisher();
    var service = new YourService(publisher);
    
    // Act
    await service.PublishSomething("test data");
    
    // Assert
    // Verify the event was published
}
```

## Best Practices

- Use **Domain Events** for internal business logic
- Use **Integration Events** for cross-service communication  
- Keep events immutable (use records)
- Include sufficient data in events for consumers
- Use meaningful event names that describe what happened
- Consider event versioning for long-term compatibility