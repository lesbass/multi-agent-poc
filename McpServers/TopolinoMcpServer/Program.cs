using TopolinoMcpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<TopolinoTool>();

var app = builder.Build();

app.UseStaticFiles();

app.MapMcp();

await app.RunAsync("http://localhost:5010");