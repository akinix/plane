using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.OAuth;
using Mediator;

namespace YH.Modules.Identity.Features.v1.OAuth.Manage;

public sealed class DeleteOAuthProviderCommandHandler : ICommandHandler<DeleteOAuthProviderCommand, bool>
{
    private readonly IOAuthProviderSettingsService _service;

    public DeleteOAuthProviderCommandHandler(IOAuthProviderSettingsService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    public async ValueTask<bool> Handle(DeleteOAuthProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(command.Id, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
