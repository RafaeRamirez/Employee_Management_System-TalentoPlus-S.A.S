namespace TalentoPlus.Application.Contracts.Responses;

public class AiQueryResponse
{
    public string Question { get; set; } = string.Empty;
    public string SqlLikeQuery { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}
