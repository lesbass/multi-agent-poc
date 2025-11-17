namespace OrchestratorAgent.Models;

internal record ChatRequest(string Message, string? SessionId = null);