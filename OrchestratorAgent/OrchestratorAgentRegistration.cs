using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchestratorAgent.Models;
using OrchestratorAgent.Services;

namespace OrchestratorAgent;

public static class OrchestratorAgentRegistration
{
    public static void AddOrchestratorAgentServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OpenAIConfiguration>(
            builder.Configuration.GetSection(OpenAIConfiguration.SectionName));

        builder.Services.AddSingleton<ILLMService, LLMService>();
    }

    public static void UseOrchestratorAgent(this WebApplication app)
    {
        app.MapPost("/chat", async (ChatRequest request, ILLMService llmService, CancellationToken cancellationToken) =>
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
            })
            .WithName("Chat");

        app.MapDelete("/chat/{sessionId}", (string sessionId, ILLMService llmService) =>
            {
                llmService.ClearSession(sessionId);
                return Results.Ok(new { message = "Session cleared" });
            })
            .WithName("ClearChatSession");
    }
}