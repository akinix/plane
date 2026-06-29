using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Labels.CreateLabel;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Labels.CreateLabel;

/// <summary>
/// Handles <see cref="CreateLabelCommand"/> — creates a new project label.
/// Checks name uniqueness within project and validates ParentId.
/// </summary>
public sealed class CreateLabelCommandHandler : ICommandHandler<CreateLabelCommand, CreateLabelResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateLabelCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateLabelResponse> Handle(CreateLabelCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Check name uniqueness within project (tenant-scoped by DbContext)
        var nameExists = await _db.Labels
            .AnyAsync(l => l.ProjectId == command.ProjectId && l.Name == command.Name && !l.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            throw new CustomException(
                "A label with this name already exists in this project.",
                Array.Empty<string>(),
                HttpStatusCode.Conflict);
        }

        // Validate ParentId exists in same project if provided
        if (command.ParentId.HasValue && command.ParentId.Value != Guid.Empty)
        {
            var parentExists = await _db.Labels
                .AnyAsync(l => l.Id == command.ParentId.Value && l.ProjectId == command.ProjectId && !l.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (!parentExists)
            {
                throw new CustomException(
                    "Parent label not found in this project.",
                    Array.Empty<string>(),
                    HttpStatusCode.BadRequest);
            }
        }

        var label = Label.Create(
            name: command.Name,
            color: command.Color,
            projectId: command.ProjectId,
            parentId: command.ParentId,
            description: command.Description,
            sortOrder: command.SortOrder ?? 65535.0);

        _db.Labels.Add(label);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateLabelResponse(label.Id);
    }
}
