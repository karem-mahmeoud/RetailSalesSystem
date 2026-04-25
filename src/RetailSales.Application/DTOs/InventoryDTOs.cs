using RetailSales.Domain.Enums;

namespace RetailSales.Application.DTOs;

public record CreateItemRequest(
    string? Sku, 
    string Name, 
    string? Brand, 
    string? Category, 
    string? Color, 
    string? Size, 
    decimal Price,
    string? CategoryCode = null,
    string? ModelCode = null,
    bool AutoGenerateSku = false);

public record AddUnitsRequest(int? StoreId, int Quantity, bool GenerateBarcode);

public record StockStatusResponse(string Sku, string Name, int CurrentStock, List<UnitStatusDto> AvailableSerials);

public record UnitStatusDto(string SerialNumber, string Barcode, ItemUnitStatus Status);

public record UpdateUnitStatusRequest(ItemUnitStatus Status, string? Remarks);
