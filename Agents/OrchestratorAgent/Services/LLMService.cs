using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using OrchestratorAgent.Models;
using OrchestratorAgent.Plugins;

namespace OrchestratorAgent.Services;

public class LLMService : ILLMService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly Kernel _kernel;
    private readonly Lock _lock = new();
    private readonly Dictionary<string, ChatHistory> _sessions = new();

    public LLMService(IOptions<OpenAIConfiguration> openAiConfigurationOptions)
    {
        var config = openAiConfigurationOptions.Value;
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("OpenAI API Key is not configured");

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(config.Model, config.ApiKey);
        builder.Plugins.AddFromType<MathPlugin>();

        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        AddPlutoMcpServer().GetAwaiter().GetResult();
    }

    public async Task<string> ChatAsync(string userMessage, string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        sessionId ??= "default";

        ChatHistory chatHistory;
        lock (_lock)
        {
            if (!_sessions.TryGetValue(sessionId, out chatHistory!))
            {
                chatHistory = [];
                _sessions[sessionId] = chatHistory;
            }
        }

        chatHistory.AddUserMessage(userMessage);
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel,
            cancellationToken);

        var response = result.Content ?? string.Empty;
        chatHistory.AddAssistantMessage(response);

        return response;
    }

    public void ClearSession(string sessionId)
    {
        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }
    }

    private async Task AddPlutoMcpServer()
    {
        var serverName = "PlutoMcpServer";
        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = serverName,
            Command = "dotnet",
            Arguments =
            [
                "run", "--project", "../McpServers/PlutoMcpServer/PlutoMcpServer.csproj"
            ]
        });

        var mcpClient = await McpClient.CreateAsync(clientTransport);

        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        Console.WriteLine("Found MCP Tools: " + string.Join(", ", tools.Select(t => t.Name)));
        _kernel.Plugins.AddFromFunctions($"{serverName}Functions",
            tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }
}