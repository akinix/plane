using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueLinks.CreateIssueLink;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueLinks.CreateIssueLink;

/// <summary>
/// Handles <see cref="CreateIssueLinkCommand"/> — creates an issue link.
/// Validates at least one of RelatedIssueId or Url is provided.
/// Validates RelatedIssueId exists in the same project.
/// </summary>
public sealed class CreateIssueLinkCommandHandler : ICommandHandler<CreateIssueLinkCommand, IssueLinkDto>
{
    private readonly WorkItemsDbContext _db;

    public CreateIssueLinkCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IssueLinkDto> Handle(CreateIssueLinkCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IssueId == Guid.Empty)
        {
            throw new CustomException("Issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Validate at least one of RelatedIssueId or Url is provided
        if (command.RelatedIssueId is null && string.IsNullOrWhiteSpace(command.Url))
        {
            throw new CustomException(
                "Either a related issue id or a URL must be provided.",
                Array.Empty<string>(),
                System.Net.HttpStatusCode.BadRequest);
        }

        // Validate parent issue exists
        var issueExists = await _db.Issues
            .AnyAsync(i => i.Id == command.IssueId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!issueExists)
        {
            throw new NotFoundException($"Issue '{command.IssueId}' was not found.");
        }

        // Validate RelatedIssueId exists in the same project (T-4-issue-crud-06)
        if (command.RelatedIssueId.HasValue && command.RelatedIssueId.Value != Guid.Empty)
        {
            var relatedExists = await _db.Issues
                .AnyAsync(i => i.Id == command.RelatedIssueId.Value
                            && i.ProjectId == command.ProjectId
                            && !i.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (!relatedExists)
            {
                throw new CustomException(
                    "Related issue not found in this project.",
                    Array.Empty<string>(),
                    System.Net.HttpStatusCode.BadRequest);
            }
        }

        // Validate LinkType is a known value
        if (!Enum.IsDefined(typeof(LinkType), command.LinkType))
        {
            throw new CustomException(
                $"Invalid link type: {command.LinkType}. Must be 0 (RelatesTo), 1 (Duplicate), 2 (Blocks), or 3 (BlockedBy).",
                Array.Empty<string>(),
                System.Net.HttpStatusCode.BadRequest);
        }

        var link = IssueLink.Create(
            issueId: command.IssueId,
            linkType: (LinkType)command.LinkType,
            relatedIssueId: command.RelatedIssueId,
            url: command.Url,
            title: command.Title,
            metadata: command.Metadata);

        _db.Set<IssueLink>().Add(link);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return IssueLinkDtoMapper.ToDto(link);
    }
}
