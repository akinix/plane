using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.v1.OAuth;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.OAuth.Manage;

public static class ManageOAuthProviderEndpoint
{
    internal static RouteHandlerBuilder MapGetOAuthProvidersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapGet("/oauth-providers", ListHandler)
            .WithName("GetOAuthProviders")
            .WithSummary("List all OAuth providers")
            .WithDescription("List all configured OAuth providers (both enabled and disabled).")
            .Produces<IReadOnlyList<OAuthProviderSettingsDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    internal static RouteHandlerBuilder MapCreateOAuthProviderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapPost("/oauth-providers", CreateHandler)
            .WithName("CreateOAuthProvider")
            .WithSummary("Create an OAuth provider")
            .WithDescription("Create a new OAuth provider configuration.")
            .Produces<OAuthProviderSettingsDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    internal static RouteHandlerBuilder MapUpdateOAuthProviderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapPut("/oauth-providers/{id:guid}", UpdateHandler)
            .WithName("UpdateOAuthProvider")
            .WithSummary("Update an OAuth provider")
            .WithDescription("Update an existing OAuth provider configuration.")
            .Produces<OAuthProviderSettingsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    internal static RouteHandlerBuilder MapToggleOAuthProviderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapPatch("/oauth-providers/{id:guid}/toggle", ToggleHandler)
            .WithName("ToggleOAuthProvider")
            .WithSummary("Toggle OAuth provider enabled/disabled")
            .WithDescription("Enable or disable an OAuth provider.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    internal static RouteHandlerBuilder MapDeleteOAuthProviderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapDelete("/oauth-providers/{id:guid}", DeleteHandler)
            .WithName("DeleteOAuthProvider")
            .WithSummary("Delete an OAuth provider")
            .WithDescription("Delete an OAuth provider configuration.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Ok<IReadOnlyList<OAuthProviderSettingsDto>>> ListHandler(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOAuthProvidersQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(result);
    }

    private static async Task<CreatedAtRoute<OAuthProviderSettingsDto>> CreateHandler(
        CreateOAuthProviderCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct).ConfigureAwait(false);
        return TypedResults.CreatedAtRoute(result, nameof(ListHandler), result);
    }

    private static async Task<Results<Ok<OAuthProviderSettingsDto>, NotFound>> UpdateHandler(
        Guid id,
        [FromBody] UpdateOAuthProviderPayload payload,
        IMediator mediator,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new UpdateOAuthProviderCommand(
                id,
                payload.ClientId,
                payload.ClientSecret,
                payload.CallbackUrl,
                payload.AutoCreateAccount,
                payload.Scope), ct).ConfigureAwait(false);
            return TypedResults.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<NoContent, NotFound>> ToggleHandler(
        Guid id,
        [FromBody] TogglePayload payload,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ToggleOAuthProviderCommand(id, payload.Enabled), ct).ConfigureAwait(false);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteHandler(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteOAuthProviderCommand(id), ct).ConfigureAwait(false);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}

/// <summary>
/// Payload for toggling an OAuth provider's enabled state.
/// </summary>
public sealed record TogglePayload(bool Enabled);

/// <summary>
/// Payload for updating an OAuth provider.
/// </summary>
public sealed record UpdateOAuthProviderPayload(
    string ClientId,
    string ClientSecret,
    string CallbackUrl,
    bool AutoCreateAccount,
    string? Scope);
