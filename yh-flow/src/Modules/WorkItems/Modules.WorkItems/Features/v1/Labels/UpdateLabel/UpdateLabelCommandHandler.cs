using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.UpdateLabel;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Labels.UpdateLabel;

/// <summary>
/// Handles <see cref="UpdateLabelCommand"/> — applies PATCH updates to mutable label fields.
/// Validates name uniqueness if name changed.
/// </summary>
public sealed class UpdateLabelCommandHandler : ICommandHandler<UpdateLabelCommand, LabelDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateLabelCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<LabelDto> Handle(UpdateLabelCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.LabelId == Guid.Empty)
        {
            throw new CustomException("Label id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var label = await _db.Labels
            .FirstOrDefaultAsync(l => l.Id == command.LabelId && l.ProjectId == command.ProjectId && !l.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            throw new NotFoundException($"Label '{command.LabelId}' was not found.");
        }

        // Check name uniqueness if name changed
        if (command.Name is not null && command.Name != label.Name)
        {
            var nameExists = await _db.Labels
                .AnyAsync(l => l.ProjectId == command.ProjectId && l.Name == command.Name && l.Id != command.LabelId && !l.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (nameExists)
            {
                throw new CustomException(
                    "A label with this name already exists in this project.",
                    Array.Empty<string>(),
                    HttpStatusCode.Conflict);
            }
        }

        label.Update(
            name: command.Name,
            color: command.Color,
            parentId: command.ParentId,
            description: command.Description,
            sortOrder: command.SortOrder);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return LabelDtoMapper.ToDto(label);
    }
}
