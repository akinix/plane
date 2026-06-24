using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueLinks.ListIssueLinks;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.ListIssueLinks;

/// <summary>
/// Handles <see cref="ListIssueLinksQuery"/> — lists all links for an issue.
/// </summary>
public sealed class ListIssueLinksQueryHandler : IQueryHandler<ListIssueLinksQuery, List<IssueLinkDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListIssueLinksQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IssueLinkDto>> Handle(ListIssueLinksQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.IssueId == Guid.Empty)
        {
            return [];
        }

        var links = await _db.Set<IssueLink>()
            .AsNoTracking()
            .Where(l => l.IssueId == query.IssueId)
            .OrderBy(l => l.CreatedOnUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return IssueLinkDtoMapper.ToDtoList(links);
    }
}
