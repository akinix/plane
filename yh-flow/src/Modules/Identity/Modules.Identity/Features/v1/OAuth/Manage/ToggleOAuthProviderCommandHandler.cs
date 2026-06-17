using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.OAuth;
using Mediator;

namespace YH.Modules.Identity.Features.v1.OAuth.Manage;

public sealed class ToggleOAuthProviderCommandHandler : ICommandHandler<ToggleOAuthProviderCommand, bool>
{
    private readonly IOAuthProviderSettingsService _service;

    public ToggleOAuthProviderCommandHandler(IOAuthProviderSettingsService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    public async ValueTask<bool> Handle(ToggleOAuthProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _service.ToggleEnabledAsync(command.Id, command.Enabled, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
