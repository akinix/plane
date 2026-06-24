using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Import.ImportIssues;

namespace YH.Modules.WorkItems.Features.v1.ImportExport.ImportIssues;

/// <summary>
/// Validator for <see cref="ImportIssuesCommand"/>.
/// </summary>
public sealed class ImportIssuesCommandValidator : AbstractValidator<ImportIssuesCommand>
{
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ImportIssuesCommandValidator()
    {
        RuleFor(x => x.Format)
            .NotEmpty().WithMessage("Format is required.")
            .Must(f => f == "csv" || f == "json")
            .WithMessage("Format must be 'csv' or 'json'.");

        RuleFor(x => x.FileContent)
            .NotNull().WithMessage("File content is required.")
            .Must(content => content.Length > 0).WithMessage("File content cannot be empty.")
            .Must(content => content.Length <= MaxFileSize).WithMessage($"File content must not exceed 10MB.");
    }
}
