using Microsoft.EntityFrameworkCore;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.API.Workers;

public class PaymentRetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentRetryWorker> _logger;

    public PaymentRetryWorker(IServiceProvider serviceProvider, ILogger<PaymentRetryWorker> logger)
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
                var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

                var failedPayments = await context.PaymentTransactions
                    .Where(p => p.Status == PaymentStatus.Failed && p.AttemptCount < 3)
                    .ToListAsync(stoppingToken);

                foreach (var payment in failedPayments)
                {
                    _logger.LogInformation($"Retrying payment {payment.Id} for Sale {payment.SaleId} (Attempt {payment.AttemptCount + 1})");
                    await salesService.ProcessFailedPaymentRetryAsync(payment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentRetryWorker");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
