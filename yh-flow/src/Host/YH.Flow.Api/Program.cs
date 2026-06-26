using YH.Framework.Web;
using YH.Framework.Web.Modules;
using YH.Modules.Auditing;
using YH.Modules.Identity;
using YH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;
using YH.Modules.Identity.Features.v1.Tokens.TokenGeneration;
using YH.Modules.Multitenancy;
using YH.Modules.Multitenancy.Contracts.v1.GetTenantStatus;
using YH.Modules.Project;
using YH.Modules.WorkItems;
using YH.Modules.Page;
using YH.Modules.View;
using YH.Modules.Analytics;
using YH.Modules.Webhooks;
using YH.Modules.Multitenancy.Features.v1.GetTenantStatus;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as string names (reads still accept names or integers). [Flags] enums (AuditTag, BodyCapture)
// opt back to numeric via their own NumericEnumConverter since comma-joined flag strings break bitwise consumers. Frontends mirror this as string unions.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower; // FLOW: SCAFF-06
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // FLOW: SCAFF-06
});

// FLOW: Configure MVC JSON options for [ApiController] endpoints (SCAFF-06)
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower; // FLOW: SCAFF-06
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // FLOW: SCAFF-06
});

if (builder.Environment.IsProduction())
{
    static void Require(IConfiguration config, string key)
    {
        if (string.IsNullOrWhiteSpace(config[key]))
        {
            throw new InvalidOperationException($"Missing required configuration '{key}' in Production.");
        }
    }

    var config = builder.Configuration;
    Require(config, "DatabaseOptions:ConnectionString");
    Require(config, "CachingOptions:Redis");
    Require(config, "JwtOptions:SigningKey");
}

builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
    o.Assemblies = [
        typeof(GenerateTokenCommand),
        typeof(GenerateTokenCommandHandler),
        typeof(GetTenantStatusQuery),
        typeof(GetTenantStatusQueryHandler),
        typeof(YH.Modules.Auditing.Contracts.AuditEnvelope),
        typeof(YH.Modules.Auditing.Persistence.AuditDbContext),
        typeof(YH.Modules.Webhooks.Contracts.v1.CreateWebhookSubscription.CreateWebhookSubscriptionCommand),
        typeof(YH.Modules.Webhooks.WebhooksModule),
        typeof(YH.Modules.Files.Contracts.v1.Commands.RequestUploadUrlCommand),
        typeof(YH.Modules.Files.FilesModule),
        typeof(YH.Modules.Notifications.Contracts.v1.Commands.MarkNotificationReadCommand),
        typeof(YH.Modules.Notifications.NotificationsModule),
        typeof(YH.Modules.Project.Contracts.ProjectConstants),
        typeof(YH.Modules.Page.Contracts.v1.Pages.CreatePage.CreatePageCommand),
        typeof(YH.Modules.Page.Features.v1.Pages.ListPages.ListPagesQueryHandler)];
});

var moduleAssemblies = new Assembly[]
{
    typeof(IdentityModule).Assembly,
    typeof(MultitenancyModule).Assembly,
    typeof(AuditingModule).Assembly,
    typeof(YH.Modules.Files.FilesModule).Assembly,
    typeof(WebhooksModule).Assembly,
    typeof(YH.Modules.Notifications.NotificationsModule).Assembly,
    typeof(YH.Modules.Workspace.WorkspaceModule).Assembly,
    typeof(ProjectModule).Assembly,
    typeof(WorkItemsModule).Assembly,
    typeof(PageModule).Assembly,
    typeof(ViewModule).Assembly,
    typeof(AnalyticsModule).Assembly,
};

builder.AddHeroPlatform(o =>
{
    o.EnableCaching = true;
    o.EnableMailing = true;
    o.EnableJobs = true;
    o.EnableQuotas = true;
    o.EnableSse = true;
    o.EnableRealtime = true;
});

builder.AddModules(moduleAssemblies);

// Self-heal deployments carrying retired per-module `{module}-outbox-dispatcher` Hangfire recurring jobs
// (the outbox is now dispatched by OutboxDispatcherHostedService). No-op once the storage is clean.
builder.Services.AddHostedService<YH.Flow.Api.OrphanedOutboxRecurringJobCleanupService>();

// Demo data is provisioned by the DbMigrator's `seed-demo` verb, not the API — the API never mutates data on startup.
// See src/Host/YH.Flow.DbMigrator/README.md.

var app = builder.Build();

app.UseHeroMultiTenantDatabases();
app.UseHeroPlatform(p =>
{
    p.MapModules = true;
    p.ServeStaticFiles = true;
    p.UseQuotas = true;
    p.MapSseEndpoints = true;
    p.MapRealtime = true;
});

app.MapGet("/", () => Results.Ok(new { message = "hello world!" }))
   .WithTags("PlayGround")
   .AllowAnonymous();
await app.RunAsync();