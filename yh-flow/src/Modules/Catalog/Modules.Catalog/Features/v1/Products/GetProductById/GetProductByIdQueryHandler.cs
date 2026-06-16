using YH.Framework.Core.Exceptions;
using YH.Modules.Catalog.Contracts.Dtos;
using YH.Modules.Catalog.Contracts.v1.Products;
using YH.Modules.Catalog.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Catalog.Features.v1.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async ValueTask<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Product {query.ProductId} not found.");

        return product.ToDto();
    }
}
