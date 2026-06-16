using FluentValidation;
using YH.Modules.Identity.Contracts.v1.Roles.DeleteRole;

namespace YH.Modules.Identity.Features.v1.Roles.DeleteRole;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}