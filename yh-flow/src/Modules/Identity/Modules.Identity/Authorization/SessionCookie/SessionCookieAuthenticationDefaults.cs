namespace YH.Modules.Identity.Authorization.SessionCookie;

/// <summary>
/// Constants for the session cookie authentication scheme.
/// </summary>
public static class SessionCookieAuthenticationDefaults
{
    /// <summary>
    /// The authentication scheme name used for session cookie authentication.
    /// </summary>
    public const string AuthenticationScheme = "SessionCookie";

    /// <summary>
    /// The cookie name used to carry the authenticated session.
    /// </summary>
    public const string CookieName = ".YHFlow.Session";
}
