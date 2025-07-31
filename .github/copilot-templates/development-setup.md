# Development Environment Setup Template

This template helps you set up your development environment for working with the EDA-Playground.

## Prerequisites

### Required Software

1. **.NET 9.0 SDK**
   ```bash
   # Download from https://dotnet.microsoft.com/download/dotnet/9.0
   # Verify installation
   dotnet --version
   ```

2. **Dapr CLI**
   ```bash
   # Windows (PowerShell)
   powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"
   
   # macOS/Linux
   wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
   
   # Verify installation
   dapr --version
   ```

3. **Docker Desktop**
   ```bash
   # Download from https://www.docker.com/products/docker-desktop
   # Verify installation
   docker --version
   docker-compose --version
   ```

### Optional Tools

- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control
- **Azure CLI** (for Azure deployments)
- **PowerShell** (for Windows users)

## Step 1: Initialize Dapr

Initialize Dapr on your local machine:

```bash
# Initialize Dapr (this will download containers)
dapr init

# Verify Dapr is running
dapr --version
```

This sets up:
- Dapr runtime
- Redis (for state and pub/sub)
- Zipkin (for distributed tracing)

## Step 2: Clone and Setup Repository

```bash
# Clone the repository
git clone https://github.com/perokvist/EDA-Playground.git
cd EDA-Playground

# Restore dependencies
dotnet restore --configfile NuGet.Config

# Build the solution
dotnet build
```

## Step 3: Configure Local Environment

### Environment Variables

Create local environment configuration:

```bash
# Create .env file (optional)
cat > .env << EOF
ASPNETCORE_ENVIRONMENT=Development
DAPR_CLI_PATH=/usr/local/bin/dapr
StateStore__Type=in-memory
PubSub__Type=in-memory
EOF
```

### Local Data Directory

```bash
# Create data directory for local state
mkdir -p .data

# This will store:
# - SQLite databases
# - Local files
# - Development data
```

## Step 4: Development with Visual Studio

### Open the Solution

1. Open `Sample.App.sln` in Visual Studio
2. Set `Sample.App.AppHost` as the startup project
3. Press F5 to run

### Configure Multiple Startup Projects

1. Right-click solution → Properties
2. Select "Multiple startup projects"
3. Set to "Start":
   - `Sample.App.AppHost`
   - Any additional services you're developing

## Step 5: Development with VS Code

### Install Extensions

```bash
# Install recommended extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-vscode.vscode-json
code --install-extension redhat.vscode-yaml
code --install-extension ms-azuretools.vscode-dapr
```

### Launch Configuration

Create `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch AppHost",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/src/Sample.App.AppHost/bin/Debug/net9.0/Sample.App.AppHost.dll",
            "args": [],
            "cwd": "${workspaceFolder}/src/Sample.App.AppHost",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}
```

## Step 6: Running Individual Services

### Using Dapr CLI

```bash
# Run the main application with Dapr
cd src/Sample.App
dapr run --app-id sample --app-port 5000 --dapr-http-port 3500 -- dotnet run

# In another terminal, run the proxy
cd src/Sample.Proxy
dapr run --app-id sample-proxy --app-port 5001 --dapr-http-port 3501 -- dotnet run
```

### Using AppHost (Recommended)

```bash
# Run the complete application stack
cd src/Sample.App.AppHost
dotnet run
```

This starts:
- All services with Dapr sidecars
- Cosmos DB emulator
- Service discovery and orchestration

## Step 7: Development Workflow

### Hot Reload Development

```bash
# Enable hot reload for faster development
cd src/Sample.App
dotnet watch run
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Sample.App.Tests/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Management

```bash
# For SQLite development
# Install SQLite tools
dotnet tool install --global dotnet-ef

# View SQLite database
sqlite3 .data/state.db
.tables
.schema
```

## Step 8: Debugging Setup

### Debugging with Dapr

1. **VS Code**: Use the Dapr extension for integrated debugging
2. **Visual Studio**: Debug through AppHost for full-stack debugging
3. **Command Line**: Use `dapr run` with `--enable-profiling` for profiling

### Debug Configuration

```json
// In .vscode/launch.json - Debug with Dapr
{
    "name": "Debug with Dapr",
    "type": "coreclr",
    "request": "launch",
    "program": "${workspaceFolder}/src/Sample.App/bin/Debug/net9.0/Sample.App.dll",
    "args": [],
    "cwd": "${workspaceFolder}/src/Sample.App",
    "stopAtEntry": false,
    "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
    },
    "postDebugTask": "daprd-down",
    "preLaunchTask": "daprd-debug"
}
```

## Step 9: Local Infrastructure

### Using Docker Compose (Alternative to AppHost)

Create `docker-compose.dev.yml`:

```yaml
version: '3.8'
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    
  cosmos-emulator:
    image: mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
    ports:
      - "8081:8081"
      - "10251:10251"
      - "10252:10252"
      - "10253:10253"
      - "10254:10254"
    environment:
      - AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10
      - AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true
```

### Start Infrastructure

```bash
# Using Docker Compose
docker-compose -f docker-compose.dev.yml up -d

# Using AppHost (includes infrastructure)
cd src/Sample.App.AppHost && dotnet run
```

## Step 10: Verification and Testing

### Health Check Endpoints

```bash
# Check application health
curl http://localhost:5000/health

# Check Dapr health
curl http://localhost:3500/v1.0/healthz
```

### Testing Event Flow

```bash
# Publish a test event
curl -X POST http://localhost:5000/sample \
  -H "Content-Type: application/json" \
  -d '{"text": "Hello from development!"}'

# Check state was persisted
curl http://localhost:5000/sample/{id}
```

### View Dapr Dashboard

```bash
# Start Dapr dashboard
dapr dashboard

# Open browser to http://localhost:8080
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**
   ```bash
   # Check what's using ports
   netstat -tulpn | grep :5000
   
   # Kill processes if needed
   sudo kill -9 $(lsof -t -i:5000)
   ```

2. **Dapr Initialization Issues**
   ```bash
   # Reinitialize Dapr
   dapr uninstall
   dapr init
   ```

3. **Build Issues**
   ```bash
   # Clean and rebuild
   dotnet clean
   dotnet restore --configfile NuGet.Config
   dotnet build
   ```

4. **Container Issues**
   ```bash
   # Reset Docker
   docker system prune -a
   ```

### Environment Reset

```bash
# Complete environment reset
dapr uninstall
docker system prune -a
rm -rf .data/
dapr init
dotnet restore --configfile NuGet.Config
dotnet build
```

## Best Practices

- **Use AppHost**: Prefer AppHost over manual Dapr commands for consistency
- **Environment Isolation**: Use separate data directories for different branches
- **Configuration**: Use environment-specific configuration files
- **Testing**: Run tests before committing changes
- **Documentation**: Keep this setup guide updated with any changes
- **Performance**: Use appropriate resource allocations for your machine
- **Security**: Don't commit secrets or sensitive data to version control
- **Monitoring**: Use Dapr dashboard and logging for debugging
- **Updates**: Keep Dapr, .NET, and other tools updated regularly