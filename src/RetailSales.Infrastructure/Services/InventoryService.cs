using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RetailSales.Application.Common;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Entities;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Item>> CreateItemAsync(CreateItemRequest request)
    {
        var sku = GenerateSku(request);
        if (string.IsNullOrEmpty(sku))
            return Result.Failure<Item>(Error.Validation("Inventory.SkuRequired", "SKU is required."));

        if (await _context.Items.AnyAsync(i => i.Sku == sku))
            return Result.Failure<Item>(Error.Conflict("Inventory.SkuExists", $"SKU '{sku}' already exists."));

        var item = new Item
        {
            Sku = sku,
            Name = request.Name,
            Brand = request.Brand,
            Category = request.Category,
            Color = request.Color,
            Size = request.Size,
            Price = request.Price,
            CurrentStock = 0
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Item {Sku} created successfully.", item.Sku);
        return Result.Success(item);
    }

    private string GenerateSku(CreateItemRequest request)
    {
        if (!request.AutoGenerateSku)
            return request.Sku ?? string.Empty;

        if (string.IsNullOrEmpty(request.CategoryCode) || string.IsNullOrEmpty(request.ModelCode))
            return string.Empty;

        return $"{request.CategoryCode}-{request.ModelCode}-{request.Color?.ToUpper() ?? "NOS"}-{request.Size?.ToUpper() ?? "UNI"}";
    }

    public async Task<Result<IEnumerable<ItemUnit>>> AddUnitsAsync(string sku, AddUnitsRequest request)
    {
        _logger.LogInformation("Adding {Quantity} units to item {Sku}.", request.Quantity, sku);
        
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Sku == sku);
        if (item == null) 
            return Result.Failure<IEnumerable<ItemUnit>>(Error.NotFound("Inventory.ItemNotFound", "Item not found."));

        var units = new List<ItemUnit>();
        
        var lastUnit = await _context.ItemUnits.OrderByDescending(u => u.Id).FirstOrDefaultAsync();
        int startIndex = lastUnit?.Id ?? 0;

        for (int i = 1; i <= request.Quantity; i++)
        {
            var currentIndex = startIndex + i;
            var unit = new ItemUnit
            {
                ItemId = item.Id,
                SerialNumber = $"ITEM-{currentIndex:D6}",
                Barcode = request.GenerateBarcode ? $"BAR-{currentIndex:D6}" : string.Empty,
                Status = ItemUnitStatus.InStock,
                CurrentStoreId = request.StoreId
            };
            
            _context.ItemUnits.Add(unit);
            units.Add(unit);
        }

        item.CurrentStock += request.Quantity;
        await _context.SaveChangesAsync();

        foreach (var unit in units)
        {
            var ledger = new StockLedger
            {
                ItemId = item.Id,
                ItemUnitId = unit.Id,
                StoreId = request.StoreId,
                TransactionType = StockTransactionType.PurchaseIn,
                Quantity = 1,
                ReferenceType = StockReferenceType.Purchase,
                BalanceAfter = item.CurrentStock,
                Remarks = "Purchase In"
            };
            _context.StockLedgers.Add(ledger);
        }
        
        await _context.SaveChangesAsync();

        return Result.Success<IEnumerable<ItemUnit>>(units);
    }

    public async Task<Result<StockStatusResponse>> GetStockStatusAsync(string sku)
    {
        var item = await _context.Items
            .Include(i => i.Units)
            .FirstOrDefaultAsync(i => i.Sku == sku);

        if (item == null) 
            return Result.Failure<StockStatusResponse>(Error.NotFound("Inventory.ItemNotFound", "Item not found."));

        var serials = item.Units
            .Select(u => new UnitStatusDto(u.SerialNumber, u.Barcode, u.Status))
            .ToList();

        return Result.Success(new StockStatusResponse(item.Sku, item.Name, item.CurrentStock, serials));
    }

    public async Task<ItemUnit?> GetUnitBySerialAsync(string serialNumber)
    {
        return await _context.ItemUnits.Include(u => u.Item).FirstOrDefaultAsync(u => u.SerialNumber == serialNumber);
    }

    public async Task<Result> UpdateUnitStatusAsync(string serialNumber, ItemUnitStatus status, string? remarks = null)
    {
        var unit = await _context.ItemUnits.Include(u => u.Item).FirstOrDefaultAsync(u => u.SerialNumber == serialNumber);
        if (unit == null) 
            return Result.Failure(Error.NotFound("Inventory.SerialNotFound", "Serial number not found."));

        unit.Status = status;
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task RecordStockLedgerAsync(int itemId, int? unitId, int? storeId, StockTransactionType type, int quantity, StockReferenceType referenceType, string? referenceId, string? remarks = null)
    {
        var item = await _context.Items.FindAsync(itemId);
        if (item == null) return;

        var ledger = new StockLedger
        {
            ItemId = itemId,
            ItemUnitId = unitId,
            StoreId = storeId,
            TransactionType = type,
            Quantity = quantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            BalanceAfter = item.CurrentStock,
            Remarks = remarks
        };

        _context.StockLedgers.Add(ledger);
        await _context.SaveChangesAsync();
    }

    public async Task<Item?> GetItemBySkuAsync(string sku)
    {
        return await _context.Items.FirstOrDefaultAsync(i => i.Sku == sku);
    }
}
