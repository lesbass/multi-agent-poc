using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OrchestratorAgent;

public interface ILLMService
{
    Task<string> ChatAsync(string userMessage, CancellationToken cancellationToken = default);
}

public class LLMService : ILLMService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public LLMService(string apiKey, string modelId = "gpt-4o")
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId, apiKey);
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ChatAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(userMessage);

        var result = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            kernel: _kernel,
            cancellationToken: cancellationToken);

        return result.Content ?? string.Empty;
    }
}
