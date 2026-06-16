using YH.Framework.Shared.Persistence;
using YH.Modules.Catalog.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Products;

public sealed record ListTrashedProductsQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<PagedResponse<ProductDto>>;
