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
                    "Você é um assistente inteligente e prestativo.\n" +
                    "PROTOCOLO DE USO DE FERRAMENTAS:\n" +
                    "1. MODO CONVERSA (Padrão): Para cumprimentos ('oi', 'tudo bem'), opiniões, ou perguntas gerais, responda APENAS com texto. NÃO USE FERRAMENTAS.\n" +
                    "2. MODO ONE PIECE: Use a ferramenta de frutas SOMENTE se a pergunta mencionar explicitamente 'One Piece', 'Akuma no Mi', 'Fruta do Diabo' ou nomes específicos de frutas do anime.\n" +
                    "3. MODO AÇÃO: Use as outras ferramentas (como gerar número) SOMENTE quando o usuário solicitar uma ação prática direta.\n" +
                    "CRITÉRIO FINAL: Na dúvida, não use a ferramenta. Responda com texto perguntando se o usuário quer que você busque essa informação."));

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