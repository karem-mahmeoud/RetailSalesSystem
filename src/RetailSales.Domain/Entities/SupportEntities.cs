using System.ComponentModel.DataAnnotations;
using RetailSales.Domain.Enums;

namespace RetailSales.Domain.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }
    
    public int SaleId { get; set; }
    
    [MaxLength(100)]
    public string? PaymentReference { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Method { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    
    public PaymentStatus Status { get; set; }
    
    public int AttemptCount { get; set; }
    
    public DateTime? LastAttemptAt { get; set; }
    
    public string? FailureReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;
}

public class Invoice
{
    public int Id { get; set; }
    
    public int SaleId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ContentType { get; set; } = "application/pdf";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Sale Sale { get; set; } = null!;
}

public class PrintJob
{
    public int Id { get; set; }
    
    public int SaleId { get; set; }
    
    public int InvoiceId { get; set; }
    
    public PrintType PrintType { get; set; }
    
    public string? Content { get; set; } // For thermal receipt text
    
    public PrintStatus Status { get; set; } = PrintStatus.Pending;
    
    public int AttemptCount { get; set; }
    
    public DateTime? LastAttemptAt { get; set; }
    
    public string? FailureReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}
