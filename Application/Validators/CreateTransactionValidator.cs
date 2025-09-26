using FluentValidation;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Validators;

/// <summary>
/// Validator for <see cref="CreateTransactionDto"/> using FluentValidation.
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
        // Rule 1: Description
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("The description is required")
            .Length(3, 200)
            .WithMessage("The description must be between 3 and 200 characters")
            .Must(BeValidDescription)
            .WithMessage("The description contains invalid characters");

        // Rule 2: Amount
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("The amount must be greater than zero")
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("The amount cannot exceed $999,999,999.99")
            .Must(HaveValidDecimalPlaces)
            .WithMessage("The amount cannot have more than 2 decimal places");

        // Rule 3: Currency
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("The currency is required")
            .Length(3)
            .WithMessage("The currency must have exactly 3 characters")
            .Must(BeValidCurrency)
            .WithMessage("Invalid currency");

        // Rule 4: Transaction type
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid transaction type");

        // Rule 5: Category (asynchronous validation)
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("A category must be selected")
            .MustAsync(CategoryExists)
            .WithMessage("The selected category does not exist")
            .MustAsync(CategoryIsActive)
            .WithMessage("The selected category is inactive");

        // Rule 6: Date
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("The date is required")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("The date cannot be in the future")
            .GreaterThan(new DateTime(2000, 1, 1))
            .WithMessage("The date cannot be earlier than the year 2000");

        // Rule 7: Notes (optional)
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Rule 8: Location (optional)
        RuleFor(x => x.Location)
            .MaximumLength(100)
            .WithMessage("The location cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }

    // Custom validations
    private static bool BeValidDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return false;

        // Do not allow only numbers or special characters
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

    // Asynchronous validations with the database
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