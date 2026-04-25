namespace RetailSales.Application.DTOs;

public record CreateSaleRequest(int? StoreId, string PaymentMethod, List<SaleItemRequest> Items);

public record SaleItemRequest(string SerialNumber);

public record ReturnSaleRequest(string Reason, List<ReturnItemRequest> Items);

public record ReturnItemRequest(string SerialNumber);
