using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace OrchestratorAgent.Plugins;

public static class TopolinoMcpPlugin
{
    private const string ServerName = "TopolinoMcpServer";
    private const string ServerUrl = "http://localhost:5010";

    public static void AddTopolinoMcpServer(this Kernel kernel)
    {
        var mcpClient = McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(ServerUrl),
                TransportMode = HttpTransportMode.AutoDetect
            })).GetAwaiter().GetResult();

        var tools = mcpClient.ListToolsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        Console.WriteLine($"{ServerName} -> Found MCP Tools: {string.Join(", ", tools.Select(t => t.Name))}");
        kernel.Plugins.AddFromFunctions($"{ServerName}Functions",
            tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }
}