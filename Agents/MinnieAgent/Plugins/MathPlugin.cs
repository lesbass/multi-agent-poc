using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MinnieAgent.Plugins;

internal sealed class MathPlugin
{
    [KernelFunction("Minnie")]
    [Description("Computes the Minnie function of the input string.")]
    public string Minnie(
        [Description("The input string")] string inputString)
    {
        Console.WriteLine($"🪛 Minnie called with {inputString}");

        return $"MIN_{inputString}_NIE";
    }
}