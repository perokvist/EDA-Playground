[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/perokvist/EDA-playground/.github%2Fworkflows%2Fdotnet.yml?branch=main&label=Build&logo=github)](https://github.com/perokvist/EDA-Playground/actions/workflows/dotnet.yml)


# EDA-Playground
Aspire | Dapr - event playground

## GitHub Copilot Templates

This repository includes comprehensive GitHub Copilot templates to help you work effectively with Event-Driven Architecture patterns using Aspire and Dapr.

### Available Templates

The following templates are available in `.github/copilot-templates/`:

- **`event-publisher.md`** - Creating event publishers with Dapr PubSub
- **`event-consumer.md`** - Building event consumers/handlers using the inbox pattern  
- **`state-management.md`** - Working with Dapr state stores across different backends
- **`service-registration.md`** - Adding new services to the Aspire AppHost
- **`testing-eda.md`** - Comprehensive testing strategies for EDA components
- **`dapr-configuration.md`** - Configuring Dapr components for different environments
- **`development-setup.md`** - Complete development environment setup guide

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
