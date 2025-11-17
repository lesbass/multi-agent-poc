using System.ComponentModel;
using ModelContextProtocol.Server;

namespace TopolinoMcpServer.Tools;

[McpServerToolType]
public class TopolinoTool
{
    [McpServerTool]
    [Description("Calcola la funzione Topolino di una stringa")]
    public static string Topolino(string inputString)
    {
        Console.WriteLine($"🪛 Topolino called with {inputString}");
        return $"TOPO_{inputString}_LINO";
    }
}