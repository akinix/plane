using Asp.Versioning;
using YH.Framework.Persistence;
using YH.Framework.Shared.Constants;
using YH.Framework.Web.Modules;
using YH.Modules.Catalog.Authorization;
using YH.Modules.Catalog.Contracts.Authorization;
using YH.Modules.Catalog.Data;
using YH.Modules.Files.Contracts;
using YH.Modules.Catalog.Features.v1.Brands.CreateBrand;
using YH.Modules.Catalog.Features.v1.Brands.DeleteBrand;
using YH.Modules.Catalog.Features.v1.Brands.GetBrandById;
using YH.Modules.Catalog.Features.v1.Brands.ListTrashedBrands;
using YH.Modules.Catalog.Features.v1.Brands.RestoreBrand;
using YH.Modules.Catalog.Features.v1.Brands.SearchBrands;
using YH.Modules.Catalog.Features.v1.Brands.UpdateBrand;
using YH.Modules.Catalog.Features.v1.Categories.CreateCategory;
using YH.Modules.Catalog.Features.v1.Categories.DeleteCategory;
using YH.Modules.Catalog.Features.v1.Categories.GetCategoryById;
using YH.Modules.Catalog.Features.v1.Categories.GetCategoryTree;
using YH.Modules.Catalog.Features.v1.Categories.ListTrashedCategories;
using YH.Modules.Catalog.Features.v1.Categories.RestoreCategory;
using YH.Modules.Catalog.Features.v1.Categories.SearchCategories;
using YH.Modules.Catalog.Features.v1.Categories.UpdateCategory;
using YH.Modules.Catalog.Features.v1.Products.AddProductImage;
using YH.Modules.Catalog.Features.v1.Products.AdjustProductStock;
using YH.Modules.Catalog.Features.v1.Products.ChangeProductPrice;
using YH.Modules.Catalog.Features.v1.Products.CreateProduct;
using YH.Modules.Catalog.Features.v1.Products.DeleteProduct;
using YH.Modules.Catalog.Features.v1.Products.GetProductById;
using YH.Modules.Catalog.Features.v1.Products.ListTrashedProducts;
using YH.Modules.Catalog.Features.v1.Products.RemoveProductImage;
using YH.Modules.Catalog.Features.v1.Products.ReorderProductImages;
using YH.Modules.Catalog.Features.v1.Products.RestoreProduct;
using YH.Modules.Catalog.Features.v1.Products.SearchProducts;
using YH.Modules.Catalog.Features.v1.Products.SetProductThumbnail;
using YH.Modules.Catalog.Features.v1.Products.UpdateProduct;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(YH.Modules.Catalog.CatalogModule), 600)]

namespace YH.Modules.Catalog;

public sealed class CatalogModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(CatalogPermissions.All);

        builder.Services.AddHeroDbContext<CatalogDbContext>();
        builder.Services.AddScoped<IDbInitializer, CatalogDbInitializer>();

        // OwnerType=Product policy for Files module attachments (product images).
        builder.Services.AddScoped<IFileAccessPolicy, ProductFileAccessPolicy>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<CatalogDbContext>(
                name: "db:catalog",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No custom middleware needed
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/catalog")
            .WithTags("Catalog")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // Trash routes registered first so the literal `/trash` segment wins
        // over the catch-all `/{id:guid}`.
        group.MapListTrashedBrandsEndpoint();
        group.MapRestoreBrandEndpoint();
        group.MapCreateBrandEndpoint();
        group.MapUpdateBrandEndpoint();
        group.MapDeleteBrandEndpoint();
        group.MapGetBrandByIdEndpoint();
        group.MapSearchBrandsEndpoint();

        // Category /tree and /trash must be registered before /{categoryId:guid}
        // so the literal routes win.
        group.MapGetCategoryTreeEndpoint();
        group.MapListTrashedCategoriesEndpoint();
        group.MapRestoreCategoryEndpoint();
        group.MapCreateCategoryEndpoint();
        group.MapUpdateCategoryEndpoint();
        group.MapDeleteCategoryEndpoint();
        group.MapGetCategoryByIdEndpoint();
        group.MapSearchCategoriesEndpoint();

        group.MapListTrashedProductsEndpoint();
        group.MapRestoreProductEndpoint();
        group.MapCreateProductEndpoint();
        group.MapUpdateProductEndpoint();
        group.MapDeleteProductEndpoint();
        group.MapChangeProductPriceEndpoint();
        group.MapAdjustProductStockEndpoint();

        // Product images — collection sub-resource under /products/{id}/images.
        group.MapAddProductImageEndpoint();
        group.MapRemoveProductImageEndpoint();
        group.MapSetProductThumbnailEndpoint();
        group.MapReorderProductImagesEndpoint();

        group.MapGetProductByIdEndpoint();
        group.MapSearchProductsEndpoint();
    }
}
