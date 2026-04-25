namespace RetailSales.Application.DTOs;

public record PaymentResult(bool IsSuccess, string? Reference, string? FailureReason);

public record PayRequest(decimal Amount, string Method, int SaleId);

public record RefundRequest(decimal Amount, string Reference);
