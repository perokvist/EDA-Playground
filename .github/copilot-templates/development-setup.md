# Development Setup Template

Quick setup guide for EDA-Playground development environment.

## Prerequisites

1. **.NET 9.0 SDK**
   ```bash
   # Download from https://dotnet.microsoft.com/download/dotnet/9.0
   dotnet --version  # Verify installation
   ```

2. **Dapr CLI**
   ```bash
   # Windows: powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"
   # macOS/Linux: wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
   dapr --version  # Verify installation
   ```

3. **Docker Desktop**
   ```bash
   # Download from https://www.docker.com/products/docker-desktop
   docker --version  # Verify installation
   ```

## Quick Start

```bash
# Clone and build
git clone [repository-url]
cd EDA-Playground
dotnet restore --configfile NuGet.Config
dotnet build

# Initialize Dapr
dapr init

# Run the application
dotnet run --project src/Sample.App.AppHost
```

## Development Workflow

**Run with hot reload:**
```bash
dotnet watch --project src/Sample.App.AppHost
```

**Run tests:**
```bash
dotnet test
```

**View Aspire dashboard:** Open http://localhost:15888 when running