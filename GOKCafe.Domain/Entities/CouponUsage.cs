namespace GOKCafe.Domain.Entities;

/// <summary>
/// Tracks each usage of a coupon
/// </summary>
public class CouponUsage : BaseEntity
{
    /// <summary>
    /// Coupon that was used
    /// </summary>
    public Guid CouponId { get; set; }

    /// <summary>
    /// Order where coupon was applied
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// User who used the coupon (null for guest users)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Session ID for guest users
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Original order amount before discount
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Amount discounted
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final amount after discount
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Remaining balance after this usage (for gradual coupons)
    /// </summary>
    public decimal? RemainingBalance { get; set; }

    /// <summary>
    /// Date when coupon was used
    /// </summary>
    public DateTime UsedAt { get; set; }

    // Navigation properties
    public Coupon Coupon { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User? User { get; set; }
}
