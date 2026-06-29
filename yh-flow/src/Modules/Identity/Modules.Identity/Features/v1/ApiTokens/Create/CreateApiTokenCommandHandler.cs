using YH.Framework.Core.Context;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;

namespace YH.Modules.Identity.Features.v1.ApiTokens.Create;

public sealed class CreateApiTokenCommandHandler : ICommandHandler<CreateApiTokenCommand, ApiTokenCreateResult>
{
    private readonly IApiTokenService _apiTokenService;
    private readonly ICurrentUser _currentUser;

    public CreateApiTokenCommandHandler(IApiTokenService apiTokenService, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(apiTokenService);
        ArgumentNullException.ThrowIfNull(currentUser);

        _apiTokenService = apiTokenService;
        _currentUser = currentUser;
    }

    public async ValueTask<ApiTokenCreateResult> Handle(CreateApiTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        var tenantId = _currentUser.GetTenant() ?? string.Empty;

        var (dto, rawKey) = await _apiTokenService.CreateAsync(
            command.Name,
            userId,
            tenantId,
            command.ExpiredAt,
            cancellationToken).ConfigureAwait(false);

        return new ApiTokenCreateResult(
            dto.Id,
            dto.Name,
            dto.Prefix,
            rawKey,
            dto.ExpiredAt,
            dto.CreatedAt);
    }
}
