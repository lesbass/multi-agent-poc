using A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using MinnieAgent.Models;
using MinnieAgent.Plugins;

namespace MinnieAgent;

public class Agent : IDisposable
{
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public Agent(OpenAIConfiguration openAiConfig)
    {
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(openAiConfig.Model, openAiConfig.ApiKey);

        builder.Plugins.AddFromType<MathPlugin>();

        var kernel = builder.Build();

        _agent = new ChatCompletionAgent
        {
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            Name = "MinnieAgent",
            Instructions = "You specialize in calculating the Minnie function of a given string."
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        taskManager.OnTaskCreated = ExecuteAgentTaskAsync;
        taskManager.OnTaskUpdated = ExecuteAgentTaskAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    public async Task ExecuteAgentTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        if (_taskManager == null)
        {
            throw new InvalidOperationException("TaskManager is not attached.");
        }

        await _taskManager.UpdateStatusAsync(task.Id, TaskState.Working, cancellationToken: cancellationToken);

        // Get message from the user
        var userMessage = task.History!.Last().Parts.First().AsTextPart().Text;

        Console.WriteLine("🪛 MinnieAgent received message: " + userMessage);

        // Get the response from the agent
        var artifact = new Artifact();
        await foreach (var response in _agent.InvokeAsync(userMessage,
                           cancellationToken: cancellationToken))
        {
            var content = response.Message.Content;
            Console.WriteLine("🪛 MinnieAgent response chunk: " + content);

            artifact.Parts.Add(new TextPart { Text = content! });
        }

        // Return as artifacts
        await _taskManager.ReturnArtifactAsync(task.Id, artifact, cancellationToken);
        await _taskManager.UpdateStatusAsync(task.Id, TaskState.Completed, cancellationToken: cancellationToken);
    }

    public static Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities
        {
            Streaming = false,
            PushNotifications = false
        };

        var minnieFunctionSkill = new AgentSkill
        {
            Id = "minnie_sk",
            Name = "Minnie Agent",
            Description =
                "Può calcolare la funzione Minnie di una string.",
            Tags = ["minnie", "string", "function", "semantic-kernel"]
        };

        return Task.FromResult(new AgentCard
        {
            Name = "Minnie Agent",
            Description =
                "Semantic Kernel-based string processor specialized in calculating the Minnie function of a string.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [minnieFunctionSkill]
        });
    }
}