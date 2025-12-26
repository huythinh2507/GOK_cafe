using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Coupon;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages coupon operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    /// <summary>
    /// Create a new coupon
    /// </summary>
    /// <param name="dto">The coupon creation data</param>
    /// <returns>The created coupon</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<CouponDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _couponService.CreateCouponAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get coupon by ID
    /// </summary>
    /// <param name="id">The coupon ID</param>
    /// <returns>The coupon details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<CouponDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCouponById(Guid id)
    {
        var result = await _couponService.GetCouponByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get coupon by code
    /// </summary>
    /// <param name="code">The coupon code</param>
    /// <returns>The coupon details</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType<ApiResponse<CouponDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCouponByCode(string code)
    {
        var result = await _couponService.GetCouponByCodeAsync(code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get all coupons with pagination
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">The page size (default: 10)</param>
    /// <param name="isSystemCoupon">Optional filter for system coupons</param>
    /// <param name="userId">Optional filter for user-specific coupons</param>
    /// <returns>A paginated list of coupons</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<CouponDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isSystemCoupon = null,
        [FromQuery] Guid? userId = null)
    {
        var result = await _couponService.GetCouponsAsync(pageNumber, pageSize, isSystemCoupon, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get user's personal coupons
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">The page size (default: 10)</param>
    /// <returns>A paginated list of user's coupons</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<CouponDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCoupons(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _couponService.GetCouponsAsync(pageNumber, pageSize, isSystemCoupon: false, userId: userId);
        return Ok(result);
    }

    /// <summary>
    /// Get system (general) coupons available for all users
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">The page size (default: 10)</param>
    /// <returns>A paginated list of system coupons</returns>
    [HttpGet("system")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<CouponDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemCoupons(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _couponService.GetCouponsAsync(pageNumber, pageSize, isSystemCoupon: true);
        return Ok(result);
    }

    /// <summary>
    /// Delete a coupon (soft delete)
    /// </summary>
    /// <param name="id">The coupon ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon(Guid id)
    {
        var result = await _couponService.DeleteCouponAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Validate a coupon without applying it
    /// </summary>
    /// <param name="request">The validation request</param>
    /// <returns>Validation result with estimated discount</returns>
    [HttpPost("validate")]
    [ProducesResponseType<ApiResponse<ValidateCouponResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _couponService.ValidateCouponAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Apply a coupon to an order
    /// </summary>
    /// <param name="request">The coupon application request</param>
    /// <returns>Application result with discount details and notice message</returns>
    [HttpPost("apply")]
    [ProducesResponseType<ApiResponse<ApplyCouponResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _couponService.ApplyCouponAsync(request);
        return Ok(result);
    }
}
