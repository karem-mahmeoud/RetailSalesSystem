using RetailSales.Application.Common;
using RetailSales.Application.DTOs;
using RetailSales.Domain.Entities;
using RetailSales.Domain.Enums;

namespace RetailSales.Application.Interfaces;

public interface IInventoryService
{
    Task<Result<Item>> CreateItemAsync(CreateItemRequest request);
    Task<Item?> GetItemBySkuAsync(string sku);
    Task<Result<IEnumerable<ItemUnit>>> AddUnitsAsync(string sku, AddUnitsRequest request);
    Task<Result<StockStatusResponse>> GetStockStatusAsync(string sku);
    Task<ItemUnit?> GetUnitBySerialAsync(string serialNumber);
    Task<Result> UpdateUnitStatusAsync(string serialNumber, ItemUnitStatus status, string? remarks = null);
    Task RecordStockLedgerAsync(int itemId, int? unitId, int? storeId, StockTransactionType type, int quantity, StockReferenceType referenceType, string? referenceId, string? remarks = null);
}
