using FluentValidation;
using YH.Modules.Catalog.Contracts.v1.Products.RemoveProductImage;

namespace YH.Modules.Catalog.Features.v1.Products.RemoveProductImage;

public sealed class RemoveProductImageCommandValidator : AbstractValidator<RemoveProductImageCommand>
{
    public RemoveProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ImageId).NotEmpty();
    }
}
