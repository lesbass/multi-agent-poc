using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace OrchestratorAgent.Plugins;

internal sealed class MathPlugin
{
    [KernelFunction("Pippo")]
    [Description("Multiplies two numbers together")]
    public string Pippo(
        [Description("The first number")] int number1,
        [Description("The second number")] int number2)
    {
        Console.WriteLine($"Pippo called with {number1} and {number2}");

        return $"PIP_{number1 * number2}_PO";
    }
}