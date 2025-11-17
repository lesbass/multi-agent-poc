using System.ComponentModel;
using ModelContextProtocol.Server;

namespace PlutoMcpServer.Tools;

[McpServerToolType]
public class PlutoTool
{
    [McpServerTool]
    [Description("Calcola la funzione Pluto di una stringa")]
    public static string Pluto(string inputString)
    {
        Console.WriteLine($"🪛 Pluto called with {inputString}");
        return $"PLU_{inputString}_TO";
    }
}