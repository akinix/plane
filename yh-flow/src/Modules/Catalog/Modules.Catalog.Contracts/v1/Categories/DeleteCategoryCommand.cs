using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Categories;

public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand<Unit>;
