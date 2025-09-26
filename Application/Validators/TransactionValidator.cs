using FluentValidation;
using MoneyTracker.Application.DTOs;

namespace MoneyTracker.Application.Validators;

public class TransactionValidator : AbstractValidator<TransactionDto>
{
    public TransactionValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("The description is required")
            .Length(3, 200).WithMessage("The description must be between 3 and 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("The amount must be greater than zero");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A category must be selected");
    }
}