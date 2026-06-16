using System.Net;
using YH.Framework.Core.Exceptions;
using YH.Modules.Catalog.Contracts.v1.Products;
using YH.Modules.Catalog.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Catalog.Features.v1.Products.AdjustProductStock;

public sealed class AdjustProductStockCommandHandler(CatalogDbContext dbContext)
    : ICommandHandler<AdjustProductStockCommand, int>
{
    public async ValueTask<int> Handle(AdjustProductStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Product {command.ProductId} not found.");

        try
        {
            product.AdjustStock(command.Delta);
        }
        catch (InvalidOperationException ex)
        {
            throw new CustomException(ex.Message, (IEnumerable<string>?)null, HttpStatusCode.Conflict);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return product.Stock;
    }
}
