using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RetailSales.Application.Common;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Entities;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.Infrastructure.Services;

public class SalesService : ISalesService
{
    private readonly ApplicationDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<SalesService> _logger;

    public SalesService(ApplicationDbContext context, IInventoryService inventoryService, IPaymentService paymentService, ILogger<SalesService> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<SaleResponse>> CreateSaleAsync(CreateSaleRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        
        // 1. Validate Serials and Prepare Sale Items
        var validationResult = await ValidateAndPrepareSaleItemsAsync(request);
        if (validationResult.IsFailure)
            return Result.Failure<SaleResponse>(validationResult.Error);

        var (saleItems, totalAmount) = validationResult.Value;

        // 2. Create Sale Record
        var sale = new Sale
        {
            SaleNumber = GenerateSaleNumber(),
            StoreId = request.StoreId,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            Status = SaleStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            Items = saleItems
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // 3. Process Payment
        var result = await _paymentService.ProcessPaymentAsync(totalAmount, request.PaymentMethod, sale.Id);
        
        // Note: Even if the gateway call fails (result.IsFailure), we record the attempt
        var paymentData = result.IsSuccess 
            ? result.Value 
            : new PaymentResult(false, null, result.Error.Message);

        await RecordPaymentTransactionAsync(sale, totalAmount, request.PaymentMethod, paymentData);

        if (paymentData.IsSuccess)
        {
            await FinalizeSaleAsync(sale);
        }
        else
        {
            sale.Status = SaleStatus.PendingPaymentRetry;
            sale.PaymentStatus = PaymentStatus.Failed;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        _logger.LogInformation("Sale {SaleNumber} processed. Payment Success: {IsSuccess}", sale.SaleNumber, paymentData.IsSuccess);
        
        return MapToResponse(sale);
    }

    private async Task<Result<(List<SaleItem> items, decimal total)>> ValidateAndPrepareSaleItemsAsync(CreateSaleRequest request)
    {
        var saleItems = new List<SaleItem>();
        decimal totalAmount = 0;

        foreach (var itemReq in request.Items)
        {
            var unit = await _context.ItemUnits.Include(u => u.Item)
                .FirstOrDefaultAsync(u => u.SerialNumber == itemReq.SerialNumber);

            if (unit == null) 
                return Result.Failure<(List<SaleItem>, decimal)>(Error.NotFound("Sales.SerialNotFound", $"Serial {itemReq.SerialNumber} not found."));
            
            if (unit.Status != ItemUnitStatus.InStock) 
                return Result.Failure<(List<SaleItem>, decimal)>(Error.Failure("Sales.SerialNotAvailable", $"Serial {itemReq.SerialNumber} is not available (Status: {unit.Status})."));
            
            if (request.StoreId.HasValue && unit.CurrentStoreId != request.StoreId) 
                return Result.Failure<(List<SaleItem>, decimal)>(Error.Failure("Sales.StoreMismatch", $"Serial {itemReq.SerialNumber} is not in this store."));

            var lineTotal = unit.Item.Price;
            totalAmount += lineTotal;

            saleItems.Add(new SaleItem
            {
                ItemId = unit.Item.Id,
                ItemUnitId = unit.Id,
                Sku = unit.Item.Sku,
                SerialNumber = unit.SerialNumber,
                ItemNameSnapshot = unit.Item.Name,
                Quantity = 1,
                UnitPrice = unit.Item.Price,
                LineTotal = lineTotal
            });
        }

        return Result.Success((saleItems, totalAmount));
    }

    private string GenerateSaleNumber() => $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

    private async Task RecordPaymentTransactionAsync(Sale sale, decimal amount, string method, PaymentResult result)
    {
        var paymentTransaction = new PaymentTransaction
        {
            SaleId = sale.Id,
            Amount = amount,
            Method = method,
            Status = result.IsSuccess ? PaymentStatus.Success : PaymentStatus.Failed,
            PaymentReference = result.Reference,
            FailureReason = result.FailureReason,
            AttemptCount = 1,
            LastAttemptAt = DateTime.UtcNow
        };
        _context.PaymentTransactions.Add(paymentTransaction);
        await Task.CompletedTask;
    }

    private async Task FinalizeSaleAsync(Sale sale)
    {
        _logger.LogInformation("Finalizing sale {SaleNumber}.", sale.SaleNumber);
        sale.Status = SaleStatus.Completed;
        sale.PaymentStatus = PaymentStatus.Success;

        foreach (var sItem in sale.Items)
        {
            var unit = await _context.ItemUnits.FindAsync(sItem.ItemUnitId);
            if (unit != null)
            {
                unit.Status = ItemUnitStatus.Sold;
                
                var item = await _context.Items.FindAsync(sItem.ItemId);
                if (item != null)
                {
                    item.CurrentStock--;
                    
                    var ledger = new StockLedger
                    {
                        ItemId = item.Id,
                        ItemUnitId = unit.Id,
                        StoreId = sale.StoreId,
                        TransactionType = StockTransactionType.SaleOut,
                        Quantity = 1,
                        ReferenceType = StockReferenceType.Sale,
                        ReferenceId = sale.SaleNumber,
                        BalanceAfter = item.CurrentStock,
                        Remarks = "Sale Out"
                    };
                    _context.StockLedgers.Add(ledger);
                }
            }
        }
    }

    public async Task<SaleResponse?> GetSaleByIdAsync(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Returns)
            .Include(s => s.Invoices)
            .FirstOrDefaultAsync(s => s.Id == id);

        return sale == null ? null : MapToResponse(sale);
    }

    public async Task<Result<SaleReturnResponse>> ReturnSaleAsync(int saleId, ReturnSaleRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        var sale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null) 
            return Result.Failure<SaleReturnResponse>(Error.NotFound("Sales.NotFound", "Sale not found."));
        
        if (sale.Status != SaleStatus.Completed && sale.Status != SaleStatus.PartiallyReturned) 
            return Result.Failure<SaleReturnResponse>(Error.Failure("Sales.InvalidStatusForReturn", "Only completed sales can be returned."));

        var returnRecord = new SaleReturn
        {
            SaleId = saleId,
            ReturnNumber = $"RET-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Reason = request.Reason,
            TotalReturnedAmount = 0,
            Status = SaleStatus.Returned
        };

        foreach (var retReq in request.Items)
        {
            var saleItem = sale.Items.FirstOrDefault(i => i.SerialNumber == retReq.SerialNumber);
            if (saleItem == null) 
                return Result.Failure<SaleReturnResponse>(Error.Failure("Sales.ItemNotInSale", $"Serial {retReq.SerialNumber} not found in this sale."));
            
            var unit = await _context.ItemUnits.FindAsync(saleItem.ItemUnitId);
            if (unit == null || unit.Status != ItemUnitStatus.Sold)
                return Result.Failure<SaleReturnResponse>(Error.Failure("Sales.ItemNotSold", $"Serial {retReq.SerialNumber} cannot be returned (Status: {unit?.Status})."));

            returnRecord.TotalReturnedAmount += saleItem.LineTotal;

            returnRecord.Items.Add(new SaleReturnItem
            {
                SaleItemId = saleItem.Id,
                ItemId = saleItem.ItemId,
                ItemUnitId = saleItem.ItemUnitId,
                SerialNumber = saleItem.SerialNumber,
                UnitPrice = saleItem.UnitPrice,
                LineTotal = saleItem.LineTotal
            });

            // Restore Unit & Stock
            unit.Status = ItemUnitStatus.InStock;
            var item = await _context.Items.FindAsync(saleItem.ItemId);
            if (item != null)
            {
                item.CurrentStock++;
                
                var ledger = new StockLedger
                {
                    ItemId = item.Id,
                    ItemUnitId = unit.Id,
                    StoreId = sale.StoreId,
                    TransactionType = StockTransactionType.ReturnIn,
                    Quantity = 1,
                    ReferenceType = StockReferenceType.Return,
                    ReferenceId = returnRecord.ReturnNumber,
                    BalanceAfter = item.CurrentStock,
                    Remarks = "Return In"
                };
                _context.StockLedgers.Add(ledger);
            }
        }

        _context.SaleReturns.Add(returnRecord);
        
        // Update Sale Status
        var totalItemsCount = sale.Items.Count;
        var alreadyReturnedCount = await _context.SaleReturnItems
            .CountAsync(ri => _context.SaleReturns.Any(r => r.Id == ri.SaleReturnId && r.SaleId == saleId));
        
        var currentlyReturnedCount = returnRecord.Items.Count;
        
        sale.Status = (alreadyReturnedCount + currentlyReturnedCount) >= totalItemsCount 
            ? SaleStatus.Returned 
            : SaleStatus.PartiallyReturned;

        // Process Refund
        await _paymentService.ProcessRefundAsync(returnRecord.TotalReturnedAmount, sale.SaleNumber);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return MapToResponse(returnRecord);
    }

    public async Task<Result> ProcessFailedPaymentRetryAsync(int paymentId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        
        var paymentTx = await _context.PaymentTransactions
            .Include(t => t.Sale)
            .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(t => t.Id == paymentId);

        if (paymentTx == null) 
            return Result.Failure(Error.NotFound("Payment.NotFound", "Payment transaction not found."));
        
        if (paymentTx.Status == PaymentStatus.Success) return Result.Success();

        paymentTx.AttemptCount++;
        paymentTx.LastAttemptAt = DateTime.UtcNow;

        // CRITICAL: Re-validate serial availability
        foreach (var sItem in paymentTx.Sale.Items)
        {
            var unit = await _context.ItemUnits.FindAsync(sItem.ItemUnitId);
            if (unit == null || unit.Status != ItemUnitStatus.InStock)
            {
                paymentTx.Sale.Status = SaleStatus.StockUnavailable;
                paymentTx.FailureReason = $"Serial {sItem.SerialNumber} is no longer available.";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result.Failure(Error.Failure("Sales.StockUnavailable", $"Serial {sItem.SerialNumber} is no longer available."));
            }
        }

        var result = await _paymentService.ProcessPaymentAsync(paymentTx.Amount, paymentTx.Method, paymentTx.SaleId);
        
        var paymentData = result.IsSuccess 
            ? result.Value 
            : new PaymentResult(false, null, result.Error.Message);

        if (paymentData.IsSuccess)
        {
            paymentTx.Status = PaymentStatus.Success;
            paymentTx.PaymentReference = paymentData.Reference;
            await FinalizeSaleAsync(paymentTx.Sale);
        }
        else
        {
            paymentTx.FailureReason = paymentData.FailureReason;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return paymentData.IsSuccess 
            ? Result.Success() 
            : Result.Failure(Error.Failure("Payment.Failed", paymentData.FailureReason ?? "Payment failed."));
    }

    private SaleResponse MapToResponse(Sale sale)
    {
        return new SaleResponse(
            sale.Id,
            sale.SaleNumber,
            sale.StoreId,
            sale.TotalAmount,
            sale.PaymentMethod,
            sale.Status,
            sale.PaymentStatus,
            sale.CreatedAt,
            sale.Items.Select(i => new SaleItemResponse(
                i.Id,
                i.ItemId,
                i.Sku,
                i.SerialNumber,
                i.ItemNameSnapshot,
                i.UnitPrice,
                i.LineTotal)).ToList());
    }

    private SaleReturnResponse MapToResponse(SaleReturn ret)
    {
        return new SaleReturnResponse(
            ret.Id,
            ret.SaleId,
            ret.ReturnNumber,
            ret.Reason,
            ret.TotalReturnedAmount,
            ret.Status,
            ret.CreatedAt,
            ret.Items.Select(i => new SaleReturnItemResponse(
                i.Id,
                i.ItemId,
                i.SerialNumber,
                i.UnitPrice,
                i.LineTotal)).ToList());
    }
}
