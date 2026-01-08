using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

const string ollamaEndpoint = "http://localhost:11434";
const string ModelName = "llama3.2";

var ollama = new OllamaApiClient(new Uri(ollamaEndpoint))
{
    SelectedModel = ModelName
};

IChatClient client = new ChatClientBuilder((IChatClient)ollama).UseFunctionInvocation().Build();

var transport = new StdioClientTransport(new()
{
    Command = "dotnet",
    Arguments = ["run", "--project", @"C:\Users\ruilo\OneDrive\Documentos\Porjetos\OllamaMcpProject\OllamaMcpProject"]
});

McpClient mcpClient = await McpClient.CreateAsync(transport);
Console.WriteLine("Ferramentas Disponiveis:");

IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

foreach(var tool in tools)
{
    Console.WriteLine($" * {tool.Name}: {tool.Description}");
}

Console.WriteLine();

var message = new List<ChatMessage>();

while(true)
{
    Console.Write("Prompt:");
    var userImput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userImput)) continue;

    message.Add(new ChatMessage(ChatRole.User, userImput));

    var updates = new List<ChatResponseUpdate>();

    await foreach(var update in client.GetStreamingResponseAsync(message, new()
    {
        Tools = [.. tools]
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
    Console.WriteLine();
    message.AddMessages(updates);
}