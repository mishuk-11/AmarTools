using AmarTools.BuildingBlocks.Domain;
using AmarTools.BuildingBlocks.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AmarTools.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core implementation of <see cref="IRepository{T}"/>.
/// Registered as a scoped service so it shares the request-scoped
/// <see cref="AppDbContext"/> and participates in the same transaction.
/// </summary>
/// <typeparam name="T">A domain entity inheriting from <see cref="BaseEntity"/>.</typeparam>
internal sealed class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set     = context.Set<T>();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _set.AsNoTracking();
        if (predicate is not null) query = query.Where(predicate);
        return await query.ToListAsync(ct);
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _set;
        if (predicate is not null) query = query.Where(predicate);
        return await query.CountAsync(ct);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public void Add(T entity)              => _set.Add(entity);
    public void AddRange(IEnumerable<T> entities) => _set.AddRange(entities);
    public void Update(T entity)           => _set.Update(entity);
    public void Remove(T entity)           => _set.Remove(entity);
}
