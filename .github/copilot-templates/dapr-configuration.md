# Dapr Configuration Template

This template helps you configure Dapr components for various scenarios in the EDA-Playground.

## Overview

Dapr components are configured using YAML files in the `src/resources/` directory. The AppHost references these configurations to set up **state stores**, **pub/sub**, **bindings**, and other Dapr building blocks.

## Component Types

The playground supports several types of Dapr components:

- **State Stores**: For persisting application state
- **Pub/Sub**: For event publishing and subscribing
- **Bindings**: For external system integration
- **Secret Stores**: For secure configuration management
- **Configuration Stores**: For dynamic configuration

## Step 1: State Store Configuration

### In-Memory State Store (Development)
```yaml
# src/resources/state.in-memory.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.in-memory
  version: v1
  metadata: []
```

### SQLite State Store (Local Development)
```yaml
# src/resources/state.sqlite.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.sqlite
  version: v1
  metadata:
  - name: connectionString
    value: "Data Source=../../.data/state.db"
  - name: timeout
    value: "20"
  - name: busyTimeout
    value: "800"
```

### Cosmos DB State Store (Production)
```yaml
# src/resources/state.cosmos.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.azure.cosmosdb
  version: v1
  metadata:
  - name: url
    value: "{cosmos-url}"
  - name: masterKey
    value: "{cosmos-key}"
  - name: database
    value: "sampleDB"
  - name: collection
    value: "state"
  - name: partitionKey
    value: "id"
```

### Redis State Store (Scalable)
```yaml
# src/resources/state.redis.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: "localhost:6379"
  - name: redisPassword
    value: ""
  - name: enableTLS
    value: "false"
```

## Step 2: Event Store Configuration

For event sourcing scenarios:

```yaml
# src/resources/state.eventstore.cosmos.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateEventStore
spec:
  type: state.azure.cosmosdb
  version: v1
  metadata:
  - name: url
    value: "{cosmos-url}"
  - name: masterKey
    value: "{cosmos-key}"
  - name: database
    value: "sampleDB"
  - name: collection
    value: "eventstore"
  - name: partitionKey
    value: "id"
  - name: timeToLive
    value: "-1"  # Never expire events
```

## Step 3: Pub/Sub Configuration

### In-Memory Pub/Sub (Development)
```yaml
# src/resources/pubsub.in-memory.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.in-memory
  version: v1
  metadata: []
```

### Redis Pub/Sub (Scalable)
```yaml
# src/resources/pubsub.redis.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: "localhost:6379"
  - name: redisPassword
    value: ""
  - name: enableTLS
    value: "false"
  - name: consumerID
    value: "{app-id}"
```

### Azure Service Bus (Production)
```yaml
# src/resources/pubsub.servicebus.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.azure.servicebus
  version: v1
  metadata:
  - name: connectionString
    value: "{servicebus-connection-string}"
  - name: timeoutInSec
    value: "60"
  - name: handlerTimeoutInSec
    value: "30"
  - name: maxDeliveryCount
    value: "10"
  - name: lockDurationInSec
    value: "300"
```

### RabbitMQ Pub/Sub
```yaml
# src/resources/pubsub.rabbitmq.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.rabbitmq
  version: v1
  metadata:
  - name: host
    value: "amqp://localhost:5672"
  - name: durable
    value: "true"
  - name: deletedWhenUnused
    value: "false"
  - name: autoAck
    value: "false"
  - name: deliveryMode
    value: "2"
  - name: requeueInFailure
    value: "true"
```

## Step 4: Binding Configuration

For external system integration:

### HTTP Binding
```yaml
# src/resources/binding.http.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: httpBinding
spec:
  type: bindings.http
  version: v1
  metadata:
  - name: url
    value: "https://api.external-service.com"
  - name: method
    value: "POST"
```

### Cron Binding (Scheduled Tasks)
```yaml
# src/resources/binding.cron.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: cronBinding
spec:
  type: bindings.cron
  version: v1
  metadata:
  - name: schedule
    value: "0 */5 * * * *"  # Every 5 minutes
```

## Step 5: Secret Store Configuration

### Local File Secret Store (Development)
```yaml
# src/resources/secretstore.local.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: secretStore
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "../../.data/secrets.json"
```

### Azure Key Vault (Production)
```yaml
# src/resources/secretstore.keyvault.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: secretStore
spec:
  type: secretstores.azure.keyvault
  version: v1
  metadata:
  - name: vaultName
    value: "{key-vault-name}"
  - name: azureTenantId
    value: "{tenant-id}"
  - name: azureClientId
    value: "{client-id}"
  - name: azureClientSecret
    value: "{client-secret}"
```

## Step 6: Configuration Store

### Redis Configuration Store
```yaml
# src/resources/configstore.redis.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: configStore
spec:
  type: configuration.redis
  version: v1
  metadata:
  - name: redisHost
    value: "localhost:6379"
  - name: redisPassword
    value: ""
```

## Step 7: Subscription Configuration

Define subscriptions declaratively:

```yaml
# src/resources/subscription.events.yml
apiVersion: dapr.io/v2alpha1
kind: Subscription
metadata:
  name: sample-subscription
spec:
  topic: sample-events
  route: /subscriptions/sample-events
  pubsubname: pubSub
  metadata:
    rawPayload: "true"
  scopes:
  - sample
```

## Step 8: Middleware Configuration

Add middleware components:

```yaml
# src/resources/middleware.ratelimit.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: ratelimit
spec:
  type: middleware.http.ratelimit
  version: v1
  metadata:
  - name: maxRequestsPerSecond
    value: "100"
```

## Step 9: Environment-Specific Configuration

### Using Configuration Templates

Create environment-specific configurations:

```yaml
# src/resources/templates/state.template.yml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.{state-type}
  version: v1
  metadata:
  - name: connectionString
    value: "{connection-string}"
  - name: database
    value: "{database-name}"
```

### Configuration Substitution in AppHost

```csharp
// In AppHost Program.cs
var stateStoreType = builder.Configuration["StateStore:Type"] ?? "in-memory";
var stateStorePath = Path.Combine("..", $"resources/state.{stateStoreType}.yml");

var stateStore = builder
    .AddDaprStateStore("stateStore", new DaprComponentOptions
    {
        LocalPath = stateStorePath
    });
```

## Step 10: Register Components in AppHost

Register your Dapr components in the AppHost:

```csharp
// In Sample.App.AppHost/Program.cs

// State stores
var stateStore = builder
    .AddDaprStateStore("stateStore", new DaprComponentOptions
    {
        LocalPath = Path.Combine("..", "resources/state.cosmos.yml")
    })
    .WaitFor(cosmos);

// Pub/Sub
var pubSub = builder
    .AddDaprPubSub("pubSub", new DaprComponentOptions
    {
        LocalPath = Path.Combine("..", "resources/pubsub.redis.yml")
    });

// Secret store
var secretStore = builder
    .AddDaprComponent("secretStore", "secretstores.azure.keyvault", new DaprComponentOptions
    {
        LocalPath = Path.Combine("..", "resources/secretstore.keyvault.yml")
    });

// Reference components in services
var apiService = builder
    .AddProject<Projects.Sample_App>("sample")
    .WithDaprSidecar(options: daprOptions with { AppId = "sample" })
    .WithReference(pubSub)
    .WithReference(stateStore)
    .WithReference(secretStore);
```

## Best Practices

- **Environment Separation**: Use different configurations for dev/staging/prod
- **Security**: Store secrets in secure secret stores, not in configuration files
- **Performance**: Choose appropriate component types for your performance requirements
- **Reliability**: Configure retry policies and circuit breakers where appropriate
- **Monitoring**: Enable metrics and tracing for Dapr components
- **Testing**: Use in-memory components for testing, real components for integration tests
- **Documentation**: Document component configuration choices and dependencies
- **Versioning**: Version your component configurations alongside your application
- **Validation**: Validate component configurations in your CI/CD pipeline
- **Backup**: Implement backup strategies for stateful components