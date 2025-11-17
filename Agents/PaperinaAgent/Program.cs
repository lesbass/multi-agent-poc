using A2A;
using A2A.AspNetCore;
using BaseAgent.Models;
using PaperinaAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddPaperinaAgentServices();

var configuration = builder.Configuration.GetSection(OpenAIConfiguration.SectionName).Get<OpenAIConfiguration>();
if (configuration is null)
{
    throw new ArgumentException(
        "OpenAI configuration section must be provided when Provider is set to 'OpenAI' or not specified");
}

var app = builder.Build();

var agent = new Agent(configuration);
var taskManager = new TaskManager();
agent.Attach(taskManager);

app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");

app.UsePaperinaAgent();

await app.RunAsync("http://localhost:5030");