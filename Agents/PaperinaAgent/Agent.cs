using A2A;
using BaseAgent.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using PaperinaAgent.Plugins;

namespace PaperinaAgent;

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
            Name = "PaperinaAgent",
            Instructions = "You specialize in calculating the Paperina function of a given string."
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

        Console.WriteLine("🪛 PaperinaAgent received message: " + userMessage);

        // Get the response from the agent
        var artifact = new Artifact();
        await foreach (var response in _agent.InvokeAsync(userMessage,
                           cancellationToken: cancellationToken))
        {
            var content = response.Message.Content;
            Console.WriteLine("🪛 PaperinaAgent response chunk: " + content);

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

        var paperinaFunctionSkill = new AgentSkill
        {
            Id = "paperina_sk",
            Name = "Paperina Agent",
            Description =
                "Può calcolare la funzione Paperina di una string.",
            Tags = ["paperina", "string", "function", "semantic-kernel"]
        };

        return Task.FromResult(new AgentCard
        {
            Name = "Paperina Agent",
            Description =
                "Semantic Kernel-based string processor specialized in calculating the Paperina function of a string.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [paperinaFunctionSkill]
        });
    }
}