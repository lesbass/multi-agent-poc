using BaseAgent.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BaseAgent.Services;

public class LLMService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly Kernel _kernel;
    private readonly Lock _lock = new();
    private readonly Dictionary<string, ChatHistory> _sessions = new();

    public LLMService(OpenAIConfiguration config,
        Action<IKernelBuilderPlugins> pluginInitializer,
        Action<Kernel> kernelSetup)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("OpenAI API Key is not configured");

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(config.Model, config.ApiKey);
        pluginInitializer(builder.Plugins);

        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        kernelSetup(_kernel);
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
}