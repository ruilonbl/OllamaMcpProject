using System.ComponentModel;
using ModelContextProtocol.Server;

internal class RandomNumberTools
{
    [McpServerTool]
    [Description("Gera um numero aleatorio entre os valores minimo e maximo especificados.")]
    public int GetRandomNumber(
        [Description("Minimum value (inclusive)")] int min = 0,
        [Description("Maximum value (exclusive)")] int max = 100)
    {
        return Random.Shared.Next(min, max);
    }
}
