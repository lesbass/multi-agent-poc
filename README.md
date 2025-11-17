# Multi-Agent POC

A proof-of-concept implementation of a multi-agent system built with .NET, featuring an orchestrator with function calling capabilities, MCP (Model Context Protocol) servers, and independent agents communicating through Agent-to-Agent (A2A) protocol.

## Architecture Overview

This project demonstrates a distributed multi-agent architecture with the following components:

### Components

1. **WebApi Orchestrator** - Central orchestrator with function calling capabilities
2. **MCP Servers**
   - **Pluto MCP Server** - STDIO transport
   - **Topolino MCP Server** - HTTP transport
3. **Independent Agents**
   - **Minnie Agent** - Connected via A2A protocol
   - **Paperina Agent** - Connected via A2A protocol

### Architecture Diagram

```
┌─────────────────────────────────────────────┐
│         WebApi Orchestrator                 │
│         (Function Calling)                  │
│         Port: 5235                          │
└───────────┬─────────────────────────────────┘
            │
            ├──────────────┬─────────────┬──────────────┐
            │              │             │              │
            ▼              ▼             ▼              ▼
    ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
    │  Pluto   │   │ Topolino │   │  Minnie  │   │ Paperina │
    │   MCP    │   │   MCP    │   │  Agent   │   │  Agent   │
    │ (STDIO)  │   │  (HTTP)  │   │  (A2A)   │   │  (A2A)   │
    └──────────┘   └──────────┘   └──────────┘   └──────────┘
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- macOS, Linux, or Windows

## Getting Started

### Running the System

The project includes a Makefile with convenient commands to run each component:

```bash
# Run the WebApi Orchestrator
make run-webapi

# Run the Topolino MCP Server (HTTP transport)
make run-topolino-mcp

# Run the Minnie Agent
make run-minnie-agent

# Run the Paperina Agent
make run-paperina-agent
```

### Starting All Components

To run the complete system, open **four separate terminals** and execute:

```bash
# Terminal 1 - MCP Servers
make run-topolino-mcp

# Terminal 2 - Minnie Agent
make run-minnie-agent

# Terminal 3 - Paperina Agent
make run-paperina-agent

# Terminal 4 - WebApi Orchestrator
make run-webapi
```

> **Note**: The Pluto MCP Server (STDIO) is typically started automatically by the orchestrator or integrated differently due to its STDIO nature.

## Usage

### Accessing the API

Once all components are running, access the Swagger UI at:

```
http://localhost:5235/swagger/index.html
```

### Example Requests

The orchestrator can handle complex, nested function calls across all agents and MCP servers. Here's an example request:

```
Can you apply the Paperina function of Minnie function of Topolino function on the Pluto function to the Pippo function of 8 and 7?
```

This demonstrates the orchestrator's ability to:
- Chain function calls across multiple agents
- Coordinate between MCP servers with different transports
- Orchestrate A2A communication between independent agents
- Compose complex workflows from simple function primitives

### Testing with HTTP Files

The project includes a `test.http` file for testing endpoints directly from your IDE (VS Code with REST Client extension or Rider).

## Project Structure

```
multi-agent/
├── Agents/
│   ├── BaseAgent/              # Base classes and models for agents
│   │   ├── Models/
│   │   │   ├── ChatRequest.cs
│   │   │   ├── ChatResponse.cs
│   │   │   └── OpenAIConfiguration.cs
│   │   └── Services/
│   │       └── LLMService.cs
│   ├── MinnieAgent/            # Minnie agent implementation
│   │   ├── Agent.cs
│   │   ├── MinnieAgentRegistration.cs
│   │   ├── Program.cs
│   │   └── Plugins/
│   │       └── MathPlugin.cs
│   └── PaperinaAgent/          # Paperina agent implementation
│       ├── Agent.cs
│       ├── PaperinaAgentRegistration.cs
│       ├── Program.cs
│       └── Plugins/
├── McpServers/
│   ├── PlutoMcpServer/         # STDIO MCP Server
│   │   ├── Program.cs
│   │   └── Tools/
│   └── TopolinoMcpServer/      # HTTP MCP Server
│       ├── Program.cs
│       └── Tools/
├── WebApi/                     # Orchestrator API
│   ├── Program.cs
│   └── Properties/
│       └── launchSettings.json
└── Makefile                    # Build and run commands
```

## Technology Stack

- **.NET 8.0** - Core framework
- **ASP.NET Core** - Web API and hosting
- **Semantic Kernel** - Agent orchestration and function calling
- **MCP (Model Context Protocol)** - Server communication protocol
- **A2A (Agent-to-Agent)** - Inter-agent communication

## Development

### Building the Solution

```bash
dotnet build multi-agent.sln
```

### Configuration

Each component has its own `appsettings.json` and `appsettings.Development.json` for environment-specific configuration.

## Features

- **Function Calling**: The orchestrator can discover and call functions across all connected agents
- **Multiple Transport Protocols**: Support for both STDIO and HTTP MCP transports
- **Agent-to-Agent Communication**: Independent agents can communicate through A2A protocol
- **Plugin Architecture**: Extensible plugin system (e.g., MathPlugin)
- **Swagger Integration**: Interactive API documentation and testing

## Contributing

This is a proof-of-concept project. Feel free to fork and experiment with different agent configurations and communication patterns.

## Author

lesbass

## Repository

[https://github.com/lesbass/multi-agent-poc](https://github.com/lesbass/multi-agent-poc)
