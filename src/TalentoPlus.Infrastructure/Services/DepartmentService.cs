using Microsoft.EntityFrameworkCore;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(cancellationToken);
        return departments.Select(d => new DepartmentDto { Id = d.Id, Name = d.Name });
    }

    public async Task<DepartmentDto> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var department = new Department { Id = Guid.NewGuid(), Name = name };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);
        return new DepartmentDto { Id = department.Id, Name = department.Name };
    }
}
