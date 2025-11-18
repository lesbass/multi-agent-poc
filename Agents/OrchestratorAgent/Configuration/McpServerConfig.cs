namespace OrchestratorAgent.Configuration;

public class McpServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Transport { get; set; } = string.Empty;
    public string? Command { get; set; }
    public string[]? Args { get; set; }
    public string? Url { get; set; }
    public bool Enabled { get; set; } = true;
}

public class McpConfiguration
{
    public Dictionary<string, McpServerConfig> McpServers { get; set; } = new();
}
