[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/perokvist/EDA-playground/.github%2Fworkflows%2Fdotnet.yml?branch=main&label=Build&logo=github)](https://github.com/perokvist/EDA-Playground/actions/workflows/dotnet.yml)


# EDA-Playground
Aspire | Dapr - event playground

## GitHub Copilot Templates

This repository includes comprehensive GitHub Copilot templates to help you work effectively with Event-Driven Architecture patterns using Aspire and Dapr.

### Available Templates

The following templates are available in `.github/copilot-templates/`:

#### Core EDA Patterns

- **`event-publisher.md`** - Creating event publishers with Dapr PubSub
  - *Use when*: You need to publish domain events, integration events, or notifications to other services
  - *Contains*: Event definition, type resolver setup, publisher service patterns, and topic configuration

- **`event-consumer.md`** - Building event consumers/handlers using the inbox pattern  
  - *Use when*: You need to handle incoming events from other services or implement event-driven workflows
  - *Contains*: Handler creation, subscription mapping, inbox pattern integration, and error handling strategies

- **`state-management.md`** - Working with Dapr state stores across different backends
  - *Use when*: You need to persist application state, implement event sourcing, or manage distributed data
  - *Contains*: State model definition, repository patterns, conditional updates, and multi-backend configuration

#### Service Architecture

- **`service-registration.md`** - Adding new services to the Aspire AppHost
  - *Use when*: You're creating new microservices or adding components to the distributed application
  - *Contains*: Project setup, dependency configuration, Aspire registration, and inter-service communication

- **`testing-eda.md`** - Comprehensive testing strategies for EDA components
  - *Use when*: You need to test event handlers, validate event flows, or ensure system reliability
  - *Contains*: Unit test patterns, integration test setup, contract testing, and Aspire test configurations

#### Infrastructure & Setup

- **`dapr-configuration.md`** - Configuring Dapr components for different environments
  - *Use when*: You need to set up state stores, pub/sub systems, or switch between development/production backends
  - *Contains*: Component YAML configurations, backend options (in-memory, SQLite, Redis), and AppHost registration

- **`development-setup.md`** - Complete development environment setup guide
  - *Use when*: You're setting up a new development environment or onboarding team members
  - *Contains*: Prerequisites installation, project setup, Dapr initialization, and development workflow

### How to Use the Templates

1. **Enable GitHub Copilot** in your development environment
2. **Reference templates in your prompts** by mentioning the specific template file:
   ```
   @copilot Use the event-publisher.md template to create a new order event publisher
   ```
3. **Ask scenario-specific questions**:
   ```
   @copilot Following the state-management.md template, help me configure a SQLite state store
   ```
4. **Get comprehensive guidance** by referencing the main instructions:
   ```
   @copilot Use the copilot-instructions.md to help me understand the project structure
   ```

### Key Features

- **Technology-specific guidance** tailored to Aspire + Dapr + .NET 9
- **Environment-aware configurations** for development, staging, and production  
- **Best practices and common pitfalls** included in each template
- **Production-ready examples** with scalability and monitoring considerations
- **Step-by-step instructions** for all major EDA scenarios

For detailed project information and development guidelines, see [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

## MCP Configuration

This repository includes a Model Context Protocol (MCP) configuration that enables AI assistants to interact with development tools and automate testing workflows.

### Available MCP Servers

The configuration in `.github/mcp-config.json` provides:

- **Playwright Server** - Browser automation for testing web interfaces and monitoring Aspire dashboards
- **Filesystem Server** - Access to project files and repository structure  
- **Git Server** - Repository operations for code analysis and change tracking

### Quick Start with MCP

1. Install MCP servers:
   ```bash
   npm install -g @modelcontextprotocol/server-playwright
   npm install -g @modelcontextprotocol/server-filesystem  
   npm install -g @modelcontextprotocol/server-git
   ```

2. Configure your MCP-compatible AI assistant to use `.github/mcp-config.json`

3. Use with AI assistants for automated testing:
   ```
   Use MCP with Playwright to test the event publishing workflow in the EDA-Playground
   ```

For detailed setup and usage instructions, see [`.github/MCP-README.md`](.github/MCP-README.md).
