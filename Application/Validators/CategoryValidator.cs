using FluentValidation;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Validators;

/// <summary>
/// Validator for <see cref="CategoryDto"/>.
/// </summary>
public class CategoryValidator : AbstractValidator<CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        ConfigureValidationRules();
    }

    private void ConfigureValidationRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("The category name is required")
            .Length(2, 50)
            .WithMessage("The name must be between 2 and 50 characters")
            .MustAsync(BeUniqueName)
            .WithMessage("A category with this name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("The description cannot exceed 200 characters");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("The color is required")
            .Matches(@"^#([A-Fa-f0-9]{6})$")
            .WithMessage("The color must be in hexadecimal format (#RRGGBB)");

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage("The icon is required")
            .MaximumLength(50)
            .WithMessage("The icon name cannot exceed 50 characters");
    }

    private async Task<bool> BeUniqueName(CategoryDto category, string name, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(name);
        return existingCategory == null || existingCategory.Id == category.Id;
    }
}