using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace YH.Modules.Workspace.MultiTenancy;

/// <summary>
/// Finbuckle slug resolution strategy (CONTEXT D-01 / RESEARCH §Example 1 / threat T-2-slugstrategy).
/// </summary>
/// <remarks>
/// <b>API note (02-01 spike Q1 authoritative finding):</b> the Finbuckle 10.1.x strategy contract
/// is the <b>non-generic</b> <see cref="IMultiTenantStrategy"/>. There is no
/// <c>IMultiTenantStrategy&lt;T&gt;</c>. PATTERNS.md / RESEARCH.md samples that show the generic
/// form WILL NOT compile — they were written against an older Finbuckle surface. The 02-01 spike
/// (<c>FinbuckleExternalStrategyRegistrationTests.cs</c>) is the ground truth.
/// <para>
/// <b>Behaviour (D-01 / Pitfall 2):</b> pull the slug from the route values via
/// <c>httpContext.GetRouteValue("slug")</c> at the key <c>"slug"</c>. If the value is a
/// non-empty string, return it as the tenant identifier — Finbuckle's resolver then looks the
/// identifier up via <c>WorkspaceTenantStore</c> (slug → AppTenantInfo) and stamps the resolved
/// tenant on the request. On top-level endpoints without a <c>{slug}</c> route segment
/// (e.g. <c>POST /api/v1/workspaces/</c>, <c>/api/v1/users/me/</c>) this strategy returns null
/// and the Phase 1 claim/header strategy chain takes over (Pitfall 2 — let the next strategy
/// decide; never throw).
/// </para>
/// <para>
/// <b>Priority / ordering:</b> registered via external <c>TryAddEnumerable</c> in
/// <c>WorkspaceModule.ConfigureServices</c> (per spike Q1 outcome — preferred path A).
/// Finbuckle evaluates strategies in registration order, returning the first non-null
/// identifier. The slug strategy MUST be ahead of the Phase 1 claim/header chain so that
/// workspace-scoped requests resolve to the workspace tenant even when the user's claim still
/// points at the platform root tenant.
/// </para>
/// <para>
/// <b>Auth timing (RESEARCH §Pattern 1):</b> <c>UseMultiTenant()</c> runs BEFORE
/// <c>UseAuthentication()</c>, so <see cref="HttpContext.User"/> is anonymous here — never read
/// claims. The strategy is purely route-driven.
/// </para>
/// </remarks>
public sealed class WorkspaceSlugStrategy : IMultiTenantStrategy
{
    /// <summary>
    /// Route value key the workspace-scoped route group binds the slug to
    /// (<c>MapGroup("api/v{version:apiVersion}/workspaces/{slug}")</c>).
    /// </summary>
    public const string SlugRouteKey = "slug";

    private readonly ILogger<WorkspaceSlugStrategy> _logger;

    public WorkspaceSlugStrategy(ILogger<WorkspaceSlugStrategy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lower priority value = evaluated earlier. The slug strategy must run ahead of the Phase 1
    /// claim/header strategies so a <c>{slug}</c> request resolves to the workspace tenant, not
    /// to whatever the JWT claim points at.
    /// </summary>
    public int Priority => -100;

    /// <summary>
    /// Resolves the tenant identifier (slug) from the current HTTP request's route values.
    /// Returns null on top-level endpoints — Finbuckle's resolver then falls back to the next
    /// strategy in the chain (Phase 1 claim/header) per Pitfall 2.
    /// </summary>
    /// <param name="context">The resolution context — must be the current <see cref="HttpContext"/>.</param>
    /// <returns>
    /// The slug, or null when the request is not workspace-scoped
    /// (no <c>{slug}</c> route value, empty value, non-string binding, or the context is not
    /// an <see cref="HttpContext"/>).
    /// </returns>
    public Task<string?> GetIdentifierAsync(object context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is not HttpContext httpContext)
        {
            // Background scope / non-HTTP resolution — slug strategy does not apply.
            return Task.FromResult<string?>(null);
        }

        // Pitfall 2: strict non-empty check. GetRouteValue returns null when the route has no
        // {slug} segment (top-level endpoints); returns object otherwise. We only treat a
        // string-bound, non-empty value as a tenant identifier.
        if (httpContext.GetRouteValue(SlugRouteKey) is not string slug || string.IsNullOrEmpty(slug))
        {
            return Task.FromResult<string?>(null);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Workspace slug strategy resolved identifier '{Slug}' from route", slug);
        }

        return Task.FromResult<string?>(slug);
    }
}
