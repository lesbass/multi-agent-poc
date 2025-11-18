namespace OrchestratorAgent.Configuration;

public class A2AAgentSkill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public class A2AAgentCapabilities
{
    public bool Streaming { get; set; }
    public bool PushNotifications { get; set; }
}

public class A2AAgentConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public A2AAgentCapabilities Capabilities { get; set; } = new();
    public A2AAgentSkill[] Skills { get; set; } = Array.Empty<A2AAgentSkill>();
    public bool Enabled { get; set; } = true;
}

public class A2AConfiguration
{
    public Dictionary<string, A2AAgentConfig> A2aAgents { get; set; } = new();
}
