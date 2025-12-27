using GOKCafe.Domain.Entities;

namespace GOKCafe.Application.DTOs.Coupon;

public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public string TypeDisplay => Type == CouponType.OneTime ? "One-Time Use" : "Gradual Use";
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public bool IsSystemCoupon { get; set; }
    public Guid? UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
    public bool IsUsed { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool CanBeUsed => IsActive && !IsExpired && !IsUsed && (MaxUsageCount == null || UsageCount < MaxUsageCount);
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? InitialBalance { get; set; } // For gradual coupons
    public bool IsSystemCoupon { get; set; }
    public Guid? UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public string? ImageUrl { get; set; }
}

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}

public class ApplyCouponResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CouponDto? Coupon { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public string NoticeMessage { get; set; } = string.Empty; // Notice about which type of coupon will be used
}

public class ValidateCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public Guid? UserId { get; set; }
}

public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public CouponDto? Coupon { get; set; }
    public decimal EstimatedDiscount { get; set; }
}
