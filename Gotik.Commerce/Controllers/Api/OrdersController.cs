using Microsoft.AspNetCore.Http;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gotik.Commerce.Controllers.Api;

/// <summary>
/// Manages order operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Orders API")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get all orders with pagination and filters (Admin only)
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="status">Optional filter for order status</param>
    /// <returns>A paginated list of orders matching the filters</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var result = await _orderService.GetOrdersAsync(pageNumber, pageSize, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get authenticated user's order history
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="status">Optional filter for order status (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled)</param>
    /// <returns>A paginated list of the user's orders</returns>
    [Authorize]
    [HttpGet("my-orders")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<OrderDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<OrderDto>>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<OrderDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<PaginatedResponse<OrderDto>>.FailureResult("User authentication required"));
        }

        var result = await _orderService.GetUserOrdersAsync(userId.Value, pageNumber, pageSize, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <returns>The order with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">The unique order number</param>
    /// <returns>The order with the specified order number</returns>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
    {
        var result = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="dto">The order creation data</param>
    /// <returns>The newly created order</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CreateOrderAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetOrderById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <param name="status">The new status value</param>
    /// <returns>The updated order</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">The unique identifier of the order to cancel</param>
    /// <returns>The cancelled order</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}
