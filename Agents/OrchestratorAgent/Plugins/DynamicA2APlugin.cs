using System.ComponentModel;
using System.Text;
using A2A;
using Microsoft.SemanticKernel;
using OrchestratorAgent.Configuration;

namespace OrchestratorAgent.Plugins;

/// <summary>
///     Dynamic plugin that automatically creates functions for all registered A2A agents.
///     Reads agent configuration from AgentRegistry and creates corresponding kernel functions.
/// </summary>
public class DynamicA2APlugin
{
    private readonly A2AConfiguration _configuration;
    private readonly Dictionary<string, A2AClient> _clients = new();

    public DynamicA2APlugin(A2AConfiguration configuration)
    {
        _configuration = configuration;
        
        // Initialize clients for all enabled agents
        foreach (var (agentId, agentConfig) in _configuration.A2aAgents.Where(kvp => kvp.Value.Enabled))
        {
            _clients[agentId] = new A2AClient(new Uri(agentConfig.Url));
        }
    }

    /// <summary>
    /// Sends a message to a specific A2A agent by its ID.
    /// </summary>
    [KernelFunction("CallA2AAgent")]
    [Description("Communicates with an A2A agent. Available agents: minnie (Minnie function), paperina (Paperina function)")]
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
