using Microsoft.AspNetCore.Http;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Contracts.Responses;
using TalentoPlus.Application.DTOs;

namespace TalentoPlus.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(EmployeeUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ImportFromExcelAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePdfAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);
    Task<AiQueryResponse> AskAiAsync(string question, CancellationToken cancellationToken = default);
}
