# Service Registration Template

Add new services to the Aspire AppHost for orchestration with Dapr.

## Quick Steps

1. **Create Service Project**
```bash
dotnet new webapi -n Sample.YourService -o src/Sample.YourService
dotnet sln add src/Sample.YourService/Sample.YourService.csproj
```

2. **Add Dependencies** (`Sample.YourService.csproj`)
```xml
<PackageReference Include="Aspire.Dapr" />
<ProjectReference Include="../Sample.App.ServiceDefaults/Sample.App.ServiceDefaults.csproj" />
```

3. **Configure Service** (`Program.cs`)
```csharp
using Sample.App.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddDaprClient();

var app = builder.Build();
app.MapDefaultEndpoints();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
```

4. **Register in AppHost** (`Sample.App.AppHost/Program.cs`)
```csharp
var yourService = builder
    .AddProject<Projects.Sample_YourService>("your-service")
    .WithDaprSidecar(options: daprOptions with { AppId = "your-service" })
    .WithReference(pubSub)
    .WithReference(stateStore);
```

## Service Communication

**HTTP via Aspire:**
```csharp
builder.Services.AddHttpClient("your-service", client =>
    client.BaseAddress = new Uri("https://your-service"));
```

**Dapr Service Invocation:**
```csharp
await daprClient.InvokeMethodAsync<Request, Response>("your-service", "api/endpoint", request);
```