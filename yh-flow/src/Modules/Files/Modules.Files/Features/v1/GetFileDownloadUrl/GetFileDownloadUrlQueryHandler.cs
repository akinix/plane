using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Framework.Storage.Services;
using YH.Modules.Files.Contracts;
using YH.Modules.Files.Contracts.v1.DTOs;
using YH.Modules.Files.Contracts.v1.Queries;
using YH.Modules.Files.Data;
using YH.Modules.Files.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace YH.Modules.Files.Features.v1.GetFileDownloadUrl;

public sealed class GetFileDownloadUrlQueryHandler(
    FilesDbContext db,
    IStorageService storage,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IOptions<FilesOptions> options)
    : IQueryHandler<GetFileDownloadUrlQuery, PresignedDownloadResponse>
{
    public async ValueTask<PresignedDownloadResponse> Handle(GetFileDownloadUrlQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        var f = await db.FileAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new NotFoundException("file not found");

        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanReadAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new NotFoundException("file not found");
        }

        var ttl = TimeSpan.FromMinutes(options.Value.DownloadUrlTtlMinutes);
        var mode = q.Inline ? "inline" : "attachment";
        var disposition = $"{mode}; filename=\"{f.OriginalFileName}\"";
        var url = await storage.GenerateDownloadUrlAsync(f.StorageKey, ttl, disposition, cancellationToken).ConfigureAwait(false);
        return new PresignedDownloadResponse(url, DateTimeOffset.UtcNow.Add(ttl));
    }
}
