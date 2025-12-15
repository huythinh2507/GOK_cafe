using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.LoyaltyPlatform;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages integration with the external Loyalty Platform to sync vouchers/coupons
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class LoyaltyPlatformController : ControllerBase
{
    private readonly ILoyaltyPlatformService _loyaltyPlatformService;

    public LoyaltyPlatformController(ILoyaltyPlatformService loyaltyPlatformService)
    {
        _loyaltyPlatformService = loyaltyPlatformService;
    }

    /// <summary>
    /// Fetch vouchers from Loyalty Platform (preview only, doesn't sync to database)
    /// </summary>
    /// <returns>List of vouchers from Loyalty Platform</returns>
    [HttpGet("vouchers/fetch")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<List<LoyaltyVoucherDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchVouchers()
    {
        var result = await _loyaltyPlatformService.FetchVouchersFromLoyaltyPlatformAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Fetch vouchers for a specific user from Loyalty Platform (preview only)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of user vouchers from Loyalty Platform</returns>
    [HttpGet("vouchers/user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<List<LoyaltyVoucherDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchUserVouchers(string userId)
    {
        var result = await _loyaltyPlatformService.FetchUserVouchersAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Synchronize all vouchers from Loyalty Platform to GOK Cafe's coupon system
    /// </summary>
    /// <returns>Sync result with statistics</returns>
    [HttpPost("vouchers/sync")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<LoyaltySyncResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncVouchers()
    {
        var result = await _loyaltyPlatformService.SyncVouchersFromLoyaltyPlatformAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Synchronize vouchers for a specific user from Loyalty Platform
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Sync result with statistics</returns>
    [HttpPost("vouchers/sync/user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<LoyaltySyncResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncUserVouchers(Guid userId)
    {
        var result = await _loyaltyPlatformService.SyncUserVouchersAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
