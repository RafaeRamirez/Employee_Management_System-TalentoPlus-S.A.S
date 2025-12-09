using TalentoPlus.Application.Contracts.Requests;

namespace TalentoPlus.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterEmployeeAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
