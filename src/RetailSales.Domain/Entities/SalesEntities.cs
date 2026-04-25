using System.ComponentModel.DataAnnotations;
using RetailSales.Domain.Enums;

namespace RetailSales.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string SaleNumber { get; set; } = string.Empty;
    
    public int? StoreId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string StoreName { get; set; } = "MY STORE";
    
    [MaxLength(200)]
    public string? CustomerName { get; set; }
    
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    public decimal TotalAmount { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;
    
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    public SaleStatus Status { get; set; } = SaleStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
}

public class SaleItem
{
    public int Id { get; set; }
    
    public int SaleId { get; set; }
    
    public int ItemId { get; set; }
    
    public int? ItemUnitId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ItemNameSnapshot { get; set; } = string.Empty;
    
    public int Quantity { get; set; } // Usually 1 for serial tracking
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public ItemUnit? ItemUnit { get; set; }
}

public class SaleReturn
{
    public int Id { get; set; }
    
    public int SaleId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReturnNumber { get; set; } = string.Empty;
    
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public decimal TotalReturnedAmount { get; set; }
    
    public SaleStatus Status { get; set; } = SaleStatus.Returned;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}

public class SaleReturnItem
{
    public int Id { get; set; }
    
    public int SaleReturnId { get; set; }
    
    public int SaleItemId { get; set; }
    
    public int? ItemId { get; set; }
    
    public int? ItemUnitId { get; set; }
    
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }

    // Navigation properties
    public SaleReturn SaleReturn { get; set; } = null!;
    public SaleItem SaleItem { get; set; } = null!;
    public ItemUnit? ItemUnit { get; set; }
}
