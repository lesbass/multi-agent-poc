using A2A;
using A2A.AspNetCore;
using MinnieAgent;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow }));

var taskManager = new TaskManager();

var echoAgent = new EchoAgent();
echoAgent.Attach(taskManager);

app.MapA2A(taskManager, "/echo");
app.MapWellKnownAgentCard(taskManager, "/echo");
app.MapHttpA2A(taskManager, "/echo");

await app.RunAsync("http://localhost:5020");