namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a discount coupon that can be applied to orders
/// </summary>
public class Coupon : BaseEntity
{
    /// <summary>
    /// Unique coupon code (e.g., "SUMMER2024", "WELCOME10")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name/description of the coupon
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what the coupon offers
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of coupon: OneTime (used once) or Gradual (gradually deducted)
    /// </summary>
    public CouponType Type { get; set; }

    /// <summary>
    /// Discount type: Percentage or FixedAmount
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// Discount value (percentage: 10 = 10%, fixed amount: actual VND value)
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Maximum discount amount (for percentage coupons)
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Minimum order amount required to use this coupon
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Remaining balance for gradual coupons (null for one-time coupons)
    /// </summary>
    public decimal? RemainingBalance { get; set; }

    /// <summary>
    /// Whether this is a system-wide general coupon or personal coupon
    /// </summary>
    public bool IsSystemCoupon { get; set; }

    /// <summary>
    /// User ID if this is a personal coupon (null for system coupons)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Whether the coupon is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Start date when coupon becomes valid
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date when coupon expires
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used (null = unlimited)
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Current usage count
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Whether this coupon has been used (for one-time coupons)
    /// </summary>
    public bool IsUsed { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

/// <summary>
/// Type of coupon usage
/// </summary>
public enum CouponType
{
    /// <summary>
    /// Can only be used once for the full discount amount
    /// </summary>
    OneTime = 1,

    /// <summary>
    /// Can be used multiple times until balance is depleted
    /// </summary>
    Gradual = 2
}

/// <summary>
/// Type of discount
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// Percentage discount (e.g., 10%)
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Fixed amount discount (e.g., 50,000 VND)
    /// </summary>
    FixedAmount = 2
}
