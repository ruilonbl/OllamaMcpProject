Ollama .NET MCP Client
Este projeto é uma implementação de cliente em C# que conecta modelos de IA locais (via Ollama) ao Model Context Protocol (MCP).

Ele demonstra como transformar um modelo de linguagem (LLM) como o llama3.2 em um agente capaz de executar ferramentas reais (Function Calling) e consultar dados externos, utilizando as bibliotecas Microsoft.Extensions.AI e OllamaSharp.

FUNCIONALIDADES
Chat Interativo: Converse com o Llama 3.2 rodando localmente.

Integração MCP (Model Context Protocol): Conexão automática com servidores de ferramentas compatíveis com MCP.

Execução de Ferramentas (Function Calling):

O modelo decide autonomamente quando chamar uma função.

Execução transparente em C# via AIFunctionFactory.

Controle de Comportamento (System Prompts): Regras lógicas para impedir que a IA use ferramentas desnecessariamente em conversas casuais.

PRE-REQUISITOS
.NET SDK 10.0

Ollama instalado e rodando.

Modelo Llama 3.2 baixado:

COMO RODAR

Esteja com o llama 3.2 baixado e o Ollama rodando localmente.

Subistitua "your-mcp-server-url" no código pelo URL do seu servidor MCP. Estara comentado o local que deve ser alterado.