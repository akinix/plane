using Microsoft.AspNetCore.Authentication;

namespace YH.Modules.Identity.Authorization.ApiKey;

/// <summary>
/// Options for the API Key authentication scheme.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The HTTP header name that carries the API key value.
    /// Defaults to "X-Api-Key".
    /// </summary>
    public string HeaderName { get; init; } = ApiKeyAuthenticationDefaults.HeaderName;
}
