using System.ComponentModel;
using System.Text;
using A2A;
using Microsoft.SemanticKernel;
using OrchestratorAgent.Configuration;

namespace OrchestratorAgent.Plugins;

public class DynamicA2APlugin
{
    private readonly A2AConfiguration _configuration;
    private readonly Dictionary<string, A2AClient> _clients = new();
    private readonly string _agentListDescription;

    public DynamicA2APlugin(A2AConfiguration configuration)
    {
        _configuration = configuration;
        
        // Initialize clients for all enabled agents
        var enabledAgents = _configuration.A2aAgents.Where(kvp => kvp.Value.Enabled).ToList();
        
        foreach (var (agentId, agentConfig) in enabledAgents)
        {
            _clients[agentId] = new A2AClient(new Uri(agentConfig.Url));
        }
        
        // Build dynamic description with available agents
        var agentDescriptions = enabledAgents
            .Select(kvp => $"{kvp.Key} ({kvp.Value.Description})")
            .ToList();
        
        _agentListDescription = agentDescriptions.Count > 0
            ? $"Communicates with an A2A agent. Available agents: {string.Join(", ", agentDescriptions)}"
            : "Communicates with an A2A agent. No agents currently available.";

        System.Console.WriteLine($"🪛 DynamicA2APlugin initialized with agents: {string.Join(", ", _clients.Keys)} \n {_agentListDescription}");
    }

    /// <summary>
    /// Sends a message to a specific A2A agent by its ID.
    /// </summary>
    [KernelFunction("CallA2AAgent")]
    [Description("Communicates with an A2A agent")]
    public async Task<string> CallAgent(
        [Description("The ID of the agent to call (e.g., 'minnie', 'paperina')")]
        string agentId,
        [Description("The message or input to send to the agent")]
        string input,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🪛 DynamicA2APlugin calling agent '{agentId}' with input: {input}");

        if (!_clients.TryGetValue(agentId, out var client))
        {
            var availableAgents = string.Join(", ", _clients.Keys);
            return $"Agent '{agentId}' not found or not enabled. Available agents: {availableAgents}";
        }

        try
        {
            var message = new AgentMessage
            {
                Role = MessageRole.User,
                Parts = [new TextPart { Text = input }],
                MessageId = Guid.NewGuid().ToString()
            };

            var response = await client.SendMessageAsync(
                new MessageSendParams { Message = message },
                cancellationToken);

            if (response is AgentTask task && task.Artifacts?.Count > 0)
            {
                var result = new StringBuilder();
                foreach (var artifact in task.Artifacts)
                {
                    foreach (var part in artifact.Parts)
                    {
                        if (part is TextPart textPart)
                        {
                            result.Append(textPart.Text);
                        }
                    }
                }

                return result.ToString();
            }

            if (response is AgentMessage agentMessage)
            {
                var result = new StringBuilder();
                foreach (var part in agentMessage.Parts)
                {
                    if (part is TextPart textPart)
                    {
                        result.Append(textPart.Text);
                    }
                }

                return result.ToString();
            }

            return $"No response received from agent '{agentId}'";
        }
        catch (Exception ex)
        {
            return $"Error communicating with agent '{agentId}': {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the list of available A2A agents.
    /// </summary>
    [KernelFunction("ListA2AAgents")]
    [Description("Lists all available A2A agents with their capabilities")]
    public string ListAgents()
    {
        var agentList = new StringBuilder();
        agentList.AppendLine("Available A2A Agents:");
        
        foreach (var (agentId, agentConfig) in _configuration.A2aAgents.Where(kvp => kvp.Value.Enabled))
        {
            agentList.AppendLine($"- {agentId}: {agentConfig.Description}");
            if (agentConfig.Skills.Length > 0)
            {
                agentList.AppendLine($"  Skills: {string.Join(", ", agentConfig.Skills.Select(s => s.Name))}");
            }
        }

        return agentList.ToString();
    }
}
