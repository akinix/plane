using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YH.Modules.Identity.Authorization.ApiKey;

/// <summary>
/// Extension methods for registering the API Key authentication scheme.
/// </summary>
internal static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Registers the API Key authentication scheme with the specified options binding.
    /// </summary>
    internal static IServiceCollection ConfigureApiKeyAuth(this IServiceCollection services)
    {
        services.AddOptions<ApiKeyAuthenticationOptions>()
            .BindConfiguration(nameof(ApiKeyAuthenticationOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication()
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.AuthenticationScheme, null!);

        return services;
    }
}
