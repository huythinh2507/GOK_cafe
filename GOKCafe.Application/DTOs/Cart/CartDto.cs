using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Cart;

public class AddToCartDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; } = 1;

    public string? SelectedSize { get; set; }
    public string? SelectedGrind { get; set; }
}

public class UpdateCartItemDto
{
    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}

public class CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();

    // Pricing breakdown
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    // Coupon information
    public string? AppliedCouponCode { get; set; }
    public Guid? AppliedCouponId { get; set; }

    // Legacy field for backward compatibility
    public decimal TotalAmount => Total;
    public int TotalItems { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string ProductSlug { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Subtotal { get; set; } // Unit price * quantity
    public decimal TotalPrice => Subtotal; // Backward compatibility
    public int StockQuantity { get; set; } // Available stock

    // Product options
    public string? SelectedSize { get; set; }
    public string? SelectedGrind { get; set; }
}

public class CheckoutDto
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string CustomerPhone { get; set; } = string.Empty;

    public string? ShippingAddress { get; set; }

    public string? Notes { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty; // Cash, CreditCard, DebitCard, OnlinePayment

    public decimal ShippingFee { get; set; } = 0;
}
