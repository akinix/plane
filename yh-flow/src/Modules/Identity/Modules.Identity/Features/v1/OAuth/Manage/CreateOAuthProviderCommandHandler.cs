using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.OAuth;
using Mediator;

namespace YH.Modules.Identity.Features.v1.OAuth.Manage;

public sealed class CreateOAuthProviderCommandHandler : ICommandHandler<CreateOAuthProviderCommand, OAuthProviderSettingsDto>
{
    private readonly IOAuthProviderSettingsService _service;

    public CreateOAuthProviderCommandHandler(IOAuthProviderSettingsService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    public async ValueTask<OAuthProviderSettingsDto> Handle(CreateOAuthProviderCommand command, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(
            command.ProviderName,
            command.ClientId,
            command.ClientSecret,
            command.CallbackUrl,
            command.AutoCreateAccount,
            command.Scope,
            cancellationToken).ConfigureAwait(false);
    }
}
