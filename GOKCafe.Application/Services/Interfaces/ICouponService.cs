using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Coupon;

namespace GOKCafe.Application.Services.Interfaces;

public interface ICouponService
{
    Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto dto);
    Task<ApiResponse<CouponDto>> GetCouponByIdAsync(Guid id);
    Task<ApiResponse<CouponDto>> GetCouponByCodeAsync(string code);
    Task<ApiResponse<PaginatedResponse<CouponDto>>> GetCouponsAsync(int pageNumber, int pageSize, bool? isSystemCoupon = null, Guid? userId = null);
    Task<ApiResponse<bool>> DeleteCouponAsync(Guid id);
    Task<ApiResponse<ValidateCouponResponse>> ValidateCouponAsync(ValidateCouponRequest request);
    Task<ApiResponse<ApplyCouponResponse>> ApplyCouponAsync(ApplyCouponRequest request);
}
