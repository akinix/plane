namespace YH.Modules.Identity.Authorization.ApiKey;

/// <summary>
/// Constants for the API Key authentication scheme.
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    /// <summary>
    /// The authentication scheme name used for API Key authentication.
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// The default HTTP header name that carries the API key value.
    /// </summary>
    public const string HeaderName = "X-Api-Key";
}
