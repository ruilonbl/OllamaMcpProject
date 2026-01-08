using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;
using System.Text.Json;

public class Program
{
    public static async Task Main(string[] args)
    {
        const string ollamaEndpoint = "http://localhost:11434";
        const string ModelName = "llama3.2";

        var ollama = new OllamaApiClient(new Uri(ollamaEndpoint))
        {
            SelectedModel = ModelName
        };

        IChatClient client = new ChatClientBuilder((IChatClient)ollama)
            .UseFunctionInvocation()
            .Build();

        var transport = new StdioClientTransport(new()
        {
            Command = "dotnet",
            Arguments = ["run", "--project", @"C:\Users\ruilo\OneDrive\Documentos\Porjetos\OllamaMcpProject\OllamaMcpProject"]
        });

        McpClient mcpClient = await McpClient.CreateAsync(transport);
        Console.WriteLine("Conectado ao MCP Server.");

        Console.WriteLine("Carregando ferramentas...");
        IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();

        var aiTools = new List<AITool>();

        foreach (var tool in mcpTools)
        {
            Console.WriteLine($" * Carregada: {tool.Name}");

            var aiFunction = AIFunctionFactory.Create(async (string argumentsJson) =>
            {
                try
                {
                    var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson);

                    var result = await mcpClient.CallToolAsync(tool.Name, arguments ?? new());

                    return result.Content.ToString();
                }
                catch (Exception ex)
                {
                    return $"Erro ao executar ferramenta: {ex.Message}";
                }
            },
            name: tool.Name,
            description: tool.Description);

            aiTools.Add(aiFunction);
        }

        Console.WriteLine("\nPronto para conversar! (Digite 'sair' para encerrar)\n");
        var messages = new List<ChatMessage>();

        messages.Add(new ChatMessage(ChatRole.System,
            "Você é um assistente inteligente. " +
            "REGRAS DE COMPORTAMENTO: " +
            "1. Para cumprimentos (oi, boa noite), conversas casuais ou perguntas gerais, responda APENAS com texto. NÃO chame ferramentas. " +
            "2. Use as ferramentas APENAS quando o usuário pedir explicitamente uma ação que a ferramenta resolve (ex: 'gere um número', 'busque tal dado'). " +
            "3. Se não tiver certeza, pergunte ao usuário antes de usar uma ferramenta."));

        while (true)
        {
            Console.Write("\nVocê: ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) continue;
            if (userInput.ToLower() == "sair") break;

            messages.Add(new ChatMessage(ChatRole.User, userInput));

            Console.Write("Llama: ");

            try
            {
                var updates = new List<ChatResponseUpdate>();

                await foreach (var update in client.GetStreamingResponseAsync(messages, new ChatOptions
                {
                    Tools = aiTools
                }))
                {
                    foreach (var content in update.Contents)
                    {
                        if (content is TextContent textContent)
                        {
                            Console.Write(textContent.Text);
                        }
                    }
                    updates.Add(update);
                }

                messages.AddMessages(updates);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErro: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}