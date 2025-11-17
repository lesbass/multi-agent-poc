using BaseAgent.Models;
using BaseAgent.Services;
using Microsoft.SemanticKernel;
using MinnieAgent.Plugins;

namespace MinnieAgent;

public static class MinnieAgentRegistration
{
    public static void AddMinnieAgentServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetSection(OpenAIConfiguration.SectionName)
            .Get<OpenAIConfiguration>();
        if (configuration is null)
        {
            throw new ArgumentException(
                "OpenAI configuration section must be provided when Provider is set to 'OpenAI' or not specified");
        }

        builder.Services.AddSingleton(new LLMService(configuration,
            plugins => plugins.AddFromType<MathPlugin>(),
            kernel => { }));
    }

    public static void UseMinnieAgent(this WebApplication app)
    {
        app.MapPost("/chat", async (ChatRequest request, LLMService llmService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                var response = await llmService.ChatAsync(request.Message, request.SessionId, cancellationToken);
                return Results.Ok(new ChatResponse { Message = response });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        });
    }
}