using System.ComponentModel;
using System.Text;
using A2A;
using Microsoft.SemanticKernel;

namespace OrchestratorAgent.Plugins;

/// <summary>
///     Plugin that enables communication with PaperinaAgent via A2A protocol.
/// </summary>
public class PaperinaAgentPlugin
{
    private const string AgentUrl = "http://localhost:5030";
    private readonly A2AClient _client = new(new Uri(AgentUrl));

    [KernelFunction]
    [Description("Calculates the Paperina function of a given string using PaperinaAgent. " +
                 "The Paperina function is a specialized string processing function.")]
    public async Task<string> Paperina(
        [Description("The input string to process with the Paperina function")]
        string input,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🪛 PaperinaAgentPlugin called with input: " + input);
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

            return "No response received from PaperinaAgent";
        }
        catch (Exception ex)
        {
            return $"Error communicating with PaperinaAgent: {ex.Message}";
        }
    }
}