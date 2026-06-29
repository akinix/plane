using YH.Framework.Core.Context;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;

namespace YH.Modules.Identity.Features.v1.ApiTokens.Revoke;

public sealed class RevokeApiTokenCommandHandler : ICommandHandler<RevokeApiTokenCommand, bool>
{
    private readonly IApiTokenService _apiTokenService;
    private readonly ICurrentUser _currentUser;

    public RevokeApiTokenCommandHandler(IApiTokenService apiTokenService, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(apiTokenService);
        ArgumentNullException.ThrowIfNull(currentUser);

        _apiTokenService = apiTokenService;
        _currentUser = currentUser;
    }

    public async ValueTask<bool> Handle(RevokeApiTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();

        try
        {
            await _apiTokenService.RevokeAsync(command.TokenId, userId, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
