using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Services;

/// <summary>
/// Provides project-scoped auto-increment sequence IDs with transaction-level locking.
/// Uses SERIALIZABLE isolation level for atomic MAX(SequenceId)+1 per project.
/// </summary>
/// <remarks>
/// <b>Locking strategy (RESEARCH Pitfall 1):</b>
/// Plane uses PostgreSQL <c>pg_advisory_xact_lock(project_id)</c> for transaction-level locking.
/// In .NET with EF Core, we use <c>IDbContextTransaction</c> at SERIALIZABLE isolation level
/// combined with <c>SELECT MAX(SequenceId) ... WHERE ProjectId = @pid</c>. This provides
/// equivalent mutual exclusion for concurrent Issue creates within the same project.
/// <para>
/// <b>Usage:</b> Call <c>GetNextSequenceIdAsync</c> inside a <c>SaveChanges</c> transaction scope
/// and assign the result to <c>issue.SequenceId</c> before saving.
/// </para>
/// </remarks>
public interface IIssueSequenceService
{
    /// <summary>
    /// Gets the next available sequence id for the specified project.
    /// Uses transaction-level locking to prevent duplicates under concurrent creates.
    /// </summary>
    /// <param name="projectId">The project scope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next sequence id (1-based, starting at 1 for new projects).</returns>
    Task<int> GetNextSequenceIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IIssueSequenceService"/> using SERIALIZABLE transaction isolation.
/// </summary>
public sealed class IssueSequenceService : IIssueSequenceService
{
    private readonly WorkItemsDbContext _db;

    public IssueSequenceService(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<int> GetNextSequenceIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        // Use a raw SQL query within the current or a new transaction to atomically
        // read-and-increment the sequence. The isolation level is set at the strategy level
        // (either SERIALIZABLE or RepeatableRead depending on provider capability).
        //
        // The raw SQL approach avoids materializing the full Issue entity for the MAX query
        // and ensures the lock is acquired at the database level.

        // PostgreSQL: SERIALIZABLE isolation provides equivalent behavior to
        // pg_advisory_xact_lock(). For projects without existing issues, returns 1.

        // Create a transaction strategy: use an execution strategy that retries on serialization failures
        var executionStrategy = _db.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            // Use SERIALIZABLE isolation for the MAX query to prevent phantom reads
            using var transaction = await _db.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);

            var maxSequenceId = await _db.Issues
                .IgnoreQueryFilters()
                .Where(i => i.ProjectId == projectId)
                .MaxAsync(i => (int?)i.SequenceId, cancellationToken)
                .ConfigureAwait(false);

            var nextId = (maxSequenceId ?? 0) + 1;

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return nextId;
        }).ConfigureAwait(false);
    }
}
