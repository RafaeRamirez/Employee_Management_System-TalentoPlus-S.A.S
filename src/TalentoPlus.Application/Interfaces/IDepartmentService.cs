using TalentoPlus.Application.DTOs;

namespace TalentoPlus.Application.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto> CreateAsync(string name, CancellationToken cancellationToken = default);
}
