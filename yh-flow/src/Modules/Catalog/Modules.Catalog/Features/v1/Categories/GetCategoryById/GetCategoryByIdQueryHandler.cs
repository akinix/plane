using YH.Framework.Core.Exceptions;
using YH.Modules.Catalog.Contracts.Dtos;
using YH.Modules.Catalog.Contracts.v1.Categories;
using YH.Modules.Catalog.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Catalog.Features.v1.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async ValueTask<CategoryDto> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var c = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CategoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Category {query.CategoryId} not found.");

        return new CategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ParentCategoryId, c.CreatedAtUtc, c.UpdatedAtUtc, c.DeletedOnUtc, c.DeletedBy);
    }
}
