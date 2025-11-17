using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace PaperinaAgent.Plugins;

internal sealed class MathPlugin
{
    [KernelFunction("Paperina")]
    [Description("Computes the Paperina function of the input string.")]
    public string Paperina(
        [Description("The input string")] string inputString)
    {
        Console.WriteLine($"🪛 Paperina called with {inputString}");

        return $"PAPER_{inputString}_INA";
    }
}