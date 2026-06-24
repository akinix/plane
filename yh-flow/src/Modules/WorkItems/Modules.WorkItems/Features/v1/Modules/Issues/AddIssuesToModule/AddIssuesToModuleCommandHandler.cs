using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.AddIssuesToModule;

/// <summary>
/// Handles <see cref="AddIssuesToModuleCommand"/> — adds issues to a module.
/// Module has no COMPLETED restriction (unlike Cycle) — all modules accept issues.
/// </summary>
public sealed class AddIssuesToModuleCommandHandler : ICommandHandler<AddIssuesToModuleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public AddIssuesToModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(AddIssuesToModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }
        if (command.ModuleId == Guid.Empty)
        {
            throw new CustomException("Module id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Verify module exists and is not deleted
        var module = await _db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId && m.ProjectId == command.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{command.ModuleId}' was not found.");
        }

        // Module has no COMPLETED restriction — add issues regardless of module status

        // Add each issue to the module
        foreach (var issueId in command.IssueIds)
        {
            if (issueId == Guid.Empty) continue;

            var moduleIssue = ModuleIssue.Create(issueId, command.ModuleId);
            _db.Set<ModuleIssue>().Add(moduleIssue);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
