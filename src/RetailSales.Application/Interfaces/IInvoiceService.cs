namespace RetailSales.Application.Interfaces;

public interface IInvoiceService
{
    Task<string> GenerateInvoicePdfAsync(int saleId);
}
