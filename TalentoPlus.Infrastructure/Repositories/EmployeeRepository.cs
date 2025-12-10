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

    public async Task<List<Employee>> GetAllAsync(string? ownerUserId = null, CancellationToken cancellationToken = default)
    {
        // Eager load Department to avoid N+1 in service/UI.
        var query = _context.Employees.Include(e => e.Department).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }

        return await query.ToListAsync(cancellationToken);
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

    public Task<int> CountAsync(string? ownerUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }
        return query.CountAsync(cancellationToken);
    }

    public Task<int> CountByStatusAsync(EmployeeStatus status, string? ownerUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.Where(e => e.Status == status);
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }
        return query.CountAsync(cancellationToken);
    }

    public Task<int> CountByDepartmentNameAsync(string departmentName, string? ownerUserId = null, CancellationToken cancellationToken = default)
    {
        var lower = departmentName.ToLower();
        var query = _context.Employees.Include(e => e.Department)
            .Where(e => e.Department != null && e.Department.Name.ToLower() == lower);
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }
        return query.CountAsync(cancellationToken);
    }

    public Task<int> CountByPositionAsync(string positionContains, string? ownerUserId = null, CancellationToken cancellationToken = default)
    {
        var lower = positionContains.ToLower();
        var query = _context.Employees.Where(e => e.Position.ToLower().Contains(lower));
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }
        return query.CountAsync(cancellationToken);
    }
}
