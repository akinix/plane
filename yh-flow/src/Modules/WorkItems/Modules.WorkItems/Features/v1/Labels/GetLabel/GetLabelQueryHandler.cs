using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.GetLabel;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Labels.GetLabel;

/// <summary>
/// Handles <see cref="GetLabelQuery"/> — fetches a single label by id.
/// </summary>
public sealed class GetLabelQueryHandler : IQueryHandler<GetLabelQuery, LabelDto>
{
    private readonly WorkItemsDbContext _db;

    public GetLabelQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<LabelDto> Handle(GetLabelQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.LabelId == Guid.Empty)
        {
            throw new CustomException("Label id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var label = await _db.Labels
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == query.LabelId && l.ProjectId == query.ProjectId && !l.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            throw new NotFoundException($"Label '{query.LabelId}' was not found.");
        }

        return LabelDtoMapper.ToDto(label);
    }
}
