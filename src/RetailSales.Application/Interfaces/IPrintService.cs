namespace RetailSales.Application.Interfaces;

public interface IPrintService
{
    Task ProcessPrintJobAsync(int jobId);
}
