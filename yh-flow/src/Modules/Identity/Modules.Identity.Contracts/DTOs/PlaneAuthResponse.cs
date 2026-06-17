using System.Text.Json.Serialization;

namespace YH.Modules.Identity.Contracts.DTOs;

public sealed record PlaneAuthResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("access_token_expires_at")] DateTime AccessTokenExpiresAt,
    [property: JsonPropertyName("user")] PlaneUserProfile User);

public sealed record PlaneUserProfile(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("is_email_verified")] bool IsEmailVerified);
