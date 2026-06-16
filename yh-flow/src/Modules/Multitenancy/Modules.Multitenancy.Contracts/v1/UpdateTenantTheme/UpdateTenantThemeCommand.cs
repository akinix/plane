using YH.Modules.Multitenancy.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Multitenancy.Contracts.v1.UpdateTenantTheme;

public sealed record UpdateTenantThemeCommand(TenantThemeDto Theme) : ICommand;