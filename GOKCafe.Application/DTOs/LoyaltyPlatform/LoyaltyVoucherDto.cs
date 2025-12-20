namespace GOKCafe.Application.DTOs.LoyaltyPlatform;

/// <summary>
/// DTO representing a voucher/coupon from the loyalty platform
/// </summary>
public class LoyaltyVoucherDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., "percentage", "fixed", "oneTime", "gradual"
    public decimal Value { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
    public string? TargetUserId { get; set; } // If voucher is for specific user
    public bool IsSystemWide { get; set; } // If available to all users
}

/// <summary>
/// Response from loyalty platform when fetching vouchers
/// </summary>
public class LoyaltyVouchersResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<LoyaltyVoucherDto> Vouchers { get; set; } = new();
}

/// <summary>
/// Result of synchronizing vouchers from loyalty platform
/// </summary>
public class LoyaltySyncResultDto
{
    public int TotalFetched { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
