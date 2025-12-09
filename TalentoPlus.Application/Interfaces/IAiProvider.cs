using TalentoPlus.Application.Contracts.Responses;

namespace TalentoPlus.Application.Interfaces;

public interface IAiProvider
{
    Task<string> BuildSqlLikeQueryAsync(string question, CancellationToken cancellationToken = default);
}
