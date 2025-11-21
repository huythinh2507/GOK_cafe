using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get all orders with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var result = await _orderService.GetOrdersAsync(pageNumber, pageSize, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
    {
        var result = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CreateOrderAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetOrderById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
