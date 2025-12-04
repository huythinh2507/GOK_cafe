using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;

namespace GOKCafe.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaginatedResponse<OrderDto>>> GetOrdersAsync(
        int pageNumber, int pageSize, string? status = null)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var query = orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => MapToDto(o))
                .ToList();

            var response = new PaginatedResponse<OrderDto>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PaginatedResponse<OrderDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<OrderDto>>.FailureResult(
                "An error occurred while retrieving orders",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaginatedResponse<OrderDto>>> GetUserOrdersAsync(
        Guid userId, int pageNumber, int pageSize, string? status = null)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var query = orders.AsQueryable().Where(o => o.UserId == userId);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => MapToDto(o))
                .ToList();

            var response = new PaginatedResponse<OrderDto>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PaginatedResponse<OrderDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<OrderDto>>.FailureResult(
                "An error occurred while retrieving user orders",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return ApiResponse<OrderDto>.FailureResult("Order not found");

            var orderDto = MapToDto(order);
            return ApiResponse<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderDto>.FailureResult(
                "An error occurred while retrieving the order",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber)
    {
        try
        {
            var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order == null)
                return ApiResponse<OrderDto>.FailureResult("Order not found");

            var orderDto = MapToDto(order);
            return ApiResponse<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderDto>.FailureResult(
                "An error occurred while retrieving the order",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto dto, Guid? userId = null)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var orderNumber = GenerateOrderNumber();
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<OrderDto>.FailureResult($"Product with ID {item.ProductId} not found");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<OrderDto>.FailureResult($"Insufficient stock for product {product.Name}");
                }

                var unitPrice = product.DiscountPrice ?? product.Price;
                var totalPrice = unitPrice * item.Quantity;
                subTotal += totalPrice;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = totalPrice
                });

                product.StockQuantity -= item.Quantity;
                _unitOfWork.Products.Update(product);
            }

            var tax = subTotal * 0.1m;
            var shippingFee = 5.00m;
            var totalAmount = subTotal + tax + shippingFee;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId, // Link order to authenticated user
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                ShippingAddress = dto.ShippingAddress,
                Notes = dto.Notes,
                SubTotal = subTotal,
                Tax = tax,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod, true),
                PaymentStatus = PaymentStatus.Pending
            };

            await _unitOfWork.Orders.AddAsync(order);

            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
            }
            await _unitOfWork.OrderItems.AddRangeAsync(orderItems);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            order.OrderItems = orderItems;
            var orderDto = MapToDto(order);
            return ApiResponse<OrderDto>.SuccessResult(orderDto, "Order created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<OrderDto>.FailureResult(
                "An error occurred while creating the order",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(Guid id, string status)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return ApiResponse<bool>.FailureResult("Order not found");

            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                return ApiResponse<bool>.FailureResult("Invalid order status");

            order.Status = orderStatus;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while updating order status",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.FailureResult("Order not found");
            }

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.FailureResult("Cannot cancel this order");
            }

            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _unitOfWork.Products.Update(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Refunded;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<bool>.SuccessResult(true, "Order cancelled successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<bool>.FailureResult(
                "An error occurred while cancelling the order",
                new List<string> { ex.Message });
        }
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList() ?? new List<OrderItemDto>()
        };
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
