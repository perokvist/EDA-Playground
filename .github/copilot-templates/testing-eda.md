# Testing EDA Components Template

Test event-driven architecture components using xUnit and Aspire testing.

## Quick Steps

1. **Unit Test Event Handlers**
```csharp
public class YourEventHandlerTests
{
    [Fact]
    public async Task ShouldHandleEventCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IYourRepository>();
        var handler = new YourEventHandler(mockRepository.Object);
        var testEvent = new YourIntegrationEvent("test data");
        
        // Act
        await handler.Handle(testEvent);
        
        // Assert
        mockRepository.Verify(x => x.SaveAsync(It.IsAny<YourEntity>()), Times.Once);
    }
}
```

2. **Unit Test Deciders**
```csharp
public class YourDeciderTests
{
    private readonly YourDecider _decider = new();
    
    [Fact]
    public void ShouldCreateEntityWhenCommandIsValid()
    {
        // Arrange
        var command = new CreateYourCommand(Guid.NewGuid(), "test data");
        
        // Act
        var events = _decider.Decide(command, _decider.Initial);
        
        // Assert
        Assert.Single(events);
        Assert.IsType<YourCreatedEvent>(events[0]);
    }
}
```

3. **Integration Test with AppHost**
```csharp
public class EDAIntegrationTests : IClassFixture<DistributedApplicationTestingBuilder>
{
    [Fact]
    public async Task ShouldPublishAndConsumeEvent()
    {
        // Arrange
        await using var app = await _appHost.BuildAsync();
        await app.StartAsync();
        
        var httpClient = app.CreateHttpClient("sample-app");
        
        // Act
        await httpClient.PostAsync("/publish", JsonContent.Create(new { data = "test" }));
        
        // Assert - verify event was processed
        await Task.Delay(1000); // Allow time for processing
        // Add assertions to verify the event was consumed
    }
}
```

## Test Categories

**Unit Tests**: Individual components (handlers, deciders, services)
**Integration Tests**: Complete event flows using Aspire testing
**Contract Tests**: Event schema validation