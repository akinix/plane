using YH.Framework.Core.Context;

namespace YH.Modules.Identity.Contracts.Services;

/// <summary>
/// Service interface for accessing HTTP request context information.
/// Provides request metadata for auditing, logging, and other cross-cutting concerns.
/// </summary>
public interface IRequestContextService : IRequestContext
{
}