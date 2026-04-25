using System.ComponentModel.DataAnnotations;
using RetailSales.Domain.Enums;

namespace RetailSales.Domain.Entities;

public class Item
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Brand { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; }
    
    [MaxLength(50)]
    public string? Size { get; set; }
    
    public decimal Price { get; set; }
    
    public int CurrentStock { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<StockLedger> StockLedgers { get; set; } = new List<StockLedger>();
    public ICollection<ItemUnit> Units { get; set; } = new List<ItemUnit>();
}

public class ItemUnit
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Barcode { get; set; } = string.Empty;
    
    public ItemUnitStatus Status { get; set; } = ItemUnitStatus.InStock;
    
    public int? CurrentStoreId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Item Item { get; set; } = null!;
    public ICollection<StockLedger> StockLedgers { get; set; } = new List<StockLedger>();
}

public class StockLedger
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    
    public int? ItemUnitId { get; set; }
    
    public int? StoreId { get; set; }
    
    public StockTransactionType TransactionType { get; set; }
    
    public int Quantity { get; set; }
    
    public StockReferenceType ReferenceType { get; set; }
    
    public string? ReferenceId { get; set; }
    
    public int BalanceAfter { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Item Item { get; set; } = null!;
    public ItemUnit? ItemUnit { get; set; }
}
