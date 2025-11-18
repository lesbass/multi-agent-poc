# Multi-Agent POC

A proof-of-concept implementation of a multi-agent system built with .NET, featuring an orchestrator with function calling capabilities, MCP (Model Context Protocol) servers, and independent agents communicating through Agent-to-Agent (A2A) protocol.

## Architecture Overview

This project implements a **Multi-agent MCP + A2A System** with dynamic service discovery, following the architecture pattern where:

1. **Remote MCP Servers** expose tools via Model Context Protocol
2. **Remote A2A Agents** provide specialized capabilities via Agent-to-Agent protocol
3. **Central Orchestrator (Host Agent)** coordinates all services with:
   - **MCP Connector** for dynamic MCP server discovery and connection
   - **Agent Registry** for A2A agent management and task delegation
   - **Configuration-driven** setup via `mcpconfig.json` and `agentconfig.json`

### Components

1. **WebApi Orchestrator** (Host Agent)
   - Central orchestrator with Semantic Kernel and function calling
   - **MCP Connector**: Dynamic discovery and connection of MCP servers
   - **Agent Registry**: A2A agent management and task delegation
   - Port: 5235

2. **MCP Servers** (Remote Tools)
   - **Pluto MCP Server** - STDIO transport
   - **Topolino MCP Server** - HTTP transport (Port: 5010)
   - Configured via `mcpconfig.json`

3. **A2A Agents** (Remote Specialists)
   - **Minnie Agent** - String processing specialist (Port: 5020)
   - **Paperina Agent** - String processing specialist (Port: 5030)
   - Configured via `agentconfig.json`

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│              WebApi Orchestrator (Host Agent)               │
│                                                             │
│  ┌────────────────┐              ┌────────────────┐        │
│  │ MCP Connector  │              │ Agent Registry │        │
│  │                │              │                │        │
│  │ • List Servers │              │ • List Agents  │        │
│  │ • Connect      │              │ • Get Card     │        │
│  │ • Disconnect   │              │ • Delegate     │        │
│  └────────┬───────┘              └────────┬───────┘        │
│           │                               │                │
│      mcpconfig.json                  agentconfig.json      │
│           │                               │                │
└───────────┼───────────────────────────────┼────────────────┘
            │                               │
            ├──────────┬────────────────────┼──────────┐
            │          │                    │          │
            ▼          ▼                    ▼          ▼
    ┌──────────┐ ┌──────────┐      ┌──────────┐ ┌──────────┐
    │  Pluto   │ │ Topolino │      │  Minnie  │ │ Paperina │
    │   MCP    │ │   MCP    │      │  Agent   │ │  Agent   │
    │ (STDIO)  │ │ (HTTP)   │      │  (A2A)   │ │  (A2A)   │
    │          │ │ :5010    │      │  :5020   │ │  :5030   │
    └──────────┘ └──────────┘      └──────────┘ └──────────┘
```

### Key Features

- ✅ **Dynamic MCP Discovery**: MCP servers configured via `mcpconfig.json`
- ✅ **Agent Registry**: Centralized A2A agent management via `agentconfig.json`
- ✅ **Service Endpoints**: REST APIs for listing and managing services
- ✅ **Task Delegation**: Intelligent routing of tasks to appropriate agents
- ✅ **Configuration-Driven**: Enable/disable services without code changes

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

### New Management Endpoints

The orchestrator now provides REST APIs for service discovery and management:

#### MCP Server Management
```bash
# List all configured MCP servers
GET http://localhost:5235/mcp/servers

# Response example:
[
  {
    "name": "Pluto MCP Server",
    "description": "STDIO-based MCP server",
    "transport": "stdio",
    "enabled": true
  },
  {
    "name": "Topolino MCP Server",
    "description": "HTTP-based MCP server",
    "transport": "http",
    "url": "http://localhost:5010/mcp",
    "enabled": true
  }
]
```

#### A2A Agent Management
```bash
# List all registered agents
GET http://localhost:5235/agents

# Get specific agent details
GET http://localhost:5235/agents/minnie

# Get agent card (capabilities and skills)
GET http://localhost:5235/agents/minnie/card

# Delegate task to specific agent
POST http://localhost:5235/agents/minnie/delegate
Content-Type: application/json

{
  "message": "Calculate the Minnie function of 'hello'"
}
```

### Configuration Files

#### mcpconfig.json
Configure MCP servers dynamically:

```json
{
  "mcpServers": {
    "pluto": {
      "name": "Pluto MCP Server",
      "description": "STDIO-based MCP server",
      "transport": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "McpServers/PlutoMcpServer/PlutoMcpServer.csproj"],
      "enabled": true
    },
    "topolino": {
      "name": "Topolino MCP Server",
      "description": "HTTP-based MCP server",
      "transport": "http",
      "url": "http://localhost:5010/mcp",
      "enabled": true
    }
  }
}
```

#### agentconfig.json
Configure A2A agents dynamically:

```json
{
  "a2aAgents": {
    "minnie": {
      "name": "Minnie Agent",
      "description": "String processing specialist",
      "url": "http://localhost:5020",
      "capabilities": {
        "streaming": false,
        "pushNotifications": false
      },
      "skills": [
        {
          "id": "minnie_sk",
          "name": "Minnie Function",
          "description": "Calculates the Minnie function",
          "tags": ["minnie", "string", "function"]
        }
      ],
      "enabled": true
    }
  }
}
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
│   ├── OrchestratorAgent/      # Host agent with orchestration
│   │   ├── Configuration/
│   │   │   ├── McpServerConfig.cs
│   │   │   └── A2AAgentConfig.cs
│   │   ├── Services/
│   │   │   ├── McpConnector.cs      # Dynamic MCP connection
│   │   │   └── AgentRegistry.cs     # A2A agent management
│   │   ├── Plugins/
│   │   │   ├── DynamicA2APlugin.cs  # Dynamic A2A agent plugin
│   │   │   ├── MathPlugin.cs
│   │   │   ├── PlutoMcpPlugin.cs
│   │   │   └── TopolinoMcpPlugin.cs
│   │   └── OrchestratorAgentRegistration.cs
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
├── WebApi/                     # Orchestrator API entry point
│   ├── mcpconfig.json          # MCP server configuration
│   ├── agentconfig.json        # A2A agent configuration
│   ├── appsettings.json
│   ├── appsettings.Development.json
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

- **Dynamic Service Discovery**: Configuration-driven MCP and A2A service management
- **MCP Connector**: Automatic discovery and connection of MCP servers
- **Agent Registry**: Centralized A2A agent management with task delegation
- **REST API Management**: Endpoints for listing, inspecting, and controlling services
- **Function Calling**: The orchestrator can discover and call functions across all connected agents
- **Multiple Transport Protocols**: Support for both STDIO and HTTP MCP transports
- **Agent-to-Agent Communication**: Independent agents can communicate through A2A protocol
- **Plugin Architecture**: Extensible plugin system (e.g., MathPlugin)
- **Configuration Hot-Reload**: Enable/disable services without code changes
- **Swagger Integration**: Interactive API documentation and testing

---

## Architecture Deep Dive

### Service Discovery Flow

#### MCP Server Discovery

1. **Startup**: `OrchestratorAgentRegistration` loads `mcpconfig.json` from `WebApi/` directory
2. **Connection**: `McpConnector.ConnectServerAsync()` establishes connections based on transport type
3. **Tool Discovery**: Each MCP server exposes its tools via the MCP protocol
4. **Registration**: Tools are registered as Semantic Kernel functions and become available for orchestration

#### A2A Agent Discovery

1. **Startup**: `OrchestratorAgentRegistration` loads `agentconfig.json` from `WebApi/` directory
2. **Registry**: `AgentRegistry` maintains an inventory of available agents
3. **Card Fetching**: Agent capabilities are fetched on-demand via the `/agent-card` endpoint
4. **Task Delegation**: Tasks are routed to agents via their `/tasks` endpoint following A2A protocol

### Implementation Details

#### McpConnector Class

**Location**: `Agents/OrchestratorAgent/Services/McpConnector.cs`

**Key Methods**:
- `ListServersAsync()` - Returns all enabled MCP servers from configuration
- `ConnectServerAsync(serverId, kernel)` - Connects a specific server to the kernel
- `DisconnectServerAsync(serverId)` - Disconnects a server

**Transport Handling**:
- **STDIO**: Spawns a child process and communicates via stdin/stdout
- **HTTP**: Connects via HTTP client to the server endpoint

#### AgentRegistry Class

**Location**: `Agents/OrchestratorAgent/Services/AgentRegistry.cs`

**Key Methods**:
- `ListAgentsAsync()` - Returns all enabled agents from configuration
- `GetAgentAsync(agentId)` - Gets agent configuration details
- `GetAgentCardAsync(agentId)` - Fetches agent capabilities and skills
- `DelegateTaskAsync(agentId, message)` - Sends a task directly to a specific agent

**Communication**:
- Uses `HttpClient` for A2A protocol communication
- Follows A2A specification for task creation and artifact retrieval
- Caches agent cards to minimize network calls

#### DynamicA2APlugin Class

**Location**: `Agents/OrchestratorAgent/Plugins/DynamicA2APlugin.cs`

**Purpose**: Provides a dynamic plugin that auto-configures based on `agentconfig.json`, eliminating the need for static agent plugins.

**Key Functions**:
- `CallA2AAgent(agentId, input)` - Generic function to call any registered agent
- `ListA2AAgents()` - Returns information about available agents

### Configuration Schema

#### mcpconfig.json Schema

Located in `WebApi/mcpconfig.json`:

```typescript
{
  "mcpServers": {
    "[serverId]": {
      "name": string,              // Display name
      "description": string,       // Server description
      "transport": "stdio" | "http", // Transport protocol
      "command"?: string,          // For STDIO: executable command
      "args"?: string[],          // For STDIO: command arguments
      "url"?: string,             // For HTTP: server base URL
      "enabled": boolean          // Enable/disable server
    }
  }
}
```

#### agentconfig.json Schema

Located in `WebApi/agentconfig.json`:

```typescript
{
  "a2aAgents": {
    "[agentId]": {
      "name": string,
      "description": string,
      "url": string,               // Agent base URL (e.g., http://localhost:5020)
      "capabilities": {
        "streaming": boolean,
        "pushNotifications": boolean
      },
      "skills": [{
        "id": string,
        "name": string,
        "description": string,
        "tags": string[]
      }],
      "enabled": boolean          // Enable/disable agent
    }
  }
}
```

### Extension Points

#### Adding a New MCP Server

1. Create server implementation in `McpServers/[ServerName]/`
2. Add configuration entry to `WebApi/mcpconfig.json`:
   ```json
   {
     "mcpServers": {
       "myserver": {
         "name": "My MCP Server",
         "description": "Custom MCP server",
         "transport": "http",
         "url": "http://localhost:6000",
         "enabled": true
       }
     }
   }
   ```
3. (Optional) Create extension method in `Plugins/` for custom initialization
4. Server is automatically discovered and connected on startup

#### Adding a New A2A Agent

1. Create agent implementation in `Agents/[AgentName]/`
2. Implement A2A protocol endpoints (`/tasks`, `/agent-card`)
3. Add configuration entry to `WebApi/agentconfig.json`:
   ```json
   {
     "a2aAgents": {
       "myagent": {
         "name": "My Agent",
         "description": "Custom specialist agent",
         "url": "http://localhost:5003",
         "capabilities": {
           "streaming": false,
           "pushNotifications": false
         },
         "skills": [
           {
             "id": "my_skill",
             "name": "My Skill",
             "description": "What this skill does",
             "tags": ["custom", "skill"]
           }
         ],
         "enabled": true
       }
     }
   }
   ```
4. Agent is automatically discovered and available via `DynamicA2APlugin`

#### Supporting Custom Transport Types

To add support for new MCP transport protocols (e.g., WebSocket):

1. Extend `McpConnector.ConnectServerAsync()` method
2. Add new case in the transport switch statement
3. Implement connection logic for the new transport
4. Update `McpServerConfig` to include new transport-specific properties

### Benefits of This Architecture

1. **Separation of Concerns**: Each agent/server is an independent, self-contained service
2. **Scalability**: Easy to add/remove services without touching core code
3. **Configuration-Driven**: All service management through JSON configuration files
4. **Discoverable**: REST API exposes all available services for inspection
5. **Flexible**: Supports multiple protocols (STDIO, HTTP) and patterns (MCP, A2A)
6. **Maintainable**: Clear boundaries and responsibilities between components
7. **Testable**: Each component can be tested independently

### Future Enhancements

Potential improvements for production use:

- [ ] **Hot-reload Configuration**: Watch configuration files and reload without restart
- [ ] **Health Checks**: Periodic health checks for MCP servers and A2A agents
- [ ] **Load Balancing**: Support multiple instances of the same agent
- [ ] **Metrics & Monitoring**: Prometheus/OpenTelemetry integration
- [ ] **Authentication**: Service-to-service authentication and authorization
- [ ] **WebSocket Support**: Streaming responses from agents
- [ ] **Circuit Breaker**: Fault tolerance for flaky services
- [ ] **Service Registry**: Consul/etcd integration for dynamic service discovery
- [ ] **Retry Policies**: Automatic retry with exponential backoff
- [ ] **Caching**: Response caching for expensive operations

---

## Contributing

This is a proof-of-concept project. Feel free to fork and experiment with different agent configurations and communication patterns.

## Author

lesbass

## Repository

[https://github.com/lesbass/multi-agent-poc](https://github.com/lesbass/multi-agent-poc)
