using Microsoft.AspNetCore.Mvc;
using RetailSales.Application.DTOs;
using RetailSales.Application.Interfaces;

namespace RetailSales.API.Controllers;

[Route("api")]
public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
    {
        var result = await _inventoryService.CreateItemAsync(request);
        return HandleResult(result);
    }

    [HttpPost("items/{sku}/units")]
    public async Task<IActionResult> AddUnits(string sku, [FromBody] AddUnitsRequest request)
    {
        var result = await _inventoryService.AddUnitsAsync(sku, request);
        return HandleResult(result);
    }

    [HttpGet("stock/{sku}")]
    public async Task<IActionResult> GetStockStatus(string sku)
    {
        var result = await _inventoryService.GetStockStatusAsync(sku);
        return HandleResult(result);
    }

    [HttpGet("item-units/{serialNumber}")]
    public async Task<IActionResult> GetUnit(string serialNumber)
    {
        var unit = await _inventoryService.GetUnitBySerialAsync(serialNumber);
        if (unit == null) return NotFound("Serial number not found.");
        return Ok(unit);
    }

    [HttpPut("item-units/{serialNumber}/status")]
    public async Task<IActionResult> UpdateUnitStatus(string serialNumber, [FromBody] UpdateUnitStatusRequest request)
    {
        var result = await _inventoryService.UpdateUnitStatusAsync(serialNumber, request.Status, request.Remarks);
        return HandleResult(result);
    }
}
