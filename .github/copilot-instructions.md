# GitHub Copilot Instructions for EDA-Playground

## Project Overview

EDA-Playground is an Event-Driven Architecture playground that combines **Aspire** and **Dapr** to demonstrate event-driven patterns and microservices architecture.

## Key Technologies

- **.NET 9.0**: Main application framework
- **Aspire**: Application orchestration and service discovery
- **Dapr**: Distributed application runtime for event publishing, state management, and service-to-service communication
- **xUnit**: Testing framework

## Project Structure

```
src/
├── Sample.App/              # Main application with event handlers and endpoints
├── Sample.App.AppHost/      # Aspire application host for orchestration
├── Sample.App.ServiceDefaults/ # Shared service configuration
├── Sample.Proxy/            # Proxy service for routing and aggregation
└── resources/               # Dapr configuration files (pubsub, state stores)

test/
├── Sample.App.Tests/        # Unit tests for main application
└── Sample.App.AppHost.Tests/ # Integration tests for AppHost
```

## Supported Scenarios

This playground supports the following EDA patterns:

1. **Event Publishing & Consuming** - Publish events and handle them asynchronously
2. **State Management** - Persist and retrieve application state using various backends
3. **Service Orchestration** - Manage multiple services with Aspire AppHost
4. **Proxy Patterns** - Route and aggregate requests across services
5. **Testing EDA Components** - Test event-driven workflows and state management

## Development Guidelines

- Follow the existing project structure when adding new components
- Use Dapr for event publishing/consuming and state management
- Register services in AppHost for proper orchestration
- Write tests for both unit and integration scenarios
- Use the provided Dapr configuration templates for consistency

## Quick Start Commands

```bash
# Restore dependencies
dotnet restore --configfile NuGet.Config

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application (requires Dapr)
dapr run --app-id sample-app --app-port 5000 -- dotnet run --project src/Sample.App
```

## Template Files

Use the templates in `.github/copilot-templates/` for common scenarios:

- `event-publisher.md` - Creating event publishers
- `event-consumer.md` - Creating event consumers/handlers
- `state-management.md` - Working with state stores
- `service-registration.md` - Adding services to AppHost
- `testing-eda.md` - Testing event-driven components
- `dapr-configuration.md` - Configuring Dapr components
- `development-setup.md` - Setting up the development environment