using FluentValidation;
using PromptTemplateManager.Application.DTOs;

namespace PromptTemplateManager.Application.Validators;

public class UpdateTemplateRequestValidator : AbstractValidator<UpdateTemplateRequest>
{
    public UpdateTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Template content cannot be null");
    }
}
