using YH.Modules.Catalog.Contracts.Dtos;
using Mediator;

namespace YH.Modules.Catalog.Contracts.v1.Categories;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<CategoryDto>;
