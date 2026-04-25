using RetailSales.Domain.Enums;

namespace RetailSales.Application.DTOs;

public record SaleResponse(
    int Id,
    string SaleNumber,
    int? StoreId,
    decimal TotalAmount,
    string PaymentMethod,
    SaleStatus Status,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt,
    List<SaleItemResponse> Items);

public record SaleItemResponse(
    int Id,
    int ItemId,
    string Sku,
    string? SerialNumber,
    string ItemNameSnapshot,
    decimal UnitPrice,
    decimal LineTotal);

public record SaleReturnResponse(
    int Id,
    int SaleId,
    string ReturnNumber,
    string? Reason,
    decimal TotalReturnedAmount,
    SaleStatus Status,
    DateTime CreatedAt,
    List<SaleReturnItemResponse> Items);

public record SaleReturnItemResponse(
    int Id,
    int? ItemId,
    string? SerialNumber,
    decimal UnitPrice,
    decimal LineTotal);
