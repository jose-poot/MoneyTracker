using FluentValidation;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Validators;

/// <summary>
/// Validador para CreateTransactionDto usando FluentValidation
/// </summary>
public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateTransactionValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        ConfigureValidationRules();
    }

    private void ConfigureValidationRules()
    {
        // Regla 1: Descripción
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es obligatoria")
            .Length(3, 200)
            .WithMessage("La descripción debe tener entre 3 y 200 caracteres")
            .Must(BeValidDescription)
            .WithMessage("La descripción contiene caracteres no válidos");

        // Regla 2: Monto
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a cero")
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("El monto no puede ser mayor a $999,999,999.99")
            .Must(HaveValidDecimalPlaces)
            .WithMessage("El monto no puede tener más de 2 decimales");

        // Regla 3: Moneda
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es obligatoria")
            .Length(3)
            .WithMessage("La moneda debe tener exactamente 3 caracteres")
            .Must(BeValidCurrency)
            .WithMessage("Moneda no válida");

        // Regla 4: Tipo de transacción
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de transacción no válido");

        // Regla 5: Categoría (validación asíncrona)
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar una categoría")
            .MustAsync(CategoryExists)
            .WithMessage("La categoría seleccionada no existe")
            .MustAsync(CategoryIsActive)
            .WithMessage("La categoría seleccionada está inactiva");

        // Regla 6: Fecha
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("La fecha es obligatoria")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("La fecha no puede ser futura")
            .GreaterThan(new DateTime(2000, 1, 1))
            .WithMessage("La fecha no puede ser anterior al año 2000");

        // Regla 7: Notas (opcional)
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Las notas no pueden tener más de 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Regla 8: Ubicación (opcional)
        RuleFor(x => x.Location)
            .MaximumLength(100)
            .WithMessage("La ubicación no puede tener más de 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }

    // Validaciones personalizadas
    private static bool BeValidDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return false;

        // No permitir solo números o caracteres especiales
        return description.Any(char.IsLetter);
    }

    private static bool HaveValidDecimalPlaces(decimal amount)
    {
        return decimal.Round(amount, 2) == amount;
    }

    private static bool BeValidCurrency(string currency)
    {
        var validCurrencies = new[] { "USD", "EUR", "MXN", "CAD", "GBP" };
        return validCurrencies.Contains(currency.ToUpperInvariant());
    }

    // Validaciones asíncronas con base de datos
    private async Task<bool> CategoryExists(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category != null;
    }

    private async Task<bool> CategoryIsActive(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category?.IsActive ?? false;
    }
}