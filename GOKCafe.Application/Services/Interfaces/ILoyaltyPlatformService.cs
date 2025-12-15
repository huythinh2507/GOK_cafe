using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.LoyaltyPlatform;

namespace GOKCafe.Application.Services.Interfaces;

/// <summary>
/// Service for integrating with the external loyalty platform to fetch and sync vouchers/coupons
/// </summary>
public interface ILoyaltyPlatformService
{
    /// <summary>
    /// Fetches all active vouchers from the loyalty platform
    /// </summary>
    Task<ApiResponse<List<LoyaltyVoucherDto>>> FetchVouchersFromLoyaltyPlatformAsync();

    /// <summary>
    /// Fetches vouchers for a specific user from the loyalty platform
    /// </summary>
    Task<ApiResponse<List<LoyaltyVoucherDto>>> FetchUserVouchersAsync(string userId);

    /// <summary>
    /// Synchronizes vouchers from loyalty platform to GOK Cafe's coupon system
    /// </summary>
    Task<ApiResponse<LoyaltySyncResultDto>> SyncVouchersFromLoyaltyPlatformAsync();

    /// <summary>
    /// Fetches and syncs vouchers for a specific user
    /// </summary>
    Task<ApiResponse<LoyaltySyncResultDto>> SyncUserVouchersAsync(Guid userId);
}
