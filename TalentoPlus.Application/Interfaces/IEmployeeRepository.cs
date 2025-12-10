using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync(string? ownerUserId = null, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? ownerUserId = null, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(EmployeeStatus status, string? ownerUserId = null, CancellationToken cancellationToken = default);
    Task<int> CountByDepartmentNameAsync(string departmentName, string? ownerUserId = null, CancellationToken cancellationToken = default);
    Task<int> CountByPositionAsync(string positionContains, string? ownerUserId = null, CancellationToken cancellationToken = default);
}
