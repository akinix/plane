using System.Net;
using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.TransferCycleIssues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Services;

namespace YH.Modules.WorkItems.Features.v1.Cycles.TransferCycleIssues;

/// <summary>
/// Handles <see cref="TransferCycleIssuesCommand"/> — transfers uncompleted issues from one cycle to another.
/// Builds a progress snapshot on the source cycle before migration.
/// </summary>
public sealed class TransferCycleIssuesCommandHandler : ICommandHandler<TransferCycleIssuesCommand, Unit>
{
    private readonly WorkItemsDbContext _db;
    private readonly IBurndownCalculator _burndown;

    public TransferCycleIssuesCommandHandler(WorkItemsDbContext db, IBurndownCalculator burndown)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _burndown = burndown ?? throw new ArgumentNullException(nameof(burndown));
    }

    public async ValueTask<Unit> Handle(TransferCycleIssuesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
        if (command.CycleId == Guid.Empty)
        {
            throw new CustomException("Source cycle id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
        if (command.NewCycleId == Guid.Empty)
        {
            throw new CustomException("Target cycle id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // 1) Load source cycle
        var sourceCycle = await _db.Cycles
            .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (sourceCycle is null)
        {
            throw new NotFoundException($"Source cycle '{command.CycleId}' was not found.");
        }

        // 2) Load target cycle
        var targetCycle = await _db.Cycles
            .FirstOrDefaultAsync(c => c.Id == command.NewCycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (targetCycle is null)
        {
            throw new NotFoundException($"Target cycle '{command.NewCycleId}' was not found.");
        }

        // 3) Validate target cycle is not COMPLETED
        if (targetCycle.EndDate.HasValue && targetCycle.EndDate.Value < DateTimeOffset.UtcNow)
        {
            throw new CustomException("Cannot transfer issues to a completed cycle.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // 4) Build progress snapshot on source cycle
        var snapshot = await _burndown.BuildSnapshotAsync(command.CycleId, cancellationToken).ConfigureAwait(false);
        var snapshotJson = JsonSerializer.Serialize(snapshot);
        sourceCycle.FreezeSnapshot(snapshotJson);

        // 5) Find uncompleted CycleIssues (issues without CompletedAt)
        var uncompletedCycleIssues = await _db.Set<CycleIssue>()
            .Where(ci => ci.CycleId == command.CycleId && !ci.IsDeleted)
            .Join(_db.Issues.Where(i => !i.IsDeleted),
                ci => ci.IssueId,
                i => i.Id,
                (ci, i) => new { CycleIssue = ci, i.CompletedAt })
            .Where(x => x.CompletedAt == null)
            .Select(x => x.CycleIssue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // 6) Migrate: soft-delete old CycleIssues + create new ones
        foreach (var ci in uncompletedCycleIssues)
        {
            ci.SoftDelete(DateTimeOffset.UtcNow);

            var newCi = CycleIssue.Create(ci.IssueId, command.NewCycleId);
            _db.Set<CycleIssue>().Add(newCi);
        }

        // 7) Single SaveChanges transaction
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
