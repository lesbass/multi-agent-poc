using System.ComponentModel;
using System.Text;
using A2A;
using Microsoft.SemanticKernel;

namespace OrchestratorAgent.Plugins;

/// <summary>
///     Plugin that enables communication with MinnieAgent via A2A protocol.
/// </summary>
public class MinnieAgentPlugin
{
    private const string AgentUrl = "http://localhost:5020";
    private readonly A2AClient _client = new(new Uri(AgentUrl));

    [KernelFunction]
    [Description("Calculates the Minnie function of a given string using MinnieAgent. " +
                 "The Minnie function is a specialized string processing function.")]
    public async Task<string> Minnie(
        [Description("The input string to process with the Minnie function")]
        string input,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🪛 MinnieAgentPlugin called with input: " + input);
        try
        {
            var message = new AgentMessage
            {
                Role = MessageRole.User,
                Parts = [new TextPart { Text = input }],
                MessageId = Guid.NewGuid().ToString()
            };

            var response = await _client.SendMessageAsync(
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

            return "No response received from MinnieAgent";
        }
        catch (Exception ex)
        {
            return $"Error communicating with MinnieAgent: {ex.Message}";
        }
    }
}