using System;
using System.Collections.Frozen;

namespace YH.Modules.Workspace.Contracts.Constants;

/// <summary>
/// Workspace slug deny-list (CONTEXT D-09 / threat T-02-01).
/// Literal port of Plane <c>apps/api/plane/utils/constants.py:RESTRICTED_WORKSPACE_SLUGS</c>.
/// These slugs are reserved for routes/system words (api, admin, settings, auth, ...) and must be
/// rejected by the SlugGenerator at create time. The case-insensitive <see cref="FrozenSet{T}"/> with
/// <see cref="StringComparer.OrdinalIgnoreCase"/> ensures mixed-case attempts ("API", "Admin") are
/// also rejected (D-09: format + deny-list). <see cref="FrozenSet{T}"/> is used (not <c>HashSet</c>)
/// to satisfy the Sonar analyzers (S2386/S3887) that flag publicly-exposed mutable collections.
/// The 02-04 SlugGenerator tests assert coverage of critical entries
/// (api/admin/settings/auth/web/billing/sign-in/sign-up/...).
/// </summary>
public static class RestrictedSlugs
{
    /// <summary>
    /// Reserved slugs ported from Plane <c>RESTRICTED_WORKSPACE_SLUGS</c> (62 unique entries after
    /// deduplication; Plane source lists 65 with three duplicates — monitor/config/mobile).
    /// </summary>
    public static FrozenSet<string> List { get; } = new HashSet<string>(62, StringComparer.OrdinalIgnoreCase)
    {
        "404",
        "accounts",
        "api",
        "create-workspace",
        "god-mode",
        "installations",
        "invitations",
        "onboarding",
        "profile",
        "spaces",
        "workspace-invitations",
        "password",
        "flags",
        "monitor",
        "monitoring",
        "ingest",
        "plane-pro",
        "plane-ultimate",
        "enterprise",
        "plane-enterprise",
        "disco",
        "silo",
        "chat",
        "calendar",
        "drive",
        "channels",
        "upgrade",
        "billing",
        "sign-in",
        "sign-up",
        "signin",
        "signup",
        "config",
        "live",
        "admin",
        "m",
        "import",
        "importers",
        "integrations",
        "integration",
        "configuration",
        "initiatives",
        "initiative",
        "workflow",
        "workflows",
        "epics",
        "epic",
        "story",
        "mobile",
        "dashboard",
        "desktop",
        "onload",
        "real-time",
        "one",
        "pages",
        "business",
        "pro",
        "settings",
        "license",
        "licenses",
        "instances",
        "instance",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Check whether a candidate slug is reserved. Case-insensitive.
    /// </summary>
    /// <param name="slug">Candidate slug.</param>
    /// <returns>True if the slug matches a reserved entry; otherwise false.</returns>
    public static bool IsRestricted(string slug) =>
        !string.IsNullOrEmpty(slug) && List.Contains(slug);
}
