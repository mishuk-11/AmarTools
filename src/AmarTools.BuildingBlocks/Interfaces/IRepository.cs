using AmarTools.BuildingBlocks.Domain;
using System.Linq.Expressions;

namespace AmarTools.BuildingBlocks.Interfaces;

/// <summary>
/// Generic repository abstraction used by every module.
/// Implementations live in <c>AmarTools.Infrastructure</c>; modules depend only
/// on this interface, keeping them free of EF Core / persistence concerns.
/// </summary>
/// <typeparam name="T">
/// A domain entity that inherits from <see cref="BaseEntity"/>.
/// </typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Returns the entity with the given primary key, or <c>null</c>.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all entities matching the optional predicate.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a single entity matching <paramref name="predicate"/>, or <c>null</c>
    /// if none is found.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Returns <c>true</c> if at least one entity satisfies the predicate.</summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Returns the count of entities matching the optional predicate.</summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Stages a new entity for insertion. Call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist.</summary>
    void Add(T entity);

    /// <summary>Stages a collection of new entities for insertion.</summary>
    void AddRange(IEnumerable<T> entities);

    /// <summary>Stages an existing entity for update.</summary>
    void Update(T entity);

    /// <summary>Stages an entity for deletion.</summary>
    void Remove(T entity);
}
