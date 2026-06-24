using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Module-Member M2M through entity (Plane <c>models/module.py</c> ModuleMember).
/// Soft-deletable to support removing members from modules without data loss.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <c>ModuleMember</c> is tenant-scoped via <see cref="IHasTenant"/>.
/// <para>
/// <b>Unique constraint:</b> A unique index on <c>(TenantId, ModuleId, MemberId)</c> with
/// <c>HasFilter("[DeletedOnUtc] IS NULL")</c> prevents duplicate member membership in a module.
/// </para>
/// </remarks>
public sealed class ModuleMember : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }

    /// <summary>Member user id (scalar, no cross-module FK).</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Module id (FK, cascade delete).</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>When this member was added to the module.</summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant — populated by Finbuckle on save.
    public string TenantId { get; private set; } = default!;

    // ISoftDeletable.
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ModuleMember() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="ModuleMember"/> association.
    /// </summary>
    public static ModuleMember Create(Guid memberId, Guid moduleId)
    {
        if (memberId == Guid.Empty)
            throw new ArgumentException("Member id is required.", nameof(memberId));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module id is required.", nameof(moduleId));

        return new ModuleMember
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            ModuleId = moduleId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Soft-deletes this module-member association (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
