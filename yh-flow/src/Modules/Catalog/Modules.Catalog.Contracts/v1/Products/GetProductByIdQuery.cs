using YH.Modules.Catalog.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Products;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;
