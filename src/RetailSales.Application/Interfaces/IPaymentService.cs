using RetailSales.Application.Common;
using RetailSales.Application.DTOs;

namespace RetailSales.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentResult>> ProcessPaymentAsync(decimal amount, string method, int saleId);
    Task<Result<PaymentResult>> ProcessRefundAsync(decimal amount, string reference);
}
