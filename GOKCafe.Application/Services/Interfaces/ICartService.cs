using GOKCafe.Application.DTOs.Cart;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;

namespace GOKCafe.Application.Services.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(Guid? userId, string? sessionId);
    Task<ApiResponse<CartDto>> AddToCartAsync(Guid? userId, string? sessionId, AddToCartDto dto);
    Task<ApiResponse<CartDto>> UpdateCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId, UpdateCartItemDto dto);
    Task<ApiResponse<bool>> RemoveCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId);
    Task<ApiResponse<bool>> ClearCartAsync(Guid? userId, string? sessionId);
    Task<ApiResponse<int>> GetCartItemCountAsync(Guid? userId, string? sessionId);
    Task<ApiResponse<OrderDto>> CheckoutFromCartAsync(Guid? userId, string? sessionId, CheckoutDto dto);

    // Coupon management
    Task<ApiResponse<CartDto>> ApplyCouponToCartAsync(Guid? userId, string? sessionId, string couponCode);
    Task<ApiResponse<CartDto>> RemoveCouponFromCartAsync(Guid? userId, string? sessionId);
}
