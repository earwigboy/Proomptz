using FluentValidation;
using PromptTemplateManager.Application.DTOs;

namespace PromptTemplateManager.Application.Validators;

public class CreateFolderRequestValidator : AbstractValidator<CreateFolderRequest>
{
    public CreateFolderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Folder name is required")
            .MaximumLength(100).WithMessage("Folder name must not exceed 100 characters")
            .Matches(@"^[^/\\:*?""<>|]+$").WithMessage("Folder name contains invalid characters");
    }
}
