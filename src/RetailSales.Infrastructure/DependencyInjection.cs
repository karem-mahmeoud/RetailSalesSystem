using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailSales.Application.Interfaces;
using RetailSales.Infrastructure.Services;
using RetailSales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RetailSales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IPaymentService, MockPaymentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPrintService, PrintService>();

        return services;
    }
}
