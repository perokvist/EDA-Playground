# Testing EDA Components Template

This template helps you create comprehensive tests for event-driven architecture components in the EDA-Playground.

## Overview

Testing EDA components requires strategies for both **unit testing** individual components and **integration testing** the complete event flows. The project uses **xUnit**, **TestContainers**, and **Aspire testing** infrastructure.

## Test Project Structure

```
test/
├── Sample.App.Tests/           # Unit tests for main application
├── Sample.App.AppHost.Tests/   # Integration tests for AppHost
└── Sample.YourService.Tests/   # Tests for your new services
```

## Step 1: Unit Testing Event Handlers

Test individual event handlers in isolation:

```csharp
// Test/Sample.App.Tests/EventHandlerTests.cs
public class YourEventHandlerTests
{
    [Fact]
    public async Task ShouldHandleEventCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<YourEventHandler>>();
        var mockRepository = new Mock<IYourRepository>();
        var handler = new YourEventHandler(mockLogger.Object, mockRepository.Object);
        
        var testEvent = new YourIntegrationEvent("test data");
        
        // Act
        await handler.Handle(testEvent);
        
        // Assert
        mockRepository.Verify(x => x.SaveAsync(It.IsAny<YourEntity>()), Times.Once);
    }
    
    [Fact]
    public async Task ShouldHandleErrorsGracefully()
    {
        // Arrange
        var mockRepository = new Mock<IYourRepository>();
        mockRepository.Setup(x => x.SaveAsync(It.IsAny<YourEntity>()))
                     .ThrowsAsync(new InvalidOperationException("Database error"));
        
        var handler = new YourEventHandler(Mock.Of<ILogger<YourEventHandler>>(), mockRepository.Object);
        var testEvent = new YourIntegrationEvent("test data");
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(testEvent));
    }
}
```

## Step 2: Unit Testing Deciders

Test business logic in deciders:

```csharp
public class YourDeciderTests
{
    private readonly YourDecider _decider = new();
    
    [Fact]
    public void ShouldCreateEntityWhenCommandIsValid()
    {
        // Arrange
        var command = new CreateYourCommand(Guid.NewGuid(), "test data");
        var initialState = _decider.Initial;
        
        // Act
        var events = _decider.Decide(command, initialState);
        
        // Assert
        Assert.Single(events);
        Assert.IsType<YourCreatedEvent>(events[0]);
    }
    
    [Fact]
    public void ShouldEvolveStateCorrectly()
    {
        // Arrange
        var initialState = _decider.Initial;
        var createdEvent = new YourCreatedEvent(Guid.NewGuid(), "test data");
        
        // Act
        var newState = _decider.Evolve(initialState, createdEvent);
        
        // Assert
        Assert.Equal(createdEvent.Id, newState.Id);
        Assert.Equal("test data", newState.Data);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ShouldRejectInvalidCommands(string invalidData)
    {
        // Arrange
        var command = new CreateYourCommand(Guid.NewGuid(), invalidData);
        var initialState = _decider.Initial;
        
        // Act
        var events = _decider.Decide(command, initialState);
        
        // Assert
        Assert.Empty(events);
    }
}
```

## Step 3: Testing State Management

Test state persistence and retrieval:

```csharp
public class StateRepositoryTests
{
    [Fact]
    public async Task ShouldSaveAndRetrieveState()
    {
        // Arrange
        var mockDaprClient = new Mock<DaprClient>();
        var testState = new YourState(Guid.NewGuid(), "test", DateTime.UtcNow);
        
        mockDaprClient.Setup(x => x.GetStateAsync<YourState>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<Dictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(testState);
        
        var repository = new YourStateRepository(mockDaprClient.Object, Mock.Of<IConfiguration>());
        
        // Act
        await repository.SaveAsync(testState);
        var retrievedState = await repository.GetAsync(testState.Id);
        
        // Assert
        Assert.Equal(testState.Id, retrievedState?.Id);
        mockDaprClient.Verify(x => x.SaveStateAsync(
            It.IsAny<string>(), 
            testState.Id.ToString(), 
            testState, 
            It.IsAny<StateOptions>(), 
            It.IsAny<Dictionary<string, string>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

## Step 4: Integration Testing with TestContainers

Test complete workflows with real dependencies:

```csharp
// Test/Sample.App.AppHost.Tests/IntegrationTests.cs
public class EventFlowIntegrationTests : IClassFixture<DistributedApplicationTestingBuilder>
{
    private readonly DistributedApplicationTestingBuilder _builder;
    
    public EventFlowIntegrationTests()
    {
        _builder = new DistributedApplicationTestingBuilder();
    }
    
    [Fact]
    public async Task ShouldProcessEventEndToEnd()
    {
        // Arrange
        await using var app = await _builder.BuildAsync();
        await app.StartAsync();
        
        var httpClient = app.CreateHttpClient("sample");
        var daprClient = new DaprClientBuilder().Build();
        
        // Act - Publish an event
        var command = new YourCommand(Guid.NewGuid(), "integration test");
        var response = await httpClient.PostAsJsonAsync("/api/your-endpoint", command);
        
        // Wait for event processing
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        // Assert - Verify the event was processed
        response.EnsureSuccessStatusCode();
        
        // Check state was updated
        var state = await daprClient.GetStateAsync<YourState>("stateStore", command.Id.ToString());
        Assert.NotNull(state);
        Assert.Equal("integration test", state.Data);
    }
}
```

## Step 5: Testing Event Publishing

Test that events are published correctly:

```csharp
public class EventPublishingTests
{
    [Fact]
    public async Task ShouldPublishIntegrationEvent()
    {
        // Arrange
        var publishedEvents = new List<Event>();
        var mockPublisher = new Mock<IntegrationPublisher>();
        
        mockPublisher.Setup(x => x.Invoke(It.IsAny<Event[]>()))
                    .Callback<Event[]>(events => publishedEvents.AddRange(events))
                    .Returns(Task.CompletedTask);
        
        var module = new YourModule(Mock.Of<DaprClient>(), mockPublisher.Object);
        var domainEvent = new YourDomainEvent(Guid.NewGuid(), "test");
        
        // Act
        await module.When(domainEvent);
        
        // Assert
        Assert.Single(publishedEvents);
        Assert.IsType<YourIntegrationEvent>(publishedEvents[0]);
    }
}
```

## Step 6: Testing with In-Memory Components

Use in-memory Dapr components for faster tests:

```csharp
public class InMemoryDaprTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public InMemoryDaprTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override Dapr client with in-memory implementation
                services.AddSingleton<DaprClient>(provider =>
                {
                    var daprClientBuilder = new DaprClientBuilder()
                        .UseJsonSerializationOptions(new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    return daprClientBuilder.Build();
                });
            });
        });
    }
    
    [Fact]
    public async Task ShouldWorkWithInMemoryDapr()
    {
        // Test implementation
    }
}
```

## Step 7: Performance Testing

Test performance characteristics of your EDA components:

```csharp
public class PerformanceTests
{
    [Fact]
    public async Task ShouldHandleHighEventThroughput()
    {
        // Arrange
        var handler = new YourEventHandler();
        var events = Enumerable.Range(0, 1000)
            .Select(i => new YourIntegrationEvent($"event-{i}"))
            .ToList();
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var tasks = events.Select(e => handler.Handle(e));
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Processing 1000 events took {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

## Step 8: Testing Error Scenarios

Test failure modes and recovery:

```csharp
public class ErrorHandlingTests
{
    [Fact]
    public async Task ShouldRetryOnTransientFailures()
    {
        // Arrange
        var mockRepository = new Mock<IYourRepository>();
        var attempts = 0;
        
        mockRepository.Setup(x => x.SaveAsync(It.IsAny<YourEntity>()))
                     .Returns(() =>
                     {
                         attempts++;
                         if (attempts < 3)
                             throw new TransientException("Temporary failure");
                         return Task.CompletedTask;
                     });
        
        var handler = new YourEventHandler(mockRepository.Object);
        var testEvent = new YourIntegrationEvent("test");
        
        // Act
        await handler.Handle(testEvent);
        
        // Assert
        Assert.Equal(3, attempts);
        mockRepository.Verify(x => x.SaveAsync(It.IsAny<YourEntity>()), Times.Exactly(3));
    }
    
    [Fact]
    public async Task ShouldHandlePoisonMessages()
    {
        // Test handling of malformed or poison messages
    }
}
```

## Step 9: Test Configuration

Configure your test projects:

```xml
<!-- In test project .csproj -->
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" />
<PackageReference Include="Moq" />
<PackageReference Include="FluentAssertions" />
<PackageReference Include="Testcontainers" />
<PackageReference Include="Aspire.Hosting.Testing" />
```

## Step 10: Continuous Integration

Configure tests in GitHub Actions:

```yaml
# In .github/workflows/dotnet.yml
- name: Test
  run: |
    dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.cobertura.xml
```

## Best Practices

- **Test Isolation**: Each test should be independent and idempotent
- **Test Data**: Use builders/factories for creating test data
- **Mocking**: Mock external dependencies but test real integration paths
- **Assertions**: Use descriptive assertions and failure messages
- **Categories**: Organize tests by categories (unit, integration, performance)
- **Cleanup**: Ensure proper cleanup of resources in integration tests
- **Parallelization**: Design tests to run in parallel when possible
- **Documentation**: Document complex test scenarios and setup requirements
- **Coverage**: Aim for high code coverage but focus on critical paths
- **Flakiness**: Avoid flaky tests by using deterministic test data and proper waits