using System.Threading;

namespace YH.Modules.Identity.Features.v1.OAuth;

public interface IOAuthProvider
{
    string ProviderName { get; }

    Uri GetAuthUrl(string state, string callbackUrl);

    Task<OAuthUserInfo> ExchangeCodeAsync(string code, string callbackUrl, CancellationToken ct);
}
