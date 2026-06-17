using Asp.Versioning;
using YH.Framework.Core.Context;
using YH.Framework.Eventing;
using YH.Framework.Persistence;
using YH.Framework.Quota;
using YH.Framework.Storage;
using YH.Framework.Storage.Local;
using YH.Framework.Storage.Services;
using YH.Framework.Web.Modules;
using YH.Modules.Identity.Authorization;
using YH.Modules.Identity.Authorization.ApiKey;
using YH.Modules.Identity.Authorization.Jwt;
using YH.Modules.Identity.Authorization.SessionCookie;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Data;
using YH.Modules.Identity.Domain;
using YH.Modules.Identity.Features.v1.Groups.AddUsersToGroup;
using YH.Modules.Identity.Features.v1.Groups.CreateGroup;
using YH.Modules.Identity.Features.v1.Groups.DeleteGroup;
using YH.Modules.Identity.Features.v1.Groups.GetGroupById;
using YH.Modules.Identity.Features.v1.Groups.GetGroupMembers;
using YH.Modules.Identity.Features.v1.Groups.GetGroups;
using YH.Modules.Identity.Features.v1.Groups.RemoveUserFromGroup;
using YH.Modules.Identity.Features.v1.Groups.UpdateGroup;
using YH.Modules.Identity.Features.v1.Impersonation.EndImpersonation;
using YH.Modules.Identity.Features.v1.Impersonation.GetImpersonationGrants;
using YH.Modules.Identity.Features.v1.Impersonation.RevokeImpersonationGrant;
using YH.Modules.Identity.Features.v1.Impersonation.StartImpersonation;
using YH.Modules.Identity.Features.v1.Permissions.GetPermissionCatalog;
using YH.Modules.Identity.Features.v1.Roles;
using YH.Modules.Identity.Features.v1.Roles.DeleteRole;
using YH.Modules.Identity.Features.v1.Roles.GetRoleById;
using YH.Modules.Identity.Features.v1.Roles.GetRoles;
using YH.Modules.Identity.Features.v1.Roles.GetRoleWithPermissions;
using YH.Modules.Identity.Features.v1.Roles.UpdateRolePermissions;
using YH.Modules.Identity.Features.v1.Roles.UpsertRole;
using YH.Modules.Identity.Features.v1.Sessions.AdminRevokeAllSessions;
using YH.Modules.Identity.Features.v1.Sessions.AdminRevokeSession;
using YH.Modules.Identity.Features.v1.Sessions.GetMySessions;
using YH.Modules.Identity.Features.v1.Sessions.GetTenantSessions;
using YH.Modules.Identity.Features.v1.Sessions.GetUserSessions;
using YH.Modules.Identity.Features.v1.Sessions.RevokeAllSessions;
using YH.Modules.Identity.Features.v1.Sessions.RevokeSession;
using YH.Modules.Identity.Features.v1.Tokens.RefreshToken;
using YH.Modules.Identity.Features.v1.Tokens.TokenGeneration;
using YH.Modules.Identity.Features.v1.TwoFactor.Disable;
using YH.Modules.Identity.Features.v1.TwoFactor.Enroll;
using YH.Modules.Identity.Features.v1.TwoFactor.VerifyEnroll;
using YH.Modules.Identity.Features.v1.Users.AssignUserRoles;
using YH.Modules.Identity.Features.v1.Users.ChangePassword;
using YH.Modules.Identity.Features.v1.Users.AdminConfirmEmail;
using YH.Modules.Identity.Features.v1.Users.ConfirmEmail;
using YH.Modules.Identity.Features.v1.Users.ResendConfirmationEmail;
using YH.Modules.Identity.Features.v1.Users.DeleteUser;
using YH.Modules.Identity.Features.v1.Users.ForgotPassword;
using YH.Modules.Identity.Features.v1.Users.GetUserById;
using YH.Modules.Identity.Features.v1.Users.GetUserGroups;
using YH.Modules.Identity.Features.v1.Users.GetUserPermissions;
using YH.Modules.Identity.Features.v1.Users.GetUserProfile;
using YH.Modules.Identity.Features.v1.Users.GetUserRoles;
using YH.Modules.Identity.Features.v1.Users.GetUsers;
using YH.Modules.Identity.Features.v1.Users.RegisterUser;
using YH.Modules.Identity.Features.v1.Users.ResetPassword;
using YH.Modules.Identity.Features.v1.Users.SearchUsers;
using YH.Modules.Identity.Features.v1.Users.SelfRegistration;
using YH.Modules.Identity.Features.v1.Users.SetProfileImage;
using YH.Modules.Identity.Features.v1.Users.ToggleUserStatus;
using YH.Modules.Identity.Features.v1.Users.UpdateUser;
using YH.Modules.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace YH.Modules.Identity;

public class IdentityModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        YH.Framework.Shared.Constants.PermissionConstants.Register(
            YH.Modules.Identity.Contracts.Authorization.IdentityPermissions.All);

        var services = builder.Services;
        services.AddScoped<RolePermissionSyncer>();
        services.AddHostedService<RolePermissionSyncHostedService>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PathAwareAuthorizationHandler>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<ICurrentUserInitializer>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<IRequestContextService, RequestContextService>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextService>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IImpersonationGrantService, ImpersonationGrantService>();

        // User services - focused single-responsibility services
        services.AddTransient<IUserRegistrationService, UserRegistrationService>();
        services.AddTransient<IUserProfileService, UserProfileService>();
        services.AddTransient<IUserStatusService, UserStatusService>();
        services.AddTransient<IUserRoleService, UserRoleService>();
        services.AddTransient<IUserPasswordService, UserPasswordService>();
        services.AddTransient<IUserPermissionService, UserPermissionService>();

        // Facade for backward compatibility
        services.AddTransient<IUserService, UserService>();

        services.AddTransient<IRoleService, RoleService>();
        services.AddHeroStorage(builder.Configuration);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddHeroDbContext<IdentityDbContext>();
        services.AddEventingCore(builder.Configuration);
        services.AddEventingForDbContext<IdentityDbContext>();
        services.AddIntegrationEventHandlers(typeof(IdentityModule).Assembly);
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<IdentityDbContext>(
                name: "db:identity",
                failureStatus: HealthStatus.Unhealthy);
        services.AddScoped<IDbInitializer, IdentityDbInitializer>();

        // Configure password policy options
        services.Configure<PasswordPolicyOptions>(builder.Configuration.GetSection("PasswordPolicy"));

        // Tenant subscription grace window (shared "Billing" section) — used by the login expiry check.
        services.Configure<TenantGraceOptions>(builder.Configuration.GetSection(TenantGraceOptions.SectionName));

        // Register password history service
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();

        // Register password expiry service
        services.AddScoped<IPasswordExpiryService, PasswordExpiryService>();

        // Register session service and background cleanup
        services.AddScoped<ISessionService, SessionService>();
        services.AddHostedService<SessionCleanupHostedService>();

        // Register group role service for group-derived permissions
        services.AddScoped<IGroupRoleService, GroupRoleService>();

        // Quota gauge: reports live user count per tenant for the Users quota.
        services.AddScoped<IQuotaGaugeProvider, UserCountQuotaGaugeProvider>();

        services.AddIdentity<FshUser, FshRole>(options =>
        {
            options.Password.RequiredLength = IdentityModuleConstants.PasswordLength;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;

            // Account lockout: 5 consecutive failed logins → 15-minute lockout (applies to new users by default).
            // IdentityService's login flow drives AccessFailedAsync / IsLockedOutAsync.
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
           .AddEntityFrameworkStores<IdentityDbContext>()
           .AddDefaultTokenProviders();

        //metrics
        services.AddSingleton<IdentityMetrics>();

        services.ConfigureJwtAuth();
        services.ConfigureApiKeyAuth();
        services.ConfigureSessionCookieAuth();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/identity")
            .WithTags("Identity")
            .WithApiVersionSet(apiVersionSet);

        // tokens
        group.MapGenerateTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");
        group.MapRefreshTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");

        // The outbox is dispatched by the framework's OutboxDispatcherHostedService (on by default). A second dispatcher
        // here would race the same rows (no row-level claim) → duplicate handlers + PK_InboxMessages collisions, so this module registers none.

        // roles
        group.MapGetRolesEndpoint();
        group.MapGetRoleByIdEndpoint();
        group.MapDeleteRoleEndpoint();
        group.MapGetRolePermissionsEndpoint();
        group.MapUpdateRolePermissionsEndpoint();
        group.MapCreateOrUpdateRoleEndpoint();

        // permission catalog — every permission registered with the host,
        // filtered to the caller's tenant context (root vs admin set)
        group.MapGetPermissionCatalogEndpoint();

        // users
        group.MapAssignUserRolesEndpoint();
        group.MapChangePasswordEndpoint();
        group.MapAdminConfirmEmailEndpoint();
        group.MapResendConfirmationEmailEndpoint().RequireRateLimiting("auth");
        group.MapConfirmEmailEndpoint().RequireRateLimiting("auth");
        group.MapDeleteUserEndpoint();
        group.MapGetUserByIdEndpoint();
        group.MapGetCurrentUserPermissionsEndpoint();
        group.MapGetMeEndpoint();
        group.MapGetUserRolesEndpoint();
        group.MapGetUsersListEndpoint();
        group.MapSearchUsersEndpoint();
        group.MapRegisterUserEndpoint();
        group.MapForgotPasswordEndpoint().RequireRateLimiting("auth");
        group.MapResetPasswordEndpoint().RequireRateLimiting("auth");
        group.MapSelfRegisterUserEndpoint().RequireRateLimiting("auth");
        group.MapToggleUserStatusEndpoint();
        group.MapUpdateUserEndpoint();
        group.MapSetProfileImageEndpoint();

        // sessions - user endpoints
        group.MapGetMySessionsEndpoint();
        group.MapRevokeSessionEndpoint();
        group.MapRevokeAllSessionsEndpoint();

        // sessions - admin endpoints
        group.MapGetTenantSessionsEndpoint();
        group.MapGetUserSessionsEndpoint();
        group.MapAdminRevokeSessionEndpoint();
        group.MapAdminRevokeAllSessionsEndpoint();

        // groups
        group.MapGetGroupsEndpoint();
        group.MapGetGroupByIdEndpoint();
        group.MapCreateGroupEndpoint();
        group.MapUpdateGroupEndpoint();
        group.MapDeleteGroupEndpoint();
        group.MapGetGroupMembersEndpoint();
        group.MapAddUsersToGroupEndpoint();
        group.MapRemoveUserFromGroupEndpoint();

        // user groups
        group.MapGetUserGroupsEndpoint();

        // impersonation
        group.MapStartImpersonationEndpoint();
        group.MapEndImpersonationEndpoint();
        group.MapGetImpersonationGrantsEndpoint();
        group.MapRevokeImpersonationGrantEndpoint();

        // two-factor authentication (TOTP)
        group.MapEnrollTwoFactorEndpoint();
        group.MapVerifyEnrollTwoFactorEndpoint();
        group.MapDisableTwoFactorEndpoint();
    }
}