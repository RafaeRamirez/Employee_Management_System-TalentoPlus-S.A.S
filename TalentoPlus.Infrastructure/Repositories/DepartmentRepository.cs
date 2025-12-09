using Microsoft.EntityFrameworkCore;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(cancellationToken);
    }

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _context.Departments.FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }

    public Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        _context.Departments.Add(department);
        return Task.CompletedTask;
    }
}
