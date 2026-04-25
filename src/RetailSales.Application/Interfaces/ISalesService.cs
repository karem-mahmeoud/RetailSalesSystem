using RetailSales.Application.Common;
using RetailSales.Application.DTOs;
using RetailSales.Domain.Entities;

namespace RetailSales.Application.Interfaces;

public interface ISalesService
{
    Task<Result<SaleResponse>> CreateSaleAsync(CreateSaleRequest request);
    Task<SaleResponse?> GetSaleByIdAsync(int id);
    Task<Result<SaleReturnResponse>> ReturnSaleAsync(int saleId, ReturnSaleRequest request);
    Task<Result> ProcessFailedPaymentRetryAsync(int paymentId);
}
