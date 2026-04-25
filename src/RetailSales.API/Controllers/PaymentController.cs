using Microsoft.AspNetCore.Mvc;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;

namespace RetailSales.API.Controllers;

[Route("api/payment")]
public class PaymentController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PayRequest request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request.Amount, request.Method, request.SaleId);
        return HandleResult(result);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        var result = await _paymentService.ProcessRefundAsync(request.Amount, request.Reference);
        return HandleResult(result);
    }
}
