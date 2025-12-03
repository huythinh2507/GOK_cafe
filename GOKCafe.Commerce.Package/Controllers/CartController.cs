using GOKCafe.Application.DTOs.Cart;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GOKCafe.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's cart or session cart
    /// </summary>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpGet]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCart([FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<CartDto>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.GetCartAsync(userId, sessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Add product to cart
    /// </summary>
    /// <param name="dto">Product details to add</param>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpPost("items")]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto, [FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<CartDto>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.AddToCartAsync(userId, sessionId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    /// <param name="cartItemId">Cart item ID</param>
    /// <param name="dto">Updated quantity</param>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpPut("items/{cartItemId}")]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CartDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto, [FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<CartDto>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.UpdateCartItemAsync(userId, sessionId, cartItemId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="cartItemId">Cart item ID to remove</param>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpDelete("items/{cartItemId}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveCartItem(Guid cartItemId, [FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<bool>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.RemoveCartItemAsync(userId, sessionId, cartItemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpDelete]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearCart([FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<bool>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.ClearCartAsync(userId, sessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get total item count in cart
    /// </summary>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpGet("count")]
    [ProducesResponseType<ApiResponse<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<int>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCartItemCount([FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<int>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.GetCartItemCountAsync(userId, sessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Checkout from cart - converts cart items to order with stock reservation
    /// </summary>
    /// <param name="dto">Checkout information (customer details, shipping, payment)</param>
    /// <param name="sessionId">Session ID for anonymous users (optional if authenticated)</param>
    [HttpPost("checkout")]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutDto dto, [FromQuery] string? sessionId = null)
    {
        var userId = GetUserId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(ApiResponse<OrderDto>.FailureResult("Either authentication or sessionId is required"));
        }

        var result = await _cartService.CheckoutFromCartAsync(userId, sessionId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #region Private Helpers

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}
