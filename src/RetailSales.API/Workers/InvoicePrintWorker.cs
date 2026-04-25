using Microsoft.EntityFrameworkCore;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.API.Workers;

public class InvoicePrintWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoicePrintWorker> _logger;

    public InvoicePrintWorker(IServiceProvider serviceProvider, ILogger<InvoicePrintWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var printService = scope.ServiceProvider.GetRequiredService<IPrintService>();

                // 1. Generate missing invoices for completed sales
                var salesWithoutInvoices = await context.Sales
                    .Where(s => s.Status == SaleStatus.Completed && !context.Invoices.Any(i => i.SaleId == s.Id))
                    .ToListAsync(stoppingToken);

                foreach (var sale in salesWithoutInvoices)
                {
                    _logger.LogInformation($"Generating invoice for Sale {sale.SaleNumber}");
                    await invoiceService.GenerateInvoicePdfAsync(sale.Id);
                }

                // 2. Process pending print jobs
                var pendingPrintJobs = await context.PrintJobs
                    .Where(p => p.Status == PrintStatus.Pending)
                    .ToListAsync(stoppingToken);

                foreach (var job in pendingPrintJobs)
                {
                    _logger.LogInformation($"Processing Print Job {job.Id} for Sale {job.SaleId}");
                    await printService.ProcessPrintJobAsync(job.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InvoicePrintWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
