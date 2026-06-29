using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;

namespace YH.Modules.Workspace.MultiTenancy;

/// <summary>
/// Finbuckle tenant store that resolves a slug to the corresponding workspace's tenant info
/// (CONTEXT D-01 / RESEARCH §Example 2 / threats T-2-tenantleak + T-2-cache).
/// </summary>
/// <remarks>
/// <b>Role in the resolution chain (D-01):</b> the slug strategy extracts the slug from the
/// route value; Finbuckle's resolver then calls
/// <see cref="GetByIdentifierAsync(string)"/> with that slug. This store:
/// <list type="number">
///   <item>probes a Redis-backed <see cref="IDistributedCache"/> for the cached resolution;</item>
///   <item>on miss, queries <see cref="WorkspaceDbContext.Workspaces"/> for an ACTIVE
///     (<c>!IsDeleted</c>) workspace with the matching slug;</item>
///   <item>on hit, materialises an <see cref="AppTenantInfo"/> whose <c>Id</c> is the workspace
///     <see cref="Guid"/> (so downstream EF Core contexts treat it as the Finbuckle TenantId) and
///     whose <c>Identifier</c> is the slug (for debugging / Finbuckle logging); writes the entry
///     back to cache;</item>
///   <item>on miss, returns null — Finbuckle falls back to the next store in the chain
///     (<c>EFCoreStore&lt;TenantDbContext, AppTenantInfo&gt;</c> registered by Phase 1
///     <c>MultitenancyModule.cs:111</c>). Per Pitfall 6 we never throw on miss.</item>
/// </list>
/// <para>
/// <b>IGlobalEntity query (Pitfall 6 / T-2-tenantleak):</b> <c>Workspace</c>
/// (<c>YH.Modules.Workspace.Domain.Workspace</c>) implements
/// <see cref="YH.Framework.Core.Domain.IGlobalEntity"/>, so the Workspaces DbSet is
/// NOT tenant-filtered — querying it does NOT form a resolve-tenant-by-querying-tenant-filtered-table
/// cycle. The query additionally asserts <c>!w.IsDeleted</c> so a soft-deleted workspace cannot
/// keep resolving to a tenant (threat T-2-tenantleak mitigation).
/// </para>
/// <para>
/// <b>Cache (T-2-cache disposition = accept):</b> the cached payload is non-sensitive tenant
/// metadata (workspaceGuid + slug + name). Key namespace <c>ws:slug:</c> isolates it from other
/// cache users. Workspace CRUD handlers in plan 02-04 are responsible for cache invalidation
/// on slug transfer / workspace delete / rename.
/// </para>
/// <para>
/// <b>Mutation methods are no-op:</b> this store is read-only against the Workspaces table.
/// <see cref="AddAsync(AppTenantInfo)"/> / <see cref="UpdateAsync(AppTenantInfo)"/> /
/// <see cref="RemoveAsync(string)"/> return true without persisting — the Workspaces table is the
/// source of truth and is mutated via the Workspace module's own handlers, not via Finbuckle's
/// tenant-management surface. <see cref="GetAllAsync()"/> / <see cref="GetAsync(string)"/> also
/// return empty / null — tenant listing is owned by <c>MultitenancyModule</c>'s
/// <c>EFCoreStore&lt;TenantDbContext&gt;</c>; this store only handles the slug resolution path.
/// </para>
/// </remarks>
public sealed class WorkspaceTenantStore : IMultiTenantStore<AppTenantInfo>
{
    /// <summary>Cache key prefix. <c>ws:slug:{identifier}</c> — namespaced to avoid collisions.</summary>
    private const string CacheKeyPrefix = "ws:slug:";

    /// <summary>Cache TTL — 30 minutes sliding. Workspace metadata rarely changes; CRUD handlers invalidate on rename/delete.</summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = CacheTtl,
    };

    /// <summary>
    /// JSON serialiser options — minimised, case-insensitive. AppTenantInfo is a flat POCO for our payload.
    /// </summary>
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web)
    {
        // Only Id/Identifier/Name are cached. The full AppTenantInfo (ConnectionString, AdminEmail,
        // QuotaLimits, ...) is intentionally excluded — it is sourced from TenantDbContext at
        // tenant-list / billing time, not from a workspace slug lookup.
    };

    private readonly WorkspaceDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<WorkspaceTenantStore> _logger;

    public WorkspaceTenantStore(
        WorkspaceDbContext db,
        IDistributedCache cache,
        ILogger<WorkspaceTenantStore> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolve a slug to the workspace's tenant info. The primary entry point for Finbuckle's
    /// resolution chain. Returns null on miss so Finbuckle falls back to the next store (Pitfall 6).
    /// </summary>
    /// <param name="identifier">Workspace slug (from the route value).</param>
    public async Task<AppTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;

        var cacheKey = CacheKeyPrefix + identifier;

        // 1) Cache lookup
        var cached = await TryGetFromCacheAsync(cacheKey).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        // 2) DB lookup against the IGlobalEntity Workspaces table (NOT tenant-filtered — Pitfall 6).
        //    Filter !w.IsDeleted so soft-deleted workspaces stop resolving to a tenant (T-2-tenantleak).
        var workspace = await _db.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Slug == identifier && !w.IsDeleted)
            .ConfigureAwait(false);

        if (workspace is null)
        {
            // Let the next Finbuckle store try (e.g. EFCoreStore<TenantDbContext>). Do NOT throw
            // — Pitfall 6 warns that throwing on miss masks the fallback chain.
            return null;
        }

        var tenantInfo = new AppTenantInfo(
            id: workspace.Id.ToString(),          // becomes the Finbuckle TenantId on downstream contexts
            identifier: workspace.Slug,            // for logging / debugging
            name: workspace.Name);

        // 3) Backfill cache (fire-and-forget-safe — we already have the value; cache is a perf optimisation only).
        await TryWriteCacheAsync(cacheKey, tenantInfo).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Workspace tenant store resolved slug '{Slug}' → workspace {WorkspaceId}",
                identifier, workspace.Id);
        }

        return tenantInfo;
    }

    // ─── Finbuckle-required members — read-only store semantics (see class remarks) ───

    /// <summary>
    /// Look up by Finbuckle tenant id. For slug-resolved workspaces the id is the workspace Guid
    /// (stringified). Caching here would require a second cache namespace; the hot path is
    /// <see cref="GetByIdentifierAsync(string)"/> (Finbuckle's slug strategy always calls by identifier),
    /// so this method falls through to the DB without caching — acceptable because Finbuckle only
    /// calls it for cross-store coordination, not for the per-request resolution hot path.
    /// </summary>
    public async Task<AppTenantInfo?> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var workspaceId))
            return null;

        var workspace = await _db.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workspaceId && !w.IsDeleted)
            .ConfigureAwait(false);

        return workspace is null
            ? null
            : new AppTenantInfo(workspace.Id.ToString(), workspace.Slug, workspace.Name);
    }

    /// <summary>
    /// Tenant enumeration is owned by <c>MultitenancyModule</c>'s
    /// <c>EFCoreStore&lt;TenantDbContext, AppTenantInfo&gt;</c> — this store does not surface
    /// a global workspace list via the Finbuckle tenant-management surface.
    /// </summary>
    public Task<IEnumerable<AppTenantInfo>> GetAllAsync()
        => Task.FromResult<IEnumerable<AppTenantInfo>>(Array.Empty<AppTenantInfo>());

    /// <inheritdoc cref="GetAllAsync()"/>
    public Task<IEnumerable<AppTenantInfo>> GetAllAsync(int take, int skip)
        => Task.FromResult<IEnumerable<AppTenantInfo>>(Array.Empty<AppTenantInfo>());

    /// <summary>
    /// No-op — the Workspaces table is the source of truth and is mutated by Workspace module
    /// handlers (create / update / delete), not by Finbuckle's tenant-management API.
    /// </summary>
    public Task<bool> AddAsync(AppTenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Invalidates the cache for <paramref name="identifier"/>. Does NOT delete the workspace row
    /// — Workspace module handlers own deletion (and call <c>Workspace.SoftDelete</c> to release
    /// the slug per D-08). Returning true lets Finbuckle's coordinator continue.
    /// </summary>
    public async Task<bool> RemoveAsync(string identifier)
    {
        if (!string.IsNullOrEmpty(identifier))
        {
            await _cache.RemoveAsync(CacheKeyPrefix + identifier).ConfigureAwait(false);
        }
        return true;
    }

    /// <summary>
    /// Invalidates the cache for <paramref name="tenantInfo"/>.Identifier so the next resolution
    /// re-queries the Workspaces table (after a rename / slug transfer).
    /// </summary>
    public async Task<bool> UpdateAsync(AppTenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        if (!string.IsNullOrEmpty(tenantInfo.Identifier))
        {
            await _cache.RemoveAsync(CacheKeyPrefix + tenantInfo.Identifier).ConfigureAwait(false);
        }
        return true;
    }

    // ─── Cache helpers ───

    private async Task<AppTenantInfo?> TryGetFromCacheAsync(string cacheKey)
    {
        try
        {
            var bytes = await _cache.GetAsync(cacheKey).ConfigureAwait(false);
            if (bytes is null || bytes.Length == 0) return null;

            var payload = JsonSerializer.Deserialize<CachedPayload>(bytes, PayloadJsonOptions);
            if (payload is null || string.IsNullOrEmpty(payload.Id) || string.IsNullOrEmpty(payload.Identifier))
                return null;

            return new AppTenantInfo(payload.Id, payload.Identifier, payload.Name);
        }
        catch (Exception ex) when (ex is JsonException or FormatException)
        {
            // Corrupt cache entry — treat as miss and let the DB lookup re-populate.
            _logger.LogWarning(ex, "Failed to deserialize cached workspace tenant payload at {Key}; ignoring", cacheKey);
            return null;
        }
    }

    private async Task TryWriteCacheAsync(string cacheKey, AppTenantInfo tenantInfo)
    {
        try
        {
            var payload = new CachedPayload(tenantInfo.Id, tenantInfo.Identifier, tenantInfo.Name);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, PayloadJsonOptions);
            await _cache.SetAsync(cacheKey, bytes, CacheOptions).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Cache write failure must not break resolution — we already have the value in hand.
            _logger.LogWarning(ex, "Failed to write workspace tenant payload to cache at {Key}; ignoring", cacheKey);
        }
    }

    /// <summary>Serialisable projection of AppTenantInfo — only the 3 fields needed for resolution.</summary>
    private sealed record CachedPayload(string Id, string Identifier, string? Name);
}
