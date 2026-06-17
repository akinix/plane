using System.Security.Cryptography;
using System.Text;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Data;
using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace YH.Modules.Identity.Services;

public sealed class ApiTokenService : IApiTokenService
{
    private const string KeyPrefix = "pk_";
    private const int KeyByteLength = 32;

    private readonly IdentityDbContext _dbContext;
    private readonly IUserPermissionService _permissionService;
    private readonly ILogger<ApiTokenService> _logger;

    public ApiTokenService(
        IdentityDbContext dbContext,
        IUserPermissionService permissionService,
        ILogger<ApiTokenService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(permissionService);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<(APITokenDto Dto, string RawKey)> CreateAsync(
        string name,
        string userId,
        string tenantId,
        DateTime? expiredAt,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(tenantId);

        // Generate raw key: "pk_" + 32 random hex characters
        var rawKey = GenerateRawKey();
        var prefix = KeyPrefix + rawKey[KeyPrefix.Length..Math.Min(KeyPrefix.Length + 8, rawKey.Length)];
        var hash = HashToken(rawKey);

        var token = APIToken.Create(name, userId, hash, prefix, tenantId, expiredAt);
        _dbContext.ApiTokens.Add(token);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created API token '{TokenName}' (prefix={Prefix}) for user {UserId}", name, prefix, userId);
        }

        var dto = MapToDto(token);
        return (dto, rawKey);
    }

    public async Task<IReadOnlyList<APITokenDto>> ListForUserAsync(string userId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var tokens = await _dbContext.ApiTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new APITokenDto(
                t.Id,
                t.Name,
                t.Prefix,
                t.UserId,
                t.TenantId,
                t.ExpiredAt,
                t.IsActive,
                t.LastUsed,
                t.CreatedAt))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return tokens;
    }

    public async Task RevokeAsync(Guid tokenId, string userId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var token = await _dbContext.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId, ct)
            .ConfigureAwait(false);

        if (token is null)
        {
            throw new KeyNotFoundException($"API token '{tokenId}' was not found or does not belong to the current user.");
        }

        token.Revoke();
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Revoked API token '{TokenId}' for user {UserId}", tokenId, userId);
        }
    }

    public async Task<ApiKeyValidationResult?> ValidateAndGetOwnerAsync(string rawKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            return null;
        }

        var hash = HashToken(rawKey);

        var token = await _dbContext.ApiTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            .ConfigureAwait(false);

        if (token is null)
        {
            return null;
        }

        // Check active status
        if (!token.IsActive)
        {
            return null;
        }

        // Check expiry
        if (token.IsExpired())
        {
            return null;
        }

        // Look up the user to get email
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == token.UserId, ct)
            .ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        // Get permissions
        var permissions = await _permissionService.GetPermissionsAsync(token.UserId, ct).ConfigureAwait(false);

        // Record usage (fire-and-forget, don't await — but we need to save)
        // Actually we need to update, so get tracked entity
        var trackedToken = await _dbContext.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == token.Id, ct)
            .ConfigureAwait(false);

        if (trackedToken is not null)
        {
            trackedToken.RecordUsage();
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        return new ApiKeyValidationResult(
            UserId: token.UserId,
            Email: user.Email ?? string.Empty,
            TenantId: token.TenantId,
            Permissions: (IReadOnlyList<string>)(permissions ?? []),
            TokenId: token.Id);
    }

    private static string GenerateRawKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(KeyByteLength);
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return KeyPrefix + hex;
    }

    private static string HashToken(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static APITokenDto MapToDto(APIToken token)
    {
        return new APITokenDto(
            token.Id,
            token.Name,
            token.Prefix,
            token.UserId,
            token.TenantId,
            token.ExpiredAt,
            token.IsActive,
            token.LastUsed,
            token.CreatedAt);
    }
}
