using GOKCafe.Application.DTOs.Cart;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private const int CartExpirationDays = 30;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CartDto>> GetCartAsync(Guid? userId, string? sessionId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var cartDto = await MapToCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResult(cartDto, "Cart retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CartDto>.FailureResult($"Error retrieving cart: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CartDto>> AddToCartAsync(Guid? userId, string? sessionId, AddToCartDto dto)
    {
        try
        {
            // Validate product exists and is active
            var product = await _unitOfWork.Products
                .GetQueryable()
                .Where(p => p.Id == dto.ProductId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return ApiResponse<CartDto>.FailureResult("Product not found");
            }

            // Check stock availability
            if (product.StockQuantity < dto.Quantity)
            {
                return ApiResponse<CartDto>.FailureResult($"Insufficient stock. Only {product.StockQuantity} items available");
            }

            var cart = await GetOrCreateCartAsync(userId, sessionId);

            // Check if product already exists in cart
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingCartItem != null)
            {
                // Update quantity
                var newQuantity = existingCartItem.Quantity + dto.Quantity;

                if (product.StockQuantity < newQuantity)
                {
                    return ApiResponse<CartDto>.FailureResult($"Cannot add {dto.Quantity} more items. Maximum available: {product.StockQuantity - existingCartItem.Quantity}");
                }

                existingCartItem.Quantity = newQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new cart item
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // Reload cart with updated items
            cart = await GetCartWithItemsAsync(cart.Id);
            var cartDto = await MapToCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResult(cartDto, "Product added to cart successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CartDto>.FailureResult($"Error adding to cart: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId, UpdateCartItemDto dto)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return ApiResponse<CartDto>.FailureResult("Cart item not found");
            }

            // Validate product stock
            var product = await _unitOfWork.Products
                .GetQueryable()
                .Where(p => p.Id == cartItem.ProductId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return ApiResponse<CartDto>.FailureResult("Product not found");
            }

            if (product.StockQuantity < dto.Quantity)
            {
                return ApiResponse<CartDto>.FailureResult($"Insufficient stock. Only {product.StockQuantity} items available");
            }

            cartItem.Quantity = dto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            // Reload cart with updated items
            cart = await GetCartWithItemsAsync(cart.Id);
            var cartDto = await MapToCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResult(cartDto, "Cart item updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CartDto>.FailureResult($"Error updating cart item: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RemoveCartItemAsync(Guid? userId, string? sessionId, Guid cartItemId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                return ApiResponse<bool>.FailureResult("Cart item not found");
            }

            cartItem.IsDeleted = true;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Cart item removed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult($"Error removing cart item: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(Guid? userId, string? sessionId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            foreach (var item in cart.CartItems)
            {
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Cart cleared successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult($"Error clearing cart: {ex.Message}");
        }
    }

    public async Task<ApiResponse<int>> GetCartItemCountAsync(Guid? userId, string? sessionId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var count = cart.CartItems.Sum(ci => ci.Quantity);

            return ApiResponse<int>.SuccessResult(count, "Cart item count retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.FailureResult($"Error getting cart item count: {ex.Message}");
        }
    }

    public async Task<ApiResponse<OrderDto>> CheckoutFromCartAsync(Guid? userId, string? sessionId, CheckoutDto dto)
    {
        try
        {
            // Get cart with items
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            if (!cart.CartItems.Any())
            {
                return ApiResponse<OrderDto>.FailureResult("Cart is empty. Cannot proceed with checkout.");
            }

            // Start a transaction for stock reservation and order creation
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Validate stock availability and calculate totals
                decimal subtotal = 0;
                var orderItems = new List<CreateOrderItemDto>();
                var errors = new List<string>();

                foreach (var cartItem in cart.CartItems)
                {
                    var product = await _unitOfWork.Products
                        .GetQueryable()
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                    if (product == null)
                    {
                        errors.Add($"Product {cartItem.ProductId} not found");
                        continue;
                    }

                    // Calculate available stock (total stock - reserved stock)
                    var availableStock = product.StockQuantity - product.ReservedQuantity;

                    if (availableStock < cartItem.Quantity)
                    {
                        errors.Add($"{product.Name}: Only {availableStock} items available (you requested {cartItem.Quantity})");
                        continue;
                    }

                    // Reserve stock for this order
                    product.ReservedQuantity += cartItem.Quantity;
                    _unitOfWork.Products.Update(product);

                    // Calculate price
                    var price = cartItem.DiscountPrice ?? cartItem.UnitPrice;
                    subtotal += price * cartItem.Quantity;

                    orderItems.Add(new CreateOrderItemDto
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity
                    });
                }

                if (errors.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<OrderDto>.FailureResult(
                        "Stock validation failed",
                        errors);
                }

                // Calculate tax (10% as example - you can make this configurable)
                var tax = subtotal * 0.10m;
                var totalAmount = subtotal + tax + dto.ShippingFee;

                // Create order
                var createOrderDto = new CreateOrderDto
                {
                    CustomerName = dto.CustomerName,
                    CustomerEmail = dto.CustomerEmail,
                    CustomerPhone = dto.CustomerPhone,
                    ShippingAddress = dto.ShippingAddress,
                    Notes = dto.Notes,
                    PaymentMethod = dto.PaymentMethod,
                    Items = orderItems
                };

                // Use OrderService to create the order (pass userId to link order to user)
                var orderService = new OrderService(_unitOfWork);
                var orderResult = await orderService.CreateOrderAsync(createOrderDto, userId);

                if (!orderResult.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<OrderDto>.FailureResult(
                        "Failed to create order",
                        orderResult.Errors);
                }

                // Clear cart after successful checkout
                foreach (var item in cart.CartItems)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<OrderDto>.SuccessResult(
                    orderResult.Data!,
                    $"Checkout successful! Order #{orderResult.Data!.OrderNumber} has been created.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Transaction failed: {ex.Message}", ex);
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderDto>.FailureResult(
                "An error occurred during checkout",
                new List<string> { ex.Message });
        }
    }

    #region Private Helper Methods

    private async Task<Cart> GetOrCreateCartAsync(Guid? userId, string? sessionId)
    {
        Cart? cart = null;

        if (userId.HasValue)
        {
            // Try to find cart by userId
            cart = await _unitOfWork.Carts
                .GetQueryable()
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Include(c => c.CartItems.Where(ci => !ci.IsDeleted))
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync();
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            // Try to find cart by sessionId
            cart = await _unitOfWork.Carts
                .GetQueryable()
                .Where(c => c.SessionId == sessionId && !c.IsDeleted)
                .Include(c => c.CartItems.Where(ci => !ci.IsDeleted))
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync();
        }

        // Create new cart if not found
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                ExpiresAt = DateTime.UtcNow.AddDays(CartExpirationDays),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            cart = await GetCartWithItemsAsync(cart.Id);
        }

        return cart;
    }

    private async Task<Cart> GetCartWithItemsAsync(Guid cartId)
    {
        return await _unitOfWork.Carts
            .GetQueryable()
            .Where(c => c.Id == cartId)
            .Include(c => c.CartItems.Where(ci => !ci.IsDeleted))
            .ThenInclude(ci => ci.Product)
            .FirstAsync();
    }

    private Task<CartDto> MapToCartDtoAsync(Cart cart)
    {
        var cartDto = new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = new List<CartItemDto>()
        };

        foreach (var item in cart.CartItems.Where(ci => !ci.IsDeleted))
        {
            var product = item.Product;

            cartDto.Items.Add(new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl,
                ProductSlug = product.Slug,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountPrice = item.DiscountPrice,
                TotalPrice = item.TotalPrice,
                StockQuantity = product.StockQuantity
            });
        }

        cartDto.TotalAmount = cartDto.Items.Sum(i => i.TotalPrice);
        cartDto.TotalItems = cartDto.Items.Sum(i => i.Quantity);

        return Task.FromResult(cartDto);
    }

    #endregion
}
