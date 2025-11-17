using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace OrchestratorAgent.Plugins;

public static class PlutoMcpPlugin
{
    private const string ServerName = "PlutoMcpServer";

    public static void AddPlutoMcpServer(Kernel kernel)
    {
        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = ServerName,
            Command = "dotnet",
            Arguments =
            [
                "run", "--project", "../McpServers/PlutoMcpServer/PlutoMcpServer.csproj"
            ]
        });

        var mcpClient = McpClient.CreateAsync(clientTransport).GetAwaiter().GetResult();

        var tools = mcpClient.ListToolsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        Console.WriteLine($"{ServerName} -> Found MCP Tools: {string.Join(", ", tools.Select(t => t.Name))}");
        kernel.Plugins.AddFromFunctions($"{ServerName}Functions",
            tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }
}