using Microsoft.EntityFrameworkCore;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.Infrastructure.Services;

public class PrintService : IPrintService
{
    private readonly ApplicationDbContext _context;

    public PrintService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ProcessPrintJobAsync(int jobId)
    {
        var job = await _context.PrintJobs
            .Include(j => j.Sale)
            .Include(j => j.Invoice)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null) return;

        job.Status = PrintStatus.Processing;
        job.AttemptCount++;
        job.LastAttemptAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            // Simulate printing logic
            await Task.Delay(1000); // Wait for "printer"

            Console.WriteLine($"[PRINTER] Printing {job.PrintType} for Sale {job.Sale.SaleNumber}...");
            Console.WriteLine($"[PRINTER] Document: {job.Invoice?.FilePath ?? "N/A"}");
            
            job.Status = PrintStatus.Completed;
        }
        catch (Exception ex)
        {
            job.Status = PrintStatus.Failed;
            job.FailureReason = ex.Message;
        }

        await _context.SaveChangesAsync();
    }
}
