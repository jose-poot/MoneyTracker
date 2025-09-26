using FluentValidation;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Validators;

/// <summary>
/// Validador para CategoryDto
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
            .WithMessage("El nombre de la categoría es obligatorio")
            .Length(2, 50)
            .WithMessage("El nombre debe tener entre 2 y 50 caracteres")
            .MustAsync(BeUniqueName)
            .WithMessage("Ya existe una categoría con este nombre");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("La descripción no puede tener más de 200 caracteres");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("El color es obligatorio")
            .Matches(@"^#([A-Fa-f0-9]{6})$")
            .WithMessage("El color debe estar en formato hexadecimal (#RRGGBB)");

        RuleFor(x => x.Icon)
            .NotEmpty()
            .WithMessage("El icono es obligatorio")
            .MaximumLength(50)
            .WithMessage("El nombre del icono no puede tener más de 50 caracteres");
    }

    private async Task<bool> BeUniqueName(CategoryDto category, string name, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(name);
        return existingCategory == null || existingCategory.Id == category.Id;
    }
}