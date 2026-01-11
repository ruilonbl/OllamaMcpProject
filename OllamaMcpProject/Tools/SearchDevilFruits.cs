using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OllamaMcpProject.Tools
{
    public class SearchDevilFruits
    {
        [McpServerTool]
        [Description("Busca informações detalhadas sobre uma Akuma no Mi (Devil Fruit) específica em One Piece.")]
        public async Task<string> SearchDevilFruit(string fruitName)
        {
            HttpClient _httpClient = new HttpClient();
            var url = "https://api.api-onepiece.com/v2/characters/en";

            try
            {
                var jsonResponse = await _httpClient.GetStringAsync(url);

                var characters = JsonNode.Parse(jsonResponse)?.AsArray();

                if (characters == null) return "Nenhuma informação encontrada na API.";

                var result = characters.FirstOrDefault(c => c["fruit"]["name"].ToString().Contains(fruitName, StringComparison.OrdinalIgnoreCase)
                );

                if (result == null)
                {
                    return $"Não encontrei nenhuma fruta com o nome '{fruitName}'. Tente o nome em inglês (ex: 'Gum-Gum') ou verifique a grafia.";
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Erro ao buscar informações da fruta: {ex.Message}";
            }
        }
    }
}
