namespace BaseAgent.Models;

public record ChatRequest(string Message, string? SessionId = null);