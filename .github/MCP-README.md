# MCP Configuration for EDA-Playground

This directory contains the Model Context Protocol (MCP) configuration for the EDA-Playground project, enabling AI assistants to interact with various tools and services used in the development workflow.

## What is MCP?

The Model Context Protocol (MCP) is an open protocol that enables AI assistants to securely connect to various tools and data sources. This configuration allows GitHub Copilot and other MCP-compatible AI assistants to:

- Automate browser testing with Playwright
- Access project files and Git repository
- Interact with the development environment

## Configuration Overview

The `mcp-config.json` file defines three main MCP servers:

### 1. Playwright Server
- **Purpose**: Browser automation and testing
- **Capabilities**: 
  - Launch and control web browsers
  - Take screenshots
  - Monitor network traffic
  - Perform UI testing
- **Use Cases**:
  - Test Aspire dashboard functionality
  - Verify event publishing through web interfaces
  - Monitor Dapr sidecar communication
  - End-to-end testing of EDA scenarios

### 2. Filesystem Server
- **Purpose**: File system access within the repository
- **Capabilities**:
  - Read and write files
  - List directory contents
  - Access project source code and configuration
- **Use Cases**:
  - Analyze source code structure
  - Read test scenarios and configuration files
  - Generate reports and documentation

### 3. Git Server
- **Purpose**: Git repository operations
- **Capabilities**:
  - Check repository status
  - View diffs and commit history
  - Analyze code changes
- **Use Cases**:
  - Review code changes impact
  - Analyze commit history for debugging
  - Track changes in EDA components

## Usage Examples

### End-to-End Testing
```json
{
  "scenario": "Test event publishing workflow",
  "steps": [
    "Use Playwright to launch browser and navigate to app",
    "Trigger event publishing through UI",
    "Verify events appear in Dapr logs",
    "Confirm state changes in data store"
  ]
}
```

### Monitoring and Debugging
```json
{
  "scenario": "Monitor Aspire dashboard",
  "steps": [
    "Use Playwright to navigate to dashboard",
    "Capture screenshots of service health",
    "Monitor network requests to Dapr sidecars",
    "Save results using filesystem access"
  ]
}
```

## Setup Instructions

1. **Install MCP-compatible client** (e.g., Claude Desktop, compatible IDE extension)

2. **Install required MCP servers**:
   ```bash
   npm install -g @modelcontextprotocol/server-playwright
   npm install -g @modelcontextprotocol/server-filesystem
   npm install -g @modelcontextprotocol/server-git
   ```

3. **Configure your MCP client** to use this configuration file

4. **Install Playwright browsers** (if using Playwright server):
   ```bash
   npx playwright install
   ```

## Integration with GitHub Copilot Templates

This MCP configuration complements the GitHub Copilot templates in `.github/copilot-templates/` by providing:

- **Automated testing capabilities** for the scenarios described in `testing-eda.md`
- **Live environment interaction** for development setup from `development-setup.md`
- **Real-time monitoring** of services described in `service-registration.md`
- **Dynamic validation** of event flows from `event-publisher.md` and `event-consumer.md`

## Security Considerations

- The filesystem server is scoped to the repository directory
- Git operations are read-only by default
- Playwright runs in headless mode for security
- Network access is limited to local development services

## Environment Variables

Configure these environment variables for optimal operation:

```bash
PLAYWRIGHT_HEADLESS=true          # Run browsers in headless mode
PLAYWRIGHT_TIMEOUT=30000          # Set timeout for operations
MCP_LOG_LEVEL=info               # Set logging level
```

## Troubleshooting

### Common Issues

1. **Playwright server connection fails**
   - Ensure Playwright is installed: `npx playwright install`
   - Check that the MCP server package is available

2. **Filesystem access denied**
   - Verify the repository path in the configuration
   - Ensure proper permissions on the repository directory

3. **Git operations fail**
   - Confirm Git is installed and repository is properly initialized
   - Check that the repository path is correct

### Debugging

Enable debug logging by setting:
```bash
export MCP_LOG_LEVEL=debug
```

This will provide detailed information about MCP server operations and tool interactions.