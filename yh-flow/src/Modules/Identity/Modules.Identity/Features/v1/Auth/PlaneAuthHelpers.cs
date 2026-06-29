using YH.Framework.Shared.Constants;
using YH.Modules.Identity.Authorization.SessionCookie;
using YH.Modules.Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace YH.Modules.Identity.Features.v1.Auth;

internal static class PlaneAuthHelpers
{
    internal static PlaneUserProfile CreateUserProfile(TokenResponse token, string fallbackEmail)
    {
        ArgumentNullException.ThrowIfNull(token);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        return CreateUserProfile(jwt.Claims, fallbackEmail);
    }

    internal static PlaneUserProfile CreateUserProfile(UserDto user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new PlaneUserProfile(
            Id: user.Id ?? string.Empty,
            Email: user.Email ?? string.Empty,
            FirstName: string.IsNullOrWhiteSpace(user.FirstName) ? null : user.FirstName,
            LastName: string.IsNullOrWhiteSpace(user.LastName) ? null : user.LastName,
            Avatar: string.IsNullOrWhiteSpace(user.ImageUrl) ? null : user.ImageUrl,
            IsEmailVerified: user.EmailConfirmed);
    }

    internal static PlaneUserProfile CreateUserProfile(IEnumerable<Claim> claims, string fallbackEmail)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var claimsList = claims.ToList();
        var userId = claimsList.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
            ?? claimsList.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
        var email = claimsList.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
            ?? claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            ?? fallbackEmail;
        var firstName = claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? claimsList.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        var lastName = claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var avatar = claimsList.FirstOrDefault(c => c.Type == ClaimConstants.ImageUrl)?.Value
            ?? claimsList.FirstOrDefault(c => c.Type.EndsWith("imageurl", StringComparison.OrdinalIgnoreCase))?.Value;

        return new PlaneUserProfile(
            Id: userId,
            Email: email,
            FirstName: string.IsNullOrWhiteSpace(firstName) ? null : firstName,
            LastName: string.IsNullOrWhiteSpace(lastName) ? null : lastName,
            Avatar: string.IsNullOrWhiteSpace(avatar) ? null : avatar,
            IsEmailVerified: true);
    }

    internal static async Task SignInSessionCookieAsync(HttpContext httpContext, PlaneUserProfile user)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
        };

        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SessionCookieAuthenticationDefaults.AuthenticationScheme));
        await httpContext.SignInAsync(SessionCookieAuthenticationDefaults.AuthenticationScheme, principal).ConfigureAwait(false);
    }

    internal static string CreateRequestOrigin(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}{httpContext.Request.PathBase.Value}";
    }
}
