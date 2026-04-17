namespace AmarTools.BuildingBlocks.Interfaces;

/// <summary>
/// Abstracts the EF Core <c>SaveChangesAsync</c> call behind an interface
/// so module services can flush multiple repository changes in one transaction
/// without depending on <c>DbContext</c> directly.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all staged changes to the database in a single transaction.
    /// </summary>
    /// <param name="ct">Propagates cancellation from the HTTP request pipeline.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
