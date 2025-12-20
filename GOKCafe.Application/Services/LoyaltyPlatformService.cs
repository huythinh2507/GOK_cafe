using System.Text;
using System.Text.Json;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.LoyaltyPlatform;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GOKCafe.Application.Services;

/// <summary>
/// Service for integrating with the external loyalty platform to fetch and sync vouchers/coupons
/// </summary>
public class LoyaltyPlatformService : ILoyaltyPlatformService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoyaltyPlatformService> _logger;
    private readonly string _loyaltyPlatformUrl;
    private readonly string? _apiKey;

    public LoyaltyPlatformService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<LoyaltyPlatformService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;

        _loyaltyPlatformUrl = _configuration["LoyaltyPlatform:Url"]
            ?? throw new InvalidOperationException("Loyalty Platform URL not configured");
        _apiKey = _configuration["LoyaltyPlatform:ApiKey"];
    }

    /// <summary>
    /// Fetches all active vouchers from the loyalty platform
    /// </summary>
    public async Task<ApiResponse<List<LoyaltyVoucherDto>>> FetchVouchersFromLoyaltyPlatformAsync()
    {
        try
        {
            _logger.LogInformation("Fetching vouchers from Loyalty Platform...");

            var client = _httpClientFactory.CreateClient();
            var url = $"{_loyaltyPlatformUrl}/api/vouchers";

            // Add API key if configured
            if (!string.IsNullOrEmpty(_apiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            _logger.LogInformation("Fetching from: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Loyalty Platform fetch failed. Status: {Status}, Response: {Response}",
                    response.StatusCode, errorContent);
                return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                    $"Failed to fetch vouchers from Loyalty Platform: {response.StatusCode}",
                    new List<string> { errorContent });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Loyalty Platform response: {Response}", responseBody);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Try to parse as LoyaltyVouchersResponse first (if the API returns a wrapped response)
            try
            {
                var voucherResponse = JsonSerializer.Deserialize<LoyaltyVouchersResponse>(responseBody, options);
                if (voucherResponse?.Success == true && voucherResponse.Vouchers != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} vouchers from Loyalty Platform",
                        voucherResponse.Vouchers.Count);
                    return ApiResponse<List<LoyaltyVoucherDto>>.SuccessResult(voucherResponse.Vouchers);
                }
            }
            catch
            {
                // If that fails, try to parse as a direct array
                var vouchers = JsonSerializer.Deserialize<List<LoyaltyVoucherDto>>(responseBody, options);
                if (vouchers != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} vouchers from Loyalty Platform",
                        vouchers.Count);
                    return ApiResponse<List<LoyaltyVoucherDto>>.SuccessResult(vouchers);
                }
            }

            return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                "Failed to parse response from Loyalty Platform",
                new List<string> { "Invalid response format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vouchers from Loyalty Platform");
            return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                "An error occurred while fetching vouchers from Loyalty Platform",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Fetches vouchers for a specific user from the loyalty platform
    /// </summary>
    public async Task<ApiResponse<List<LoyaltyVoucherDto>>> FetchUserVouchersAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching vouchers for user {UserId} from Loyalty Platform...", userId);

            var client = _httpClientFactory.CreateClient();
            var url = $"{_loyaltyPlatformUrl}/api/vouchers/user/{userId}";

            // Add API key if configured
            if (!string.IsNullOrEmpty(_apiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            _logger.LogInformation("Fetching from: {Url}", url);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Loyalty Platform fetch failed. Status: {Status}, Response: {Response}",
                    response.StatusCode, errorContent);
                return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                    $"Failed to fetch user vouchers from Loyalty Platform: {response.StatusCode}",
                    new List<string> { errorContent });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Loyalty Platform response: {Response}", responseBody);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Try to parse as LoyaltyVouchersResponse first
            try
            {
                var voucherResponse = JsonSerializer.Deserialize<LoyaltyVouchersResponse>(responseBody, options);
                if (voucherResponse?.Success == true && voucherResponse.Vouchers != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} vouchers for user {UserId}",
                        voucherResponse.Vouchers.Count, userId);
                    return ApiResponse<List<LoyaltyVoucherDto>>.SuccessResult(voucherResponse.Vouchers);
                }
            }
            catch
            {
                // If that fails, try to parse as a direct array
                var vouchers = JsonSerializer.Deserialize<List<LoyaltyVoucherDto>>(responseBody, options);
                if (vouchers != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} vouchers for user {UserId}",
                        vouchers.Count, userId);
                    return ApiResponse<List<LoyaltyVoucherDto>>.SuccessResult(vouchers);
                }
            }

            return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                "Failed to parse response from Loyalty Platform",
                new List<string> { "Invalid response format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user vouchers from Loyalty Platform");
            return ApiResponse<List<LoyaltyVoucherDto>>.FailureResult(
                "An error occurred while fetching user vouchers from Loyalty Platform",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Synchronizes vouchers from loyalty platform to GOK Cafe's coupon system
    /// </summary>
    public async Task<ApiResponse<LoyaltySyncResultDto>> SyncVouchersFromLoyaltyPlatformAsync()
    {
        try
        {
            _logger.LogInformation("Starting Loyalty Platform voucher synchronization...");

            var result = new LoyaltySyncResultDto();

            // Fetch vouchers from Loyalty Platform
            var vouchersResponse = await FetchVouchersFromLoyaltyPlatformAsync();
            if (!vouchersResponse.Success || vouchersResponse.Data == null)
            {
                return ApiResponse<LoyaltySyncResultDto>.FailureResult(
                    "Failed to fetch vouchers from Loyalty Platform",
                    vouchersResponse.Errors);
            }

            result.TotalFetched = vouchersResponse.Data.Count;

            if (vouchersResponse.Data.Count == 0)
            {
                _logger.LogInformation("No vouchers to sync from Loyalty Platform");
                return ApiResponse<LoyaltySyncResultDto>.SuccessResult(result, "No vouchers to sync");
            }

            // Fetch all existing coupons by code for quick lookup
            var allCoupons = await _unitOfWork.Coupons.GetAllAsync();
            var existingCoupons = allCoupons.ToDictionary(c => c.Code, c => c);

            _logger.LogInformation("Found {ExistingCount} existing coupons in system", existingCoupons.Count);

            foreach (var voucher in vouchersResponse.Data)
            {
                try
                {
                    if (existingCoupons.TryGetValue(voucher.Code, out var existingCoupon))
                    {
                        // Update existing coupon
                        UpdateCouponFromVoucher(existingCoupon, voucher);
                        _unitOfWork.Coupons.Update(existingCoupon);
                        result.Updated++;
                        _logger.LogDebug("Updated coupon: {Code}", voucher.Code);
                    }
                    else
                    {
                        // Create new coupon
                        var newCoupon = CreateCouponFromVoucher(voucher);
                        await _unitOfWork.Coupons.AddAsync(newCoupon);
                        result.Created++;
                        _logger.LogDebug("Created new coupon: {Code}", voucher.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing voucher: {Code}", voucher.Code);
                    result.Errors.Add($"Error syncing {voucher.Code}: {ex.Message}");
                    result.Skipped++;
                }
            }

            // Save all changes
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Loyalty Platform sync completed. Created: {Created}, Updated: {Updated}, Skipped: {Skipped}",
                result.Created, result.Updated, result.Skipped);

            return ApiResponse<LoyaltySyncResultDto>.SuccessResult(
                result,
                "Vouchers synchronized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing vouchers from Loyalty Platform");
            return ApiResponse<LoyaltySyncResultDto>.FailureResult(
                "An error occurred while syncing vouchers from Loyalty Platform",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Fetches and syncs vouchers for a specific user
    /// </summary>
    public async Task<ApiResponse<LoyaltySyncResultDto>> SyncUserVouchersAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting user {UserId} voucher synchronization from Loyalty Platform...", userId);

            var result = new LoyaltySyncResultDto();

            // Fetch user vouchers from Loyalty Platform
            var vouchersResponse = await FetchUserVouchersAsync(userId.ToString());
            if (!vouchersResponse.Success || vouchersResponse.Data == null)
            {
                return ApiResponse<LoyaltySyncResultDto>.FailureResult(
                    "Failed to fetch user vouchers from Loyalty Platform",
                    vouchersResponse.Errors);
            }

            result.TotalFetched = vouchersResponse.Data.Count;

            if (vouchersResponse.Data.Count == 0)
            {
                _logger.LogInformation("No vouchers to sync for user {UserId}", userId);
                return ApiResponse<LoyaltySyncResultDto>.SuccessResult(result, "No vouchers to sync");
            }

            // Fetch existing coupons for this user
            var allCoupons = await _unitOfWork.Coupons.GetAllAsync();
            var existingCoupons = allCoupons
                .Where(c => c.UserId == userId)
                .ToDictionary(c => c.Code, c => c);

            _logger.LogInformation("Found {ExistingCount} existing coupons for user {UserId}",
                existingCoupons.Count, userId);

            foreach (var voucher in vouchersResponse.Data)
            {
                try
                {
                    if (existingCoupons.TryGetValue(voucher.Code, out var existingCoupon))
                    {
                        // Update existing coupon
                        UpdateCouponFromVoucher(existingCoupon, voucher);
                        _unitOfWork.Coupons.Update(existingCoupon);
                        result.Updated++;
                        _logger.LogDebug("Updated user coupon: {Code}", voucher.Code);
                    }
                    else
                    {
                        // Create new coupon for user
                        var newCoupon = CreateCouponFromVoucher(voucher);
                        newCoupon.UserId = userId;
                        newCoupon.IsSystemCoupon = false;
                        await _unitOfWork.Coupons.AddAsync(newCoupon);
                        result.Created++;
                        _logger.LogDebug("Created new user coupon: {Code}", voucher.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing user voucher: {Code}", voucher.Code);
                    result.Errors.Add($"Error syncing {voucher.Code}: {ex.Message}");
                    result.Skipped++;
                }
            }

            // Save all changes
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} voucher sync completed. Created: {Created}, Updated: {Updated}, Skipped: {Skipped}",
                userId, result.Created, result.Updated, result.Skipped);

            return ApiResponse<LoyaltySyncResultDto>.SuccessResult(
                result,
                "User vouchers synchronized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user vouchers from Loyalty Platform");
            return ApiResponse<LoyaltySyncResultDto>.FailureResult(
                "An error occurred while syncing user vouchers from Loyalty Platform",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new Coupon entity from a LoyaltyVoucherDto
    /// </summary>
    private Coupon CreateCouponFromVoucher(LoyaltyVoucherDto voucher)
    {
        // Map voucher type to coupon type
        var couponType = voucher.Type.ToLower() switch
        {
            "onetime" or "one-time" or "single" => CouponType.OneTime,
            "gradual" or "recurring" or "multiple" => CouponType.Gradual,
            _ => CouponType.OneTime
        };

        // Map discount type
        var discountType = voucher.Type.ToLower() switch
        {
            "percentage" or "percent" => DiscountType.Percentage,
            "fixed" or "fixedamount" or "fixed-amount" => DiscountType.FixedAmount,
            _ => DiscountType.Percentage
        };

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            Type = couponType,
            DiscountType = discountType,
            DiscountValue = voucher.Value,
            MaxDiscountAmount = voucher.MaxDiscount,
            MinOrderAmount = voucher.MinOrderAmount,
            RemainingBalance = voucher.RemainingBalance,
            IsSystemCoupon = voucher.IsSystemWide,
            UserId = !string.IsNullOrEmpty(voucher.TargetUserId) && Guid.TryParse(voucher.TargetUserId, out var userId)
                ? userId
                : null,
            IsActive = voucher.IsActive,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            MaxUsageCount = voucher.MaxUsageCount,
            UsageCount = voucher.UsageCount,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return coupon;
    }

    /// <summary>
    /// Updates an existing Coupon entity from a LoyaltyVoucherDto
    /// </summary>
    private void UpdateCouponFromVoucher(Coupon coupon, LoyaltyVoucherDto voucher)
    {
        coupon.Name = voucher.Name;
        coupon.Description = voucher.Description;

        // Map voucher type to coupon type
        coupon.Type = voucher.Type.ToLower() switch
        {
            "onetime" or "one-time" or "single" => CouponType.OneTime,
            "gradual" or "recurring" or "multiple" => CouponType.Gradual,
            _ => CouponType.OneTime
        };

        // Map discount type
        coupon.DiscountType = voucher.Type.ToLower() switch
        {
            "percentage" or "percent" => DiscountType.Percentage,
            "fixed" or "fixedamount" or "fixed-amount" => DiscountType.FixedAmount,
            _ => DiscountType.Percentage
        };

        coupon.DiscountValue = voucher.Value;
        coupon.MaxDiscountAmount = voucher.MaxDiscount;
        coupon.MinOrderAmount = voucher.MinOrderAmount;
        coupon.RemainingBalance = voucher.RemainingBalance;
        coupon.IsActive = voucher.IsActive;
        coupon.StartDate = voucher.StartDate;
        coupon.EndDate = voucher.EndDate;
        coupon.MaxUsageCount = voucher.MaxUsageCount;
        coupon.UsageCount = voucher.UsageCount;
        coupon.UpdatedAt = DateTime.UtcNow;
    }
}
