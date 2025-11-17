using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OrchestratorAgent;

public interface ILLMService
{
    Task<string> ChatAsync(string userMessage, string? sessionId = null, CancellationToken cancellationToken = default);
    void ClearSession(string sessionId);
}

public class LLMService : ILLMService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly Kernel _kernel;
    private readonly object _lock = new();
    private readonly Dictionary<string, ChatHistory> _sessions = new();

    public LLMService(string apiKey, string modelId = "gpt-4o")
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId, apiKey);
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
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

        var result = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            kernel: _kernel,
            cancellationToken: cancellationToken);

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
}