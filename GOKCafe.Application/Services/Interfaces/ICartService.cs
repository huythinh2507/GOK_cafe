using GOKCafe.Application.DTOs.Cart;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(Guid? userId, string? sessionId);
    Task<ApiResponse<CartDto>> AddToCartAsync(Guid? userId, string? sessionId, AddToCartDto dto);
    Task<ApiResponse<CartDto>> UpdateCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId, UpdateCartItemDto dto);
    Task<ApiResponse<bool>> RemoveCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId);
    Task<ApiResponse<bool>> ClearCartAsync(Guid? userId, string? sessionId);
    Task<ApiResponse<int>> GetCartItemCountAsync(Guid? userId, string? sessionId);
}
