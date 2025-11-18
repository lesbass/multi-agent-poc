using BaseAgent.Models;
using BaseAgent.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OrchestratorAgent.Configuration;
using OrchestratorAgent.Plugins;
using OrchestratorAgent.Services;

namespace OrchestratorAgent;

public static class OrchestratorAgentRegistration
{
    public static void AddOrchestratorAgentServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetSection(OpenAIConfiguration.SectionName)
            .Get<OpenAIConfiguration>();
        if (configuration is null)
        {
            throw new ArgumentException(
                "OpenAI configuration section must be provided when Provider is set to 'OpenAI' or not specified");
        }

        // Load MCP configuration
        var mcpConfig = new McpConfiguration();
        builder.Configuration.Bind("mcpServers", mcpConfig.McpServers);
        
        // Try to load from external file if section is empty
        if (mcpConfig.McpServers.Count == 0)
        {
            var mcpConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "mcpconfig.json");
            if (File.Exists(mcpConfigFile))
            {
                var mcpConfigBuilder = new ConfigurationBuilder()
                    .AddJsonFile(mcpConfigFile, optional: false);
                var mcpConfiguration = mcpConfigBuilder.Build();
                mcpConfiguration.Bind(mcpConfig);
            }
        }

        // Load A2A configuration
        var a2aConfig = new A2AConfiguration();
        builder.Configuration.Bind("a2aAgents", a2aConfig.A2aAgents);
        
        // Try to load from external file if section is empty
        if (a2aConfig.A2aAgents.Count == 0)
        {
            var a2aConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "agentconfig.json");
            if (File.Exists(a2aConfigFile))
            {
                var a2aConfigBuilder = new ConfigurationBuilder()
                    .AddJsonFile(a2aConfigFile, optional: false);
                var a2aConfiguration = a2aConfigBuilder.Build();
                a2aConfiguration.Bind(a2aConfig);
            }
        }

        builder.Services.AddSingleton(mcpConfig);
        builder.Services.AddSingleton(a2aConfig);
        
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IMcpConnector, McpConnector>();
        builder.Services.AddSingleton<IAgentRegistry, AgentRegistry>();

        builder.Services.AddSingleton(serviceProvider =>
        {
            var mcpConnector = serviceProvider.GetRequiredService<IMcpConnector>();
            
            var llmService = new LLMService(configuration,
                plugins =>
                {
                    plugins.AddFromType<MathPlugin>();
                    
                    // Add dynamic A2A plugin that auto-configures from agentconfig.json
                    plugins.AddFromObject(new DynamicA2APlugin(a2aConfig), "A2AAgents");
                },
                kernel =>
                {
                    // Dynamically connect enabled MCP servers
                    var servers = mcpConnector.ListServersAsync().Result;
                    foreach (var server in servers.Where(s => s.Enabled))
                    {
                        var serverId = mcpConfig.McpServers.First(kvp => kvp.Value == server).Key;
                        try
                        {
                            // Pass the kernel directly since it's already built
                            mcpConnector.ConnectServerAsync(serverId, kernel).Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to connect MCP server {server.Name}: {ex.Message}");
                        }
                    }
                });
            
            return llmService;
        });
    }

    public static void UseOrchestratorAgent(this WebApplication app)
    {
        app.MapPost("/chat", async (ChatRequest request, LLMService llmService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                var response = await llmService.ChatAsync(request.Message, request.SessionId, cancellationToken);
                return Results.Ok(new ChatResponse { Message = response });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        });

        // MCP Management Endpoints
        app.MapGet("/mcp/servers", async (IMcpConnector mcpConnector, CancellationToken cancellationToken) =>
        {
            var servers = await mcpConnector.ListServersAsync(cancellationToken);
            return Results.Ok(servers);
        }).WithName("ListMcpServers");

        // A2A Agent Management Endpoints
        app.MapGet("/agents", async (IAgentRegistry agentRegistry, CancellationToken cancellationToken) =>
        {
            var agents = await agentRegistry.ListAgentsAsync(cancellationToken);
            return Results.Ok(agents);
        }).WithName("ListAgents");

        app.MapGet("/agents/{agentId}", async (string agentId, IAgentRegistry agentRegistry, CancellationToken cancellationToken) =>
        {
            var agent = await agentRegistry.GetAgentAsync(agentId, cancellationToken);
            return agent != null ? Results.Ok(agent) : Results.NotFound();
        }).WithName("GetAgent");

        app.MapGet("/agents/{agentId}/card", async (string agentId, IAgentRegistry agentRegistry, CancellationToken cancellationToken) =>
        {
            var agentCard = await agentRegistry.GetAgentCardAsync(agentId, cancellationToken);
            return agentCard != null ? Results.Ok(agentCard) : Results.NotFound();
        }).WithName("GetAgentCard");

        app.MapPost("/agents/{agentId}/delegate", async (
            string agentId, 
            DelegateTaskRequest request,
            IAgentRegistry agentRegistry, 
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                var response = await agentRegistry.DelegateTaskAsync(agentId, request.Message, cancellationToken);
                return Results.Ok(new { response });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }).WithName("DelegateTask");
    }

    public record DelegateTaskRequest(string Message);
}