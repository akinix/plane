using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.GetIssue;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Issues.GetIssue;

/// <summary>
/// Handles <see cref="GetIssueQuery"/> — fetches a single issue with detail fields.
/// </summary>
public sealed class GetIssueQueryHandler : IQueryHandler<GetIssueQuery, IssueDetailDto>
{
    private readonly WorkItemsDbContext _db;

    public GetIssueQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IssueDetailDto> Handle(GetIssueQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.IssueId == Guid.Empty)
        {
            throw new CustomException("Issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var issue = await _db.Issues
            .AsNoTracking()
            .Include(i => i.Assignees)
            .Include(i => i.Labels)
            .FirstOrDefaultAsync(i => i.Id == query.IssueId && i.ProjectId == query.ProjectId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (issue is null)
        {
            throw new NotFoundException($"Issue '{query.IssueId}' was not found.");
        }

        // Resolve state name and group for detail display
        string? stateName = null;
        int? stateGroup = null;
        if (issue.StateId.HasValue)
        {
            var state = await _db.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == issue.StateId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (state is not null)
            {
                stateName = state.Name;
                stateGroup = (int)state.Group;
            }
        }

        return IssueDtoMapper.ToDetailDto(issue, stateName, stateGroup);
    }
}
