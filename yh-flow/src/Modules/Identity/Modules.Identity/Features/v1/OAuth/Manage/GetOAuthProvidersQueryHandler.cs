using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.OAuth;
using Mediator;

namespace YH.Modules.Identity.Features.v1.OAuth.Manage;

public sealed class GetOAuthProvidersQueryHandler : IQueryHandler<GetOAuthProvidersQuery, IReadOnlyList<OAuthProviderSettingsDto>>
{
    private readonly IOAuthProviderSettingsService _service;

    public GetOAuthProvidersQueryHandler(IOAuthProviderSettingsService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    public async ValueTask<IReadOnlyList<OAuthProviderSettingsDto>> Handle(GetOAuthProvidersQuery query, CancellationToken cancellationToken)
    {
        return await _service.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }
}
