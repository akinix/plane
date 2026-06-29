using System.Text.Json.Serialization;

namespace YH.Modules.Identity.Features.v1.Auth;

internal sealed record PlaneAuthError(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_code")] string ErrorCode);
