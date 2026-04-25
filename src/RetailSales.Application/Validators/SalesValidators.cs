using FluentValidation;
using RetailSales.Application.DTOs;

namespace RetailSales.Application.Validators;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).SetValidator(new SaleItemRequestValidator());
    }
}

public class SaleItemRequestValidator : AbstractValidator<SaleItemRequest>
{
    public SaleItemRequestValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required.")
            .MaximumLength(100).WithMessage("Serial number cannot exceed 100 characters.");
    }
}

public class ReturnSaleRequestValidator : AbstractValidator<ReturnSaleRequest>
{
    public ReturnSaleRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Return reason is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item to return is required.");

        RuleForEach(x => x.Items).SetValidator(new ReturnItemRequestValidator());
    }
}

public class ReturnItemRequestValidator : AbstractValidator<ReturnItemRequest>
{
    public ReturnItemRequestValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required.");
    }
}
