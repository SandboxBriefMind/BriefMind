using System.Text;
using System.Text.Json;

namespace BriefMind.Server.Infrastructure
{
   public interface IOllamaClient
    {
        Task<string> GenerateAsync(string model, string prompt, bool jsonMode = false);
    }

    public class OllamaClient : IOllamaClient
    {
        private readonly HttpClient _httpClient;
        private const string DefaultOllamaEndpoint = "http://localhost:11434/api/generate";

        public OllamaClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GenerateAsync(string model, string prompt, bool jsonMode = false)
        {
            var requestPayload = new
            {
                model = model,
                prompt = prompt,
                stream = false,
                format = jsonMode ? "json" : null
            };

            string jsonContent = JsonSerializer.Serialize(requestPayload);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(DefaultOllamaEndpoint, httpContent);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            
            return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }
    }
}
