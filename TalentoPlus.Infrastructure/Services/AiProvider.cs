using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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
            // No external AI configured; fallback to deterministic heuristics.
            return BasicHeuristics(question);
        }

        // Gemini generateContent payload
        var prompt = BuildPrompt(question);
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var uri = apiUrl.Contains("?")
            ? $"{apiUrl}&key={apiKey}"
            : $"{apiUrl}?key={apiKey}";

        var response = await _httpClient.PostAsync(
            uri,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return BasicHeuristics(question);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var aiText = ExtractGeminiText(content);
        return string.IsNullOrWhiteSpace(aiText) ? BasicHeuristics(question) : MapToCanonical(aiText);
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

    private static string BuildPrompt(string question)
    {
        // Constrain the model to return a simple intent that we will map to deterministic queries.
        return $@"
Eres un clasificador. Dada una pregunta en español sobre empleados, responde SOLO con uno de estos formatos:
- DEPARTAMENTO:""<nombre_departamento>""
- ESTADO:INACTIVO | ESTADO:VACACIONES | ESTADO:ACTIVO
- CARGO:""<texto_cargo>""
- TODO

Ejemplos:
""¿Cuántos empleados pertenecen al departamento de Tecnología?"" -> DEPARTAMENTO:""Tecnología""
""¿Cuántos están inactivos?"" -> ESTADO:INACTIVO
""¿Cuántos auxiliares hay?"" -> CARGO:""auxiliar""
""Total de empleados"" -> TODO

Pregunta: {question}
Responde solo con el formato indicado, sin explicaciones.";
    }

    private static string ExtractGeminiText(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return text ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string MapToCanonical(string aiText)
    {
        var text = aiText.Trim().ToUpperInvariant();
        if (text.StartsWith("DEPARTAMENTO"))
        {
            var value = ExtractQuoted(text);
            return $"contar empleados por departamento \"{value}\"";
        }

        if (text.StartsWith("ESTADO"))
        {
            if (text.Contains("INACTIVO"))
                return "contar empleados en estado Inactivo";
            if (text.Contains("VACACION"))
                return "contar empleados en estado Vacation";
            return "contar empleados en estado Active";
        }

        if (text.StartsWith("CARGO"))
        {
            var value = ExtractQuoted(text);
            return $"contar empleados por cargo \"{value}\"";
        }

        return "contar todos los empleados";
    }

    private static string ExtractQuoted(string text)
    {
        var start = text.IndexOf('"');
        var end = text.LastIndexOf('"');
        if (start >= 0 && end > start)
        {
            return text.Substring(start + 1, end - start - 1);
        }
        return string.Empty;
    }
}
