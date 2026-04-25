using RetailSales.Application.Common;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;

namespace RetailSales.Infrastructure.Services;

public class MockPaymentService : IPaymentService
{
    private readonly Random _random = new();

    public async Task<Result<PaymentResult>> ProcessPaymentAsync(decimal amount, string method, int saleId)
    {
        // Simulate network delay
        await Task.Delay(500);

        // Simulate NBK logic: Success 80% of the time, Fail 20%
        bool isSuccess = _random.Next(1, 101) <= 80;

        if (isSuccess)
        {
            return Result.Success(new PaymentResult(true, $"NBK-{Guid.NewGuid().ToString()[..8].ToUpper()}", null));
        }
        else
        {
            // Note: Even if payment "fails" at the gateway, we return Success with a failed PaymentResult
            // unless there is a system-level error (like gateway timeout).
            // For robustness, we'll treat "Insufficient funds" as a successful call to the gateway but a failed payment.
            return Result.Success(new PaymentResult(false, null, "Insufficient funds or NBK Gateway timeout."));
        }
    }

    public async Task<Result<PaymentResult>> ProcessRefundAsync(decimal amount, string reference)
    {
        await Task.Delay(300);
        return Result.Success(new PaymentResult(true, $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}", null));
    }
}
