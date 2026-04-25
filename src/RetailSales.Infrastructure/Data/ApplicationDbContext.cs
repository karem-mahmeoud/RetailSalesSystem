using Microsoft.EntityFrameworkCore;
using RetailSales.Domain.Entities;

namespace RetailSales.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemUnit> ItemUnits => Set<ItemUnit>();
    public DbSet<StockLedger> StockLedgers => Set<StockLedger>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.Price).HasPrecision(18, 4);
        });

        modelBuilder.Entity<ItemUnit>(entity =>
        {
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
        });

        modelBuilder.Entity<StockLedger>(entity =>
        {
            entity.HasOne(d => d.Item)
                .WithMany(p => p.StockLedgers)
                .HasForeignKey(d => d.ItemId);

            entity.HasOne(d => d.ItemUnit)
                .WithMany(p => p.StockLedgers)
                .HasForeignKey(d => d.ItemUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.SaleNumber).IsUnique();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.LineTotal).HasPrecision(18, 4);
            
            entity.HasOne(d => d.ItemUnit)
                .WithMany()
                .HasForeignKey(d => d.ItemUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleReturn>(entity =>
        {
            entity.HasIndex(e => e.ReturnNumber).IsUnique();
            entity.Property(e => e.TotalReturnedAmount).HasPrecision(18, 4);
            
            entity.HasOne(d => d.Sale)
                .WithMany(p => p.Returns)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleReturnItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.LineTotal).HasPrecision(18, 4);
            
            entity.HasOne(d => d.SaleReturn)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.SaleReturnId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.SaleItem)
                .WithMany()
                .HasForeignKey(d => d.SaleItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ItemUnit)
                .WithMany()
                .HasForeignKey(d => d.ItemUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasOne(d => d.Sale)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrintJob>(entity =>
        {
            entity.HasOne(d => d.Sale)
                .WithMany()
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(d => d.Invoice)
                .WithMany()
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
