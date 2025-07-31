# Service Registration Template

This template helps you add new services to the Aspire AppHost in the EDA-Playground.

## Overview

The AppHost orchestrates multiple services using **Aspire** and integrates them with **Dapr** sidecars for event-driven communication. Services are registered in the `Sample.App.AppHost` project.

## Step 1: Create Your New Service Project

Create a new service project in the `src/` directory:

```bash
# Create new service project
dotnet new webapi -n Sample.YourService -o src/Sample.YourService

# Add project to solution
dotnet sln add src/Sample.YourService/Sample.YourService.csproj
```

## Step 2: Add Required Dependencies

Add necessary NuGet packages to your service:

```xml
<!-- In Sample.YourService.csproj -->
<PackageReference Include="Aspire.Dapr" />
<PackageReference Include="Dapr.Client" />
<PackageReference Include="Dapr.Extensions.Configuration" />
```

## Step 3: Reference Service Defaults

Add reference to the shared service defaults:

```xml
<!-- In Sample.YourService.csproj -->
<ProjectReference Include="../Sample.App.ServiceDefaults/Sample.App.ServiceDefaults.csproj" />
```

## Step 4: Configure Your Service

Set up your service's `Program.cs`:

```csharp
using Sample.App.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (logging, health checks, etc.)
builder.AddServiceDefaults();

// Add Dapr client
builder.Services.AddDaprClient();

// Add your service-specific dependencies
builder.Services.AddScoped<IYourService, YourService>();

// Configure any additional services
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure middleware
app.MapDefaultEndpoints();
app.UseCloudEvents(); // Required for Dapr

// Map your endpoints
app.MapGet("/", () => "Your Service is running!");
app.MapYourApiEndpoints();

// Map Dapr subscriptions if needed
app.MapSubscribeHandler();

app.Run();
```

## Step 5: Register Service in AppHost

Add your service to the AppHost `Program.cs`:

```csharp
// In Sample.App.AppHost/Program.cs

// After existing service registrations...

var yourService = builder
    .AddProject<Projects.Sample_YourService>("your-service")
    .WithDaprSidecar(options: daprOptions with { 
        AppId = "your-service", 
        LogLevel = "debug" 
    })
    .WithReference(pubSub)           // Add if service uses pub/sub
    .WithReference(stateStore)       // Add if service uses state management
    .WithReference(stateEventStore)  // Add if service uses event store
    .WaitFor(cosmos);                // Add if service depends on Cosmos DB

// Optional: Add service reference to other services
apiservice.WithReference(yourService, "your-service");
```

## Step 6: Update Project References

If your service is referenced by other projects, update the AppHost project file:

```xml
<!-- In Sample.App.AppHost.csproj -->
<ProjectReference Include="..\Sample.YourService\Sample.YourService.csproj" />
```

## Step 7: Configure Service Communication

### HTTP Communication
```csharp
// In a service that calls your new service
public class ClientService
{
    private readonly HttpClient _httpClient;
    
    public ClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("your-service");
    }
    
    public async Task<string> CallYourService()
    {
        var response = await _httpClient.GetStringAsync("/api/endpoint");
        return response;
    }
}

// Register the HTTP client in Program.cs
builder.Services.AddHttpClient("your-service", client =>
{
    client.BaseAddress = new Uri("https://your-service"); // Aspire service discovery
});
```

### Dapr Service Invocation
```csharp
// Using Dapr for service-to-service communication
public class ClientService
{
    private readonly DaprClient _daprClient;
    
    public ClientService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task<YourResponse> CallYourService(YourRequest request)
    {
        return await _daprClient.InvokeMethodAsync<YourRequest, YourResponse>(
            "your-service", 
            "api/endpoint", 
            request
        );
    }
}
```

## Step 8: Add Environment-Specific Configuration

### Development Settings
```json
// In Sample.App.AppHost/appsettings.Development.json
{
  "Services": {
    "your-service": {
      "Debug": true,
      "Environment": "Development"
    }
  }
}
```

### Production Configuration
```csharp
// For production deployments, configure additional settings
var yourService = builder
    .AddProject<Projects.Sample_YourService>("your-service")
    .WithDaprSidecar(options: daprOptions with { 
        AppId = "your-service",
        LogLevel = "info" // Less verbose for production
    })
    .WithReplicas(3) // Scale for production
    .WithReference(pubSub)
    .WithReference(stateStore);
```

## Step 9: Add Health Checks

Configure health checks for your service:

```csharp
// In your service's Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDapr(); // Dapr health check

// In AppHost, health checks are automatically configured via service defaults
```

## Step 10: Testing Service Registration

Create integration tests for your service:

```csharp
// In test/Sample.YourService.Tests/
public class YourServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public YourServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task ShouldRegisterServiceCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## Advanced Scenarios

### Adding External Dependencies

```csharp
// For Redis cache
var redis = builder.AddRedis("redis");
yourService.WithReference(redis);

// For SQL Server
var sqlServer = builder.AddSqlServer("sql");
var database = sqlServer.AddDatabase("yourdb");
yourService.WithReference(database);

// For RabbitMQ
var rabbitmq = builder.AddRabbitMQ("messaging");
yourService.WithReference(rabbitmq);
```

### Custom Dapr Components

```csharp
// Register custom Dapr components
var customStateStore = builder
    .AddDaprStateStore("custom-store", new DaprComponentOptions
    {
        LocalPath = Path.Combine("..", "resources/state.custom.yml")
    });

yourService.WithReference(customStateStore);
```

### Service Dependencies

```csharp
// Make your service wait for dependencies
var database = builder.AddSqlServer("sql").AddDatabase("yourdb");

var yourService = builder
    .AddProject<Projects.Sample_YourService>("your-service")
    .WithDaprSidecar(options: daprOptions with { AppId = "your-service" })
    .WaitFor(database)  // Wait for database to be ready
    .WaitFor(pubSub);   // Wait for pub/sub to be ready
```

## Best Practices

- **Service Discovery**: Use Aspire's built-in service discovery instead of hardcoded URLs
- **Health Checks**: Always implement proper health checks
- **Configuration**: Use Aspire's configuration management
- **Dependencies**: Explicitly declare service dependencies with `WaitFor()`
- **Naming**: Use consistent naming conventions for services and app IDs
- **Testing**: Create integration tests that verify service registration
- **Scaling**: Consider replica configuration for production
- **Security**: Configure authentication and authorization as needed
- **Monitoring**: Leverage Aspire's built-in observability features