using Microsoft.EntityFrameworkCore;
using RetailSales.Domain.Entities;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Items.AnyAsync()) return;

        // 1. Seed Items
        var items = new List<Item>
        {
            new Item
            {
                Sku = "ELEC-IPH15-BLK-256",
                Name = "iPhone 15 Pro",
                Brand = "Apple",
                Category = "Electronics",
                Color = "Black",
                Size = "256GB",
                Price = 999.00m,
                CurrentStock = 0
            },
            new Item
            {
                Sku = "CLOT-TSHIRT-WHT-LRG",
                Name = "Classic White T-Shirt",
                Brand = "BasicWear",
                Category = "Clothing",
                Color = "White",
                Size = "Large",
                Price = 19.99m,
                CurrentStock = 0
            },
            new Item
            {
                Sku = "ELEC-LAPTOP-SLV-512",
                Name = "MacBook Air M2",
                Brand = "Apple",
                Category = "Electronics",
                Color = "Silver",
                Size = "512GB",
                Price = 1199.00m,
                CurrentStock = 0
            }
        };

        context.Items.AddRange(items);
        await context.SaveChangesAsync();

        // 2. Seed Item Units (Physical Inventory)
        foreach (var item in items)
        {
            int quantity = item.Category == "Electronics" ? 3 : 10;
            item.CurrentStock = quantity;

            for (int i = 1; i <= quantity; i++)
            {
                var unit = new ItemUnit
                {
                    ItemId = item.Id,
                    SerialNumber = $"{item.Sku.Split('-')[1]}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Barcode = $"{item.Sku.Replace("-", "")}{i:D3}",
                    Status = ItemUnitStatus.InStock,
                    CurrentStoreId = 1 // Main Store
                };
                context.ItemUnits.Add(unit);
            }
        }

        await context.SaveChangesAsync();
        
        // 3. Seed initial stock ledgers
        foreach (var item in items)
        {
            var units = await context.ItemUnits.Where(u => u.ItemId == item.Id).ToListAsync();
            foreach (var unit in units)
            {
                context.StockLedgers.Add(new StockLedger
                {
                    ItemId = item.Id,
                    ItemUnitId = unit.Id,
                    StoreId = 1,
                    TransactionType = StockTransactionType.PurchaseIn,
                    Quantity = 1,
                    ReferenceType = StockReferenceType.Purchase,
                    ReferenceId = "INITIAL-SEED",
                    BalanceAfter = item.CurrentStock,
                    Remarks = "Initial Seed Stock"
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
