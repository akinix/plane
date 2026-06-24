using System.Net;
using Mediator;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.CreateModule;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.CreateModule;

/// <summary>
/// Handles <see cref="CreateModuleCommand"/> — creates a new project module.
/// </summary>
public sealed class CreateModuleCommandHandler : ICommandHandler<CreateModuleCommand, CreateModuleResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateModuleResponse> Handle(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var module = Module.Create(
            name: command.Name,
            projectId: command.ProjectId,
            status: command.Status ?? "planned",
            startDate: command.StartDate,
            targetDate: command.TargetDate,
            description: command.Description,
            leadId: command.LeadId);

        _db.Modules.Add(module);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateModuleResponse(module.Id);
    }
}
