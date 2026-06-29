using YH.Framework.Core.Context;
using System.Security.Claims;

namespace YH.Modules.Identity.Contracts.Services;

/// <summary>
/// Service interface for managing the current user context.
/// Combines user identity access with initialization capabilities.
/// </summary>
public interface ICurrentUserService : ICurrentUser, ICurrentUserInitializer
{
}