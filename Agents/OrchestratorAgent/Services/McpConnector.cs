using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OrchestratorAgent.Configuration;
using OrchestratorAgent.Plugins;

namespace OrchestratorAgent.Services;

public interface IMcpConnector
{
    Task<IEnumerable<McpServerConfig>> ListServersAsync(CancellationToken cancellationToken = default);
    Task ConnectServerAsync(string serverId, Kernel kernel, CancellationToken cancellationToken = default);
    Task DisconnectServerAsync(string serverId, CancellationToken cancellationToken = default);
}

public class McpConnector : IMcpConnector
{
    private readonly McpConfiguration _configuration;
    private readonly ILogger<McpConnector> _logger;
    private readonly Dictionary<string, bool> _connectedServers = new();

    public McpConnector(McpConfiguration configuration, ILogger<McpConnector> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<IEnumerable<McpServerConfig>> ListServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = _configuration.McpServers
            .Where(kvp => kvp.Value.Enabled)
            .Select(kvp => kvp.Value);

        return Task.FromResult(servers);
    }

    public Task ConnectServerAsync(string serverId, Kernel kernel, CancellationToken cancellationToken = default)
    {
        if (!_configuration.McpServers.TryGetValue(serverId, out var serverConfig))
        {
            throw new ArgumentException($"MCP server '{serverId}' not found in configuration.");
        }

        if (!serverConfig.Enabled)
        {
            throw new InvalidOperationException($"MCP server '{serverId}' is disabled.");
        }

        _logger.LogInformation("Connecting to MCP server: {ServerName} ({Transport})", 
            serverConfig.Name, serverConfig.Transport);

        // Dynamic connection based on transport type
        switch (serverConfig.Transport.ToLower())
        {
            case "stdio":
                ConnectStdioServer(serverId, serverConfig, kernel);
                break;
            case "http":
                ConnectHttpServer(serverId, serverConfig, kernel);
                break;
            default:
                throw new NotSupportedException($"Transport type '{serverConfig.Transport}' is not supported.");
        }

        _connectedServers[serverId] = true;
        _logger.LogInformation("Successfully connected to MCP server: {ServerName}", serverConfig.Name);

        return Task.CompletedTask;
    }

    public Task DisconnectServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        if (_connectedServers.ContainsKey(serverId))
        {
            _connectedServers.Remove(serverId);
            _logger.LogInformation("Disconnected from MCP server: {ServerId}", serverId);
        }

        return Task.CompletedTask;
    }

    private void ConnectStdioServer(string serverId, McpServerConfig config, Kernel kernel)
    {
        if (string.IsNullOrEmpty(config.Command))
        {
            throw new InvalidOperationException($"STDIO server '{serverId}' requires a command.");
        }

        // Add STDIO MCP server
        switch (serverId.ToLower())
        {
            case "pluto":
                kernel.AddPlutoMcpServer();
                break;
            default:
                _logger.LogWarning("STDIO server '{ServerId}' does not have a specific implementation.", serverId);
                break;
        }
    }

    private void ConnectHttpServer(string serverId, McpServerConfig config, Kernel kernel)
    {
        if (string.IsNullOrEmpty(config.Url))
        {
            throw new InvalidOperationException($"HTTP server '{serverId}' requires a URL.");
        }

        // Add HTTP MCP server
        switch (serverId.ToLower())
        {
            case "topolino":
                kernel.AddTopolinoMcpServer();
                break;
            default:
                _logger.LogWarning("HTTP server '{ServerId}' does not have a specific implementation.", serverId);
                break;
        }
    }
}
