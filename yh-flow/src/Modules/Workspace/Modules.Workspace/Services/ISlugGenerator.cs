using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Services;

/// <summary>
/// Workspace slug lifecycle service (CONTEXT D-07 / D-08 / D-09).
/// </summary>
/// <remarks>
/// <para><b>Three responsibilities:</b></para>
/// <list type="number">
///   <item><see cref="Slugify"/> — pure text normalization to <c>[a-z0-9-]</c> form (D-09).</item>
///   <item><see cref="IsValidSlug"/> — format + restricted-words validation (D-09).</item>
///   <item><see cref="GenerateUniqueSlugAsync"/> — collision-checked generation with bounded retry
///     (D-07). Retries 5 times with a random suffix before failing.</item>
/// </list>
/// <para>
/// Pure members (<see cref="Slugify"/>, <see cref="IsValidSlug"/>) deliberately carry NO DbContext
/// dependency so they are directly unit-testable. The DB-touching
/// <see cref="GenerateUniqueSlugAsync"/> lives on the implementation and is covered by integration
/// tests with an InMemory <see cref="WorkspaceDbContext"/>.
/// </para>
/// </remarks>
public interface ISlugGenerator
{
    /// <summary>
    /// Normalizes arbitrary text to a slug candidate (lowercased, digits preserved, other chars → '-',
    /// consecutive '-' collapsed, leading/trailing '-' trimmed, capped at
    /// <see cref="YH.Modules.Workspace.WorkspaceModuleConstants.SlugMaxLength"/> chars).
    /// Does NOT verify uniqueness or restricted-words.
    /// </summary>
    string Slugify(string input);

    /// <summary>
    /// Validates a candidate slug: length 3..48, matches <c>^[a-z0-9]+(?:-[a-z0-9]+)*$</c>,
    /// and is not a reserved slug (<see cref="YH.Modules.Workspace.Contracts.Constants.RestrictedSlugs"/>).
    /// </summary>
    bool IsValidSlug(string slug);

    /// <summary>
    /// Generates a workspace-unique slug for <paramref name="name"/>. Returns the same value as
    /// <see cref="Slugify"/> on the first attempt; on collision appends a short random suffix
    /// (<c>{base}-{suffix}</c>) up to 4 more times (5 total). Throws <c>CustomException</c>
    /// (HTTP 409) if all 5 attempts collide.
    /// </summary>
    /// <param name="name">Display name. Must be non-null/non-whitespace (else ArgumentException).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A unique slug that does not collide with any existing (incl. soft-deleted) workspace row.</returns>
    Task<string> GenerateUniqueSlugAsync(string? name, CancellationToken ct);
}
