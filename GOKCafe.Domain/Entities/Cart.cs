namespace GOKCafe.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; } // For anonymous users
    public DateTime? ExpiresAt { get; set; }

    // Coupon information
    public Guid? AppliedCouponId { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal ShippingFee { get; set; } = 0;

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Coupon? AppliedCoupon { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // Computed properties
    public decimal Subtotal => CartItems.Sum(item => item.TotalPrice);
    public decimal Total => Subtotal + ShippingFee - DiscountAmount;
    public decimal TotalAmount => Total; // Backward compatibility
    public int TotalItems => CartItems.Sum(item => item.Quantity);
}
