using YH.Framework.Core.Context;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;

namespace YH.Modules.Identity.Features.v1.ApiTokens.List;

public sealed class ListApiTokensQueryHandler : IQueryHandler<ListApiTokensQuery, IReadOnlyList<APITokenDto>>
{
    private readonly IApiTokenService _apiTokenService;
    private readonly ICurrentUser _currentUser;

    public ListApiTokensQueryHandler(IApiTokenService apiTokenService, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(apiTokenService);
        ArgumentNullException.ThrowIfNull(currentUser);

        _apiTokenService = apiTokenService;
        _currentUser = currentUser;
    }

    public async ValueTask<IReadOnlyList<APITokenDto>> Handle(ListApiTokensQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return await _apiTokenService.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
