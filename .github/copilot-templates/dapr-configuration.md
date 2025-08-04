# Dapr Configuration Template

Configure Dapr components for state stores, pub/sub, and other building blocks.

## Quick Component Configurations

1. **In-Memory State Store** (`src/resources/state.in-memory.yml`)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: stateStore
spec:
  type: state.in-memory
  version: v1
```

2. **SQLite State Store** (`src/resources/state.sqlite.yml`)
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
    value: "Data Source=../../.data/state.db"
```

3. **In-Memory Pub/Sub** (`src/resources/pubsub.in-memory.yml`)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubSub
spec:
  type: pubsub.in-memory
  version: v1
```

4. **Redis Pub/Sub** (`src/resources/pubsub.redis.yml`)
```yaml
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
```

## Register in AppHost

```csharp
// In Sample.App.AppHost/Program.cs
var stateStore = builder.AddDaprStateStore("stateStore", new DaprComponentOptions
{
    LocalPath = Path.Combine("..", "resources", "state.in-memory.yml")
});

var pubSub = builder.AddDaprPubSub("pubSub", new DaprComponentOptions
{
    LocalPath = Path.Combine("..", "resources", "pubsub.in-memory.yml")
});
```