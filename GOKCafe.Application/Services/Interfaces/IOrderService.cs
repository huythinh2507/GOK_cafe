using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;

namespace GOKCafe.Application.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<PaginatedResponse<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize, string? status = null);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid id);
    Task<ApiResponse<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber);
    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto dto);
    Task<ApiResponse<bool>> UpdateOrderStatusAsync(Guid id, string status);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid id);
}
