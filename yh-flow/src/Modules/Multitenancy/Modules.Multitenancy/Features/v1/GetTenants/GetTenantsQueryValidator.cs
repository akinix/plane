using FluentValidation;
using YH.Framework.Web.Validation;
using YH.Modules.Multitenancy.Contracts.v1.GetTenants;

namespace YH.Modules.Multitenancy.Features.v1.GetTenants;

public sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
        Include(new PagedQueryValidator<GetTenantsQuery>());
    }
}