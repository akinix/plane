using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.GetProject;
using YH.Modules.Project.Data;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects.GetProject;

/// <summary>
/// Handles <see cref="GetProjectQuery"/> — fetches a single project by id.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ProjectEntity"/> implements <c>IHasTenant</c>,
/// so the <c>ProjectDbContext</c> automatically applies the tenant filter. The handler only
/// needs to filter by project id and exclude soft-deleted rows.
/// </remarks>
public sealed class GetProjectQueryHandler : IQueryHandler<GetProjectQuery, ProjectDto>
{
    private readonly ProjectDbContext _db;

    public GetProjectQueryHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ProjectDto> Handle(GetProjectQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // AsNoTracking — pure read. Project is IHasTenant so tenant filter automatically applies.
        // Also explicitly exclude soft-deleted rows.
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (project is null)
        {
            throw new NotFoundException($"Project '{query.ProjectId}' was not found.");
        }

        return ProjectDtoMapper.ToDto(project);
    }
}
