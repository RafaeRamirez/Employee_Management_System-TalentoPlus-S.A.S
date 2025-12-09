using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

public class AiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> BuildSqlLikeQueryAsync(string question, CancellationToken cancellationToken = default)
    {
        var apiUrl = _configuration["AI__ApiUrl"];
        var apiKey = _configuration["AI__ApiKey"];

        if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            return BasicHeuristics(question);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var payload = new { prompt = $"Transforma esta pregunta en una consulta legible: {question}" };
        var response = await _httpClient.PostAsync(apiUrl, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return BasicHeuristics(question);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(content) ? BasicHeuristics(question) : content;
    }

    private static string BasicHeuristics(string question)
    {
        var lower = question.ToLowerInvariant();
        if (lower.Contains("departamento"))
        {
            return "contar empleados por departamento \"<nombre>\"";
        }
        if (lower.Contains("inactivo"))
        {
            return "contar empleados en estado Inactivo";
        }
        if (lower.Contains("vacaciones"))
        {
            return "contar empleados en estado Vacation";
        }
        if (lower.Contains("auxiliar") || lower.Contains("cargo"))
        {
            return "contar empleados por cargo \"<texto>\"";
        }

        return "contar todos los empleados";
    }
}
