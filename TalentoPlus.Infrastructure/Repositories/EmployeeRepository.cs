using Microsoft.EntityFrameworkCore;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Eager load Department to avoid N+1 in service/UI.
        return await _context.Employees.Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees.Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await _context.Employees.Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Document == document, cancellationToken);
    }

    public Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Add(employee);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(employee);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _context.Employees.CountAsync(cancellationToken);
    }

    public Task<int> CountByStatusAsync(EmployeeStatus status, CancellationToken cancellationToken = default)
    {
        return _context.Employees.CountAsync(e => e.Status == status, cancellationToken);
    }

    public Task<int> CountByDepartmentNameAsync(string departmentName, CancellationToken cancellationToken = default)
    {
        var lower = departmentName.ToLower();
        return _context.Employees.Include(e => e.Department)
            .CountAsync(e => e.Department != null && e.Department.Name.ToLower() == lower, cancellationToken);
    }

    public Task<int> CountByPositionAsync(string positionContains, CancellationToken cancellationToken = default)
    {
        var lower = positionContains.ToLower();
        return _context.Employees.CountAsync(e => e.Position.ToLower().Contains(lower), cancellationToken);
    }
}
