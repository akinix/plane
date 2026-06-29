using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Products;

public sealed record ChangeProductPriceCommand(
    Guid ProductId,
    decimal Amount,
    string Currency) : ICommand<Guid>;
