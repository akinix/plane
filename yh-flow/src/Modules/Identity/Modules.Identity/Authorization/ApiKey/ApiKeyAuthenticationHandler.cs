using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YH.Framework.Shared.Constants;
using YH.Modules.Identity.Contracts.Services;

namespace YH.Modules.Identity.Authorization.ApiKey;

/// <summary>
/// Custom authentication handler that validates API keys from the X-Api-Key header.
/// Uses DI to obtain <see cref="IApiTokenService"/> from the request pipeline — never
/// accesses the database directly (trust boundary).
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Read the API key header
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var rawKey = apiKeyHeader.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            return AuthenticateResult.NoResult();
        }

        // 2. Resolve the validation service via DI (trust boundary: handler never touches DB)
        var apiTokenService = Context.RequestServices.GetRequiredService<IApiTokenService>();

        // 3. Validate the key
        var validationResult = await apiTokenService.ValidateAndGetOwnerAsync(rawKey, Context.RequestAborted);
        if (validationResult is null)
        {
            return AuthenticateResult.Fail("Invalid or expired API key");
        }

        // 4. Build claims principal — same claim structure as JWT for downstream compatibility
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, validationResult.UserId),
            new(ClaimTypes.Email, validationResult.Email),
            new(ClaimConstants.Tenant, validationResult.TenantId),
        };

        foreach (var permission in validationResult.Permissions)
        {
            claims.Add(new Claim(ClaimConstants.Permission, permission));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
