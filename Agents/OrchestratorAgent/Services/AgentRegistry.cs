using System.Net.Http.Json;
using A2A;
using Microsoft.Extensions.Logging;
using OrchestratorAgent.Configuration;

namespace OrchestratorAgent.Services;

public interface IAgentRegistry
{
    Task<IEnumerable<A2AAgentConfig>> ListAgentsAsync(CancellationToken cancellationToken = default);
    Task<A2AAgentConfig?> GetAgentAsync(string agentId, CancellationToken cancellationToken = default);
    Task<AgentCard?> GetAgentCardAsync(string agentId, CancellationToken cancellationToken = default);
    Task<string> DelegateTaskAsync(string agentId, string message, CancellationToken cancellationToken = default);
    Task RegisterAgentAsync(string agentId, A2AAgentConfig config, CancellationToken cancellationToken = default);
    Task UnregisterAgentAsync(string agentId, CancellationToken cancellationToken = default);
}

public class AgentRegistry : IAgentRegistry
{
    private readonly A2AConfiguration _configuration;
    private readonly ILogger<AgentRegistry> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, AgentCard> _agentCards = new();

    public AgentRegistry(A2AConfiguration configuration, ILogger<AgentRegistry> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public Task<IEnumerable<A2AAgentConfig>> ListAgentsAsync(CancellationToken cancellationToken = default)
    {
        var agents = _configuration.A2aAgents
            .Where(kvp => kvp.Value.Enabled)
            .Select(kvp => kvp.Value);

        return Task.FromResult(agents);
    }

    public Task<A2AAgentConfig?> GetAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        _configuration.A2aAgents.TryGetValue(agentId, out var agent);
        return Task.FromResult(agent);
    }

    public async Task<AgentCard?> GetAgentCardAsync(string agentId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_agentCards.TryGetValue(agentId, out var cachedCard))
        {
            return cachedCard;
        }

        var agent = await GetAgentAsync(agentId, cancellationToken);
        if (agent == null || !agent.Enabled)
        {
            return null;
        }

        try
        {
            _logger.LogInformation("Fetching agent card from: {AgentUrl}", agent.Url);
            
            var response = await _httpClient.GetAsync($"{agent.Url}/agent-card", cancellationToken);
            response.EnsureSuccessStatusCode();

            var agentCard = await response.Content.ReadFromJsonAsync<AgentCard>(cancellationToken);
            
            if (agentCard != null)
            {
                _agentCards[agentId] = agentCard;
            }

            return agentCard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch agent card for {AgentId}", agentId);
            return null;
        }
    }

    public async Task<string> DelegateTaskAsync(string agentId, string message, CancellationToken cancellationToken = default)
    {
        var agent = await GetAgentAsync(agentId, cancellationToken);
        if (agent == null || !agent.Enabled)
        {
            throw new ArgumentException($"Agent '{agentId}' not found or disabled.");
        }

        try
        {
            _logger.LogInformation("Delegating task to agent: {AgentName}", agent.Name);

            // Create task request
            var taskRequest = new
            {
                sessionId = Guid.NewGuid().ToString(),
                input = new
                {
                    text = message
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{agent.Url}/tasks", taskRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var taskResponse = await response.Content.ReadFromJsonAsync<AgentTaskResponse>(cancellationToken);
            
            if (taskResponse?.Artifacts != null && taskResponse.Artifacts.Count > 0)
            {
                var artifact = taskResponse.Artifacts[0];
                if (artifact.Parts != null && artifact.Parts.Count > 0)
                {
                    var textPart = artifact.Parts[0] as TextPart;
                    return textPart?.Text ?? "No response from agent";
                }
            }

            return "Task delegated but no response received";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delegate task to agent {AgentId}", agentId);
            throw;
        }
    }

    public Task RegisterAgentAsync(string agentId, A2AAgentConfig config, CancellationToken cancellationToken = default)
    {
        _configuration.A2aAgents[agentId] = config;
        _agentCards.Remove(agentId); // Clear cache
        _logger.LogInformation("Registered agent: {AgentId}", agentId);
        return Task.CompletedTask;
    }

    public Task UnregisterAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        if (_configuration.A2aAgents.Remove(agentId))
        {
            _agentCards.Remove(agentId);
            _logger.LogInformation("Unregistered agent: {AgentId}", agentId);
        }

        return Task.CompletedTask;
    }

    private class AgentTaskResponse
    {
        public List<Artifact>? Artifacts { get; set; }
    }
}
