using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Brands;

public sealed record DeleteBrandCommand(Guid BrandId) : ICommand<Unit>;
