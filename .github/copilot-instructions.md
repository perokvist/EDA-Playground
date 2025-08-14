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
─ resources/               # Dapr configuration files (pubsub, state stores)

test/
├── Sample.App.Tests/        # Unit tests for main application
└── Sample.App.AppHost.Tests/ # Integration tests for AppHost
```

## Supported Scenarios

This playground supports the following EDA patterns:

1. **Event Publishing & Consuming** - Publish events and handle them asynchronously
2. **State Management** - Persist and retrieve application state using various backends
3. **Service Orchestration** - Manage multiple services with Aspire AppHost
4. **Testing EDA Components** - Test event-driven workflows and state management

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

## Coding style

- Don't use regions in code files.
- Prefer using pattern matching over if statements.
- Prefer using "=>" over "return" statements." when functions are simple.
- View Records should use View suffix
- Query Records should use Query suffix

## Template Files

Use the templates in `.github/copilot-templates/` for common scenarios:

- `state-management.md` - Working with state stores
- `service-registration.md` - Adding services to AppHost
- `dapr-configuration.md` - Configuring Dapr components
- `development-setup.md` - Setting up the development environment

## Architecture

The architecture of the EDA-Playground is based on three main slices: Command, View, and Automation. From Event Modelling.

### Command Slice

Pattern: Trigger → Command → Event(s)
Purpose: Represents a user or system action that changes the state of the system.
Example: A user clicks "Submit Order", which triggers a command and results in an OrderPlaced event.

### View Slice

Pattern: Event(s) → View
Purpose: Represents a query or read model that interprets and displays data based on past events.
Example: A dashboard showing all orders placed today, built from OrderPlaced events.

### Automation Slice

Pattern: Event(s) → View → Automated Trigger → Command → Event(s)
Purpose: Represents automated system behavior based on data conditions.
Example: When inventory drops below a threshold, an automated process triggers a RestockInventory command.