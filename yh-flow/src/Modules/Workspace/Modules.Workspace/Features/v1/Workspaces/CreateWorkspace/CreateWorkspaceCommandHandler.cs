using System.Net;
using Mediator;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Services;

// Namespace/type collision: the root namespace `YH.Modules.Workspace` and the entity
// `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier. Alias the entity so
// `Workspace.Create(...)` resolves to the entity factory rather than the namespace
// (pattern from 02-02 WorkspaceDbContext.cs).
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;

/// <summary>
/// Handles <see cref="CreateWorkspaceCommand"/> — creates the workspace aggregate, auto-enrols the
/// creator as the first Admin <see cref="WorkspaceMember"/> (D-06), and persists both in one
/// SaveChanges unit of work.
/// </summary>
/// <remarks>
/// <para><b>Slug resolution (D-07/D-09):</b> if <see cref="CreateWorkspaceCommand.Slug"/> is provided
/// and valid, it is used directly; otherwise <see cref="ISlugGenerator.GenerateUniqueSlugAsync"/>
/// derives a unique slug from the name. The generator's collision check ignores soft-deleted rows
/// (D-08 — they carry the <c>__{epoch}</c> suffix), so reuse-after-delete is honoured.</para>
/// <para><b>Auto Admin member (D-06):</b> the plan-blessed approach for 02-04 is to inline-create
/// the <see cref="WorkspaceMember"/> row inside this handler (instead of calling the not-yet-built
/// <c>WorkspaceMembershipService.AddOwnerAsync</c>, which lands in 02-05). The inline approach is
/// self-contained and lets this plan ship independently; 02-05 may refactor the call later.</para>
/// <para><b>Cache invalidation (T-2-cacheinvalid):</b> NOT needed on create — the slug is brand new,
/// so the slug→tenant cache miss repopulates on next resolution. (WorkspaceTenantStore caches for
/// 30 min; a brand-new slug has no cached entry.)</para>
/// </remarks>
public sealed class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    private static readonly string[] InvalidSlugErrors =
        { "Slug must be 3-48 chars, match [a-z0-9-], and not be a reserved word." };

    private readonly WorkspaceDbContext _db;
    private readonly ISlugGenerator _slugGenerator;

    public CreateWorkspaceCommandHandler(WorkspaceDbContext db, ISlugGenerator slugGenerator)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _slugGenerator = slugGenerator ?? throw new ArgumentNullException(nameof(slugGenerator));
    }

    public async ValueTask<CreateWorkspaceResponse> Handle(CreateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnerUserId == Guid.Empty)
        {
            // Defensive: endpoint must populate OwnerUserId from the authenticated principal.
            throw new ArgumentException("Owner user id is required.", nameof(command));
        }

        // Resolve slug — explicit & valid takes precedence; otherwise generate from name.
        string finalSlug;
        if (!string.IsNullOrWhiteSpace(command.Slug))
        {
            if (!_slugGenerator.IsValidSlug(command.Slug))
            {
                throw new CustomException(
                    "Invalid workspace slug.",
                    InvalidSlugErrors,
                    HttpStatusCode.BadRequest);
            }

            // Even a valid-format slug may already be taken — generate-with-base to get the
            // collision-checked path (returns the same value when free, or appends a suffix).
            finalSlug = await ResolveExplicitSlugAsync(command.Slug, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            finalSlug = await _slugGenerator
                .GenerateUniqueSlugAsync(command.Name, cancellationToken)
                .ConfigureAwait(false);
        }

        var workspace = WorkspaceEntity.Create(
            name: command.Name,
            slug: finalSlug,
            ownerUserId: command.OwnerUserId,
            logo: command.Logo,
            timeZone: command.TimeZone);

        _db.Workspaces.Add(workspace);

        // D-06 — auto-enrol the creator as the first Admin member in the same SaveChanges unit.
        var ownerMember = WorkspaceMember.Create(
            workspaceId: workspace.Id,
            userId: command.OwnerUserId.ToString(),
            role: (int)WorkspaceRole.Admin,
            isActive: true);
        _db.Members.Add(ownerMember);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateWorkspaceResponse(workspace.Id, workspace.Slug);
    }

    /// <summary>
    /// For an explicit client-supplied slug: validate uniqueness against existing rows. We reuse the
    /// generator's collision probe by calling <see cref="ISlugGenerator.GenerateUniqueSlugAsync"/>
    /// with the slug as the name — Slugify is idempotent on an already-valid slug, so on the
    /// collision-free path the candidate equals the input; on collision the generator appends a
    /// suffix, which we surface as a 409 (client asked for a slug that is taken).
    /// </summary>
    private async Task<string> ResolveExplicitSlugAsync(string requestedSlug, CancellationToken ct)
    {
        var generated = await _slugGenerator
            .GenerateUniqueSlugAsync(requestedSlug, ct)
            .ConfigureAwait(false);

        if (!string.Equals(generated, requestedSlug, StringComparison.Ordinal))
        {
            // The generator had to append a suffix → the requested slug is taken. Surface as 409 so
            // the client can pick a different explicit slug (matches Plane workspace-slug-check UX).
            throw new CustomException(
                $"Workspace slug '{requestedSlug}' is already taken.",
                Array.Empty<string>(),
                HttpStatusCode.Conflict);
        }

        return requestedSlug;
    }
}
