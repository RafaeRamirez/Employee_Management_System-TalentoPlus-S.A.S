using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Single commit point to coordinate multiple repository operations.
        return _context.SaveChangesAsync(cancellationToken);
    }
}
