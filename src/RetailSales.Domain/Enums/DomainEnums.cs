namespace RetailSales.Domain.Enums;

public enum StockTransactionType
{
    PurchaseIn = 1,
    SaleOut = 2,
    ReturnIn = 3,
    TransferOut = 4,
    TransferIn = 5,
    DamageOut = 6,
    AdjustmentIn = 7,
    AdjustmentOut = 8
}

public enum StockReferenceType
{
    Adjustment = 1,
    Sale = 2,
    Return = 3,
    Purchase = 4,
    Transfer = 5
}

public enum ItemUnitStatus
{
    InStock = 1,
    Sold = 2,
    Returned = 3,
    Damaged = 4,
    Lost = 5,
    Reserved = 6,
    Transferred = 7
}

public enum PaymentStatus
{
    Pending = 1,
    Success = 2,
    Failed = 3,
    Refunded = 4
}

public enum SaleStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3,
    Returned = 4,
    PartiallyReturned = 5,
    PendingPaymentRetry = 6,
    StockUnavailable = 7
}

public enum PrintStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public enum PrintType
{
    InvoicePdf = 1,
    ThermalReceipt = 2
}
