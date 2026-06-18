using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.Workspace.Contracts.Constants;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Services;

/// <summary>
/// Default <see cref="ISlugGenerator"/> implementation (CONTEXT D-07 / D-08 / D-09, threat T-2-slug / T-2-slugrestricted).
/// </summary>
/// <remarks>
/// <para><b>Algorithm (RESEARCH §Example 4 + PATTERNS §SlugGenerator):</b></para>
/// <list type="number">
///   <item><see cref="Slugify"/> — lowercase + digit-preserving + non-<c>[a-z0-9]</c> → '-' +
///     collapse consecutive '-' + trim ends + cap at <see cref="WorkspaceModuleConstants.SlugMaxLength"/>.</item>
///   <item><see cref="IsValidSlug"/> — length 3..48 + strict <c>^[a-z0-9]+(?:-[a-z0-9]+)*$</c> +
///     reject reserved words via <see cref="RestrictedSlugs"/> (case-insensitive).</item>
///   <item><see cref="GenerateUniqueSlugAsync"/> — base = <see cref="Slugify"/>(name); up to 5 attempts.
///     Attempts 1..4 append a 4-char random hex suffix derived from
///     <see cref="RandomNumberGenerator"/> (BCL-only crypto-strength randomness, matches the
///     ApiTokenService pattern). Each candidate is checked against
///     <c>db.Workspaces.AnyAsync(w => w.Slug == candidate)</c> <b>WITHOUT</b> an <c>IsDeleted</c>
///     filter: soft-deleted workspaces already had their slug suffixed with <c>__{epoch}</c> by
///     <c>Workspace.SoftDelete</c> (D-08), so they can never collide with the original base; adding
///     <c>!IsDeleted</c> would be redundant and could miss the edge case where a row's slug has not
///     been suffixed yet. Throws <see cref="CustomException"/> (HTTP 409 Conflict) after 5 failures.</item>
/// </list>
/// <para>
/// <b>Regex source-gen:</b> the validation regex is compiled once (<c>RegexOptions.Compiled</c>) and
/// cached as a static readonly — <c>[RegexGenerator]</c> would require an extra partial class; the
/// compiled form is good enough for the request-volume NFR-1 target. The regex is the strict Plane
/// slug format: lowercase alphanumeric, single dashes between groups, no leading/trailing/double dash.
/// </para>
/// </remarks>
public sealed partial class SlugGenerator : ISlugGenerator
{
    /// <summary>Strict Plane slug format: <c>[a-z0-9]+(-[a-z0-9]+)*</c>.</summary>
    private static readonly Regex SlugFormatRegex = CreateSlugFormatRegex();

    private const int MaxAttempts = 5;
    private const int SuffixLength = 4;

    private readonly WorkspaceDbContext _db;

    public SlugGenerator(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Lowercase + replace any non-[a-z0-9] run with a single '-'. The pattern is greedy on
        // non-alphanumeric (after lowercasing) so "ACME Corp!" → "acme-corp".
        // CA1308 (ToLowerToUpper) is intentionally suppressed: D-09 mandates lowercase slugs.
#pragma warning disable CA1308
        var lowered = input.ToLowerInvariant();
#pragma warning restore CA1308
        var slug = NonAlphanumericRuns().Replace(lowered, "-");
        // Collapse consecutive dashes (already collapsed by greedy regex above, but be defensive in
        // case the regex is replaced) and trim ends.
        slug = MultipleDashes().Replace(slug, "-").Trim('-');

        // Cap at max length. Truncate at a dash boundary if possible so we never leave a trailing '-'.
        if (slug.Length > WorkspaceModuleConstants.SlugMaxLength)
        {
            slug = slug[..WorkspaceModuleConstants.SlugMaxLength];
            var lastDash = slug.LastIndexOf('-');
            if (lastDash > 0)
            {
                slug = slug[..lastDash];
            }
        }

        return slug;
    }

    /// <inheritdoc />
    public bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        if (slug.Length is < 3 or > WorkspaceModuleConstants.SlugMaxLength)
        {
            return false;
        }

        if (!SlugFormatRegex.IsMatch(slug))
        {
            return false;
        }

        // Restricted-words check is case-insensitive (RestrictedSlugs uses OrdinalIgnoreCase).
        return !RestrictedSlugs.IsRestricted(slug);
    }

    /// <inheritdoc />
    public async Task<string> GenerateUniqueSlugAsync(string? name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workspace name is required to generate a slug.", nameof(name));
        }

        var baseSlug = Slugify(name);
        if (string.IsNullOrEmpty(baseSlug))
        {
            // Slugify produced nothing usable (input had no ASCII alphanumerics). Fall back to a
            // random suffix-only candidate so non-Latin names still produce a valid slug.
            baseSlug = $"ws-{GenerateShortSuffix()}";
        }

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            // Attempt 0: bare base. Attempts 1..4: append a short random suffix.
            var candidate = attempt == 0 ? baseSlug : $"{baseSlug}-{GenerateShortSuffix()}";

            // AsNoTracking because this is a pure existence probe; the Workspace aggregate is
            // IGlobalEntity so no tenant filter applies — we intentionally DO NOT filter on IsDeleted
            // (see class remarks; soft-deleted rows already carry the __{epoch} suffix).
            var exists = await _db.Workspaces
                .AsNoTracking()
                .AnyAsync(w => w.Slug == candidate, ct)
                .ConfigureAwait(false);

            if (!exists)
            {
                return candidate;
            }
        }

        throw new CustomException(
            "Unable to generate a unique workspace slug after 5 attempts.",
            new[] { $"Base slug '{baseSlug}' collided on every attempt." },
            HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Produces a 4-char lowercase hex suffix from 4 random bytes. Uses
    /// <see cref="RandomNumberGenerator"/> (crypto-strength) so concurrent create requests cannot
    /// synchronize on a shared PRNG seed.
    /// </summary>
    private static string GenerateShortSuffix()
    {
        Span<byte> buffer = stackalloc byte[SuffixLength];
        RandomNumberGenerator.Fill(buffer);
        // Convert each byte to a 2-char hex; take the first SuffixLength chars.
        Span<char> hex = stackalloc char[SuffixLength * 2];
        for (var i = 0; i < SuffixLength; i++)
        {
            var b = buffer[i];
            hex[i * 2] = HexChar(b >> 4);
            hex[i * 2 + 1] = HexChar(b & 0xF);
        }
        return new string(hex[..SuffixLength]);
    }

    private static char HexChar(int nibble) =>
        (char)(nibble < 10 ? '0' + nibble : 'a' + (nibble - 10));

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.None)]
    private static partial Regex NonAlphanumericRuns();

    [GeneratedRegex("-{2,}", RegexOptions.None)]
    private static partial Regex MultipleDashes();

    private static Regex CreateSlugFormatRegex() =>
        new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
}
