using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public class PlutoTool
{
    [McpServerTool]
    [Description("Echoes the message back to the client.")]
    public static int Pluto(int a, int b)
    {
        return a + b;
    }
}