namespace OrchestratorAgent.Services;

public interface ILLMService
{
    Task<string> ChatAsync(string userMessage, string? sessionId = null, CancellationToken cancellationToken = default);
    void ClearSession(string sessionId);
}