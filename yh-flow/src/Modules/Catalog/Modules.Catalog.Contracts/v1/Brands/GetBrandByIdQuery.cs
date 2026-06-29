using YH.Modules.Catalog.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Brands;

public sealed record GetBrandByIdQuery(Guid BrandId) : IQuery<BrandDto>;
