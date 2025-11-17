using A2A;
using A2A.AspNetCore;
using MinnieAgent;
using MinnieAgent.Models;

var builder = WebApplication.CreateBuilder(args);

var openAiSection = builder.Configuration.GetSection(OpenAIConfiguration.SectionName).Get<OpenAIConfiguration>();
var app = builder.Build();

var agent = new Agent(openAiSection ?? throw new ArgumentException(
    "OpenAI configuration section must be provided when Provider is set to 'OpenAI' or not specified"));
var taskManager = new TaskManager();
agent.Attach(taskManager);

app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");

await app.RunAsync("http://localhost:5020");