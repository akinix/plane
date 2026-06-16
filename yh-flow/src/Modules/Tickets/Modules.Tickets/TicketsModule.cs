using Asp.Versioning;
using YH.Framework.Persistence;
using YH.Framework.Shared.Constants;
using YH.Framework.Web.Modules;
using YH.Modules.Tickets.Contracts.Authorization;
using YH.Modules.Tickets.Data;
using YH.Modules.Tickets.Features.v1.Tickets.AddTicketComment;
using YH.Modules.Tickets.Features.v1.Tickets.AssignTicket;
using YH.Modules.Tickets.Features.v1.Tickets.CloseTicket;
using YH.Modules.Tickets.Features.v1.Tickets.CreateTicket;
using YH.Modules.Tickets.Features.v1.Tickets.DeleteTicket;
using YH.Modules.Tickets.Features.v1.Tickets.GetTicketById;
using YH.Modules.Tickets.Features.v1.Tickets.ListTicketComments;
using YH.Modules.Tickets.Features.v1.Tickets.ListTrashedTickets;
using YH.Modules.Tickets.Features.v1.Tickets.ReopenTicket;
using YH.Modules.Tickets.Features.v1.Tickets.ResolveTicket;
using YH.Modules.Tickets.Features.v1.Tickets.RestoreTicket;
using YH.Modules.Tickets.Features.v1.Tickets.SearchTickets;
using YH.Modules.Tickets.Features.v1.Tickets.UpdateTicket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(YH.Modules.Tickets.TicketsModule), 700)]

namespace YH.Modules.Tickets;

public sealed class TicketsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(TicketsPermissions.All);

        builder.Services.AddHeroDbContext<TicketsDbContext>();
        builder.Services.AddScoped<IDbInitializer, TicketsDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<TicketsDbContext>(
                name: "db:tickets",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No custom middleware needed
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}")
            .WithTags("Tickets")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // Trash + comment routes register before the catch-all `{ticketId:guid}` GET so literal
        // segments win — minimal APIs match the first compatible pattern, so order matters.
        group.MapListTrashedTicketsEndpoint();
        group.MapAddTicketCommentEndpoint();
        group.MapListTicketCommentsEndpoint();

        group.MapRestoreTicketEndpoint();
        group.MapAssignTicketEndpoint();
        group.MapResolveTicketEndpoint();
        group.MapReopenTicketEndpoint();
        group.MapCloseTicketEndpoint();

        group.MapCreateTicketEndpoint();
        group.MapSearchTicketsEndpoint();
        group.MapUpdateTicketEndpoint();
        group.MapDeleteTicketEndpoint();
        group.MapGetTicketByIdEndpoint();
    }
}
