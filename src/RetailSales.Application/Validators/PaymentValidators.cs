using FluentValidation;
using RetailSales.Application.DTOs;

namespace RetailSales.Application.Validators;

public class PayRequestValidator : AbstractValidator<PayRequest>
{
    public PayRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters.");

        RuleFor(x => x.SaleId)
            .GreaterThan(0).WithMessage("Valid Sale ID is required.");
    }
}
