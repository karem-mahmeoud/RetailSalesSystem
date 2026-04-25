using Microsoft.AspNetCore.Mvc;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;

namespace RetailSales.API.Controllers;

[Route("api/sales")]
public class SalesController : ApiControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        var result = await _salesService.CreateSaleAsync(request);
        return HandleResult(result);
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ReturnSale(int id, [FromBody] ReturnSaleRequest request)
    {
        var result = await _salesService.ReturnSaleAsync(id, request);
        return HandleResult(result);
    }

    [HttpPost("payments/{paymentId}/retry")]
    public async Task<IActionResult> RetryPayment(int paymentId)
    {
        var result = await _salesService.ProcessFailedPaymentRetryAsync(paymentId);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSale(int id)
    {
        var sale = await _salesService.GetSaleByIdAsync(id);
        if (sale == null) return NotFound();
        return Ok(sale);
    }
}
