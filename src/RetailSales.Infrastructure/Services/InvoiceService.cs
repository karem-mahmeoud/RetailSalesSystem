using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RetailSales.Application.Interfaces;
using RetailSales.Domain.Entities;
using RetailSales.Domain.Enums;
using RetailSales.Infrastructure.Data;

namespace RetailSales.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateInvoicePdfAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null) throw new Exception("Sale not found.");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(sale.StoreName).FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Invoice: {sale.SaleNumber}");
                        col.Item().Text($"Date: {sale.SaleDate:dd-MM-yyyy HH:mm}");
                    });
                });

                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Spacing(5);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item");
                            header.Cell().Element(CellStyle).Text("Serial");
                            header.Cell().Element(CellStyle).AlignRight().Text("Price");
                            header.Cell().Element(CellStyle).AlignRight().Text("Total");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        foreach (var item in sale.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.ItemNameSnapshot);
                            table.Cell().Element(CellStyle).Text(item.SerialNumber ?? "-");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N3}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.LineTotal:N3}");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        }
                    });

                    x.Item().AlignRight().Text($"Total: {sale.TotalAmount:N3}").FontSize(14).SemiBold();
                    x.Item().PaddingTop(10).Text($"Payment Method: {sale.PaymentMethod}");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Thank you for your business!");
                });
            });
        });

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{sale.SaleNumber}.pdf");
        document.GeneratePdf(filePath);

        // Save invoice record
        var invoice = new Invoice
        {
            SaleId = sale.Id,
            InvoiceNumber = sale.SaleNumber,
            InvoiceDate = DateTime.UtcNow,
            FilePath = filePath
        };

        _context.Invoices.Add(invoice);

        // Create Print Job
        var printJob = new PrintJob
        {
            SaleId = sale.Id,
            Invoice = invoice,
            PrintType = PrintType.InvoicePdf,
            Status = PrintStatus.Pending
        };
        _context.PrintJobs.Add(printJob);

        await _context.SaveChangesAsync();

        return filePath;
    }
}
