using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Coupon;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;

    public CouponService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto dto)
    {
        try
        {
            // Check if code already exists
            var existingCoupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == dto.Code);
            if (existingCoupon != null)
            {
                return ApiResponse<CouponDto>.FailureResult("Coupon code already exists", new List<string> { "CODE_EXISTS" });
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                return ApiResponse<CouponDto>.FailureResult("End date must be after start date", new List<string> { "INVALID_DATES" });
            }

            // Validate discount value
            if (dto.DiscountType == DiscountType.Percentage && dto.DiscountValue > 100)
            {
                return ApiResponse<CouponDto>.FailureResult("Percentage discount cannot exceed 100%", new List<string> { "INVALID_DISCOUNT" });
            }

            // For gradual coupons, initial balance is required
            if (dto.Type == CouponType.Gradual && !dto.InitialBalance.HasValue)
            {
                return ApiResponse<CouponDto>.FailureResult("Initial balance is required for gradual coupons", new List<string> { "MISSING_BALANCE" });
            }

            var coupon = new Coupon
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                MinOrderAmount = dto.MinOrderAmount,
                RemainingBalance = dto.Type == CouponType.Gradual ? dto.InitialBalance : null,
                IsSystemCoupon = dto.IsSystemCoupon,
                UserId = dto.UserId,
                IsActive = true,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MaxUsageCount = dto.MaxUsageCount,
                UsageCount = 0,
                IsUsed = false,
                ImageUrl = dto.ImageUrl
            };

            await _unitOfWork.Coupons.AddAsync(coupon);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon));
        }
        catch (Exception ex)
        {
            return ApiResponse<CouponDto>.FailureResult("Error creating coupon", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CouponDto>> GetCouponByIdAsync(Guid id)
    {
        try
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
            {
                return ApiResponse<CouponDto>.FailureResult("Coupon not found", new List<string> { "NOT_FOUND" });
            }

            return ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon));
        }
        catch (Exception ex)
        {
            return ApiResponse<CouponDto>.FailureResult("Error getting coupon", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CouponDto>> GetCouponByCodeAsync(string code)
    {
        try
        {
            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon == null)
            {
                return ApiResponse<CouponDto>.FailureResult("Coupon not found", new List<string> { "NOT_FOUND" });
            }

            return ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon));
        }
        catch (Exception ex)
        {
            return ApiResponse<CouponDto>.FailureResult("Error getting coupon", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaginatedResponse<CouponDto>>> GetCouponsAsync(int pageNumber, int pageSize, bool? isSystemCoupon = null, Guid? userId = null)
    {
        try
        {
            var query = _unitOfWork.Coupons.GetQueryable();

            if (isSystemCoupon.HasValue)
            {
                query = query.Where(c => c.IsSystemCoupon == isSystemCoupon.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            var totalItems = await query.CountAsync();
            var coupons = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var couponDtos = coupons.Select(MapToDto).ToList();

            var response = new PaginatedResponse<CouponDto>
            {
                Items = couponDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return ApiResponse<PaginatedResponse<CouponDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<CouponDto>>.FailureResult("Error getting coupons", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteCouponAsync(Guid id)
    {
        try
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
            {
                return ApiResponse<bool>.FailureResult("Coupon not found", new List<string> { "NOT_FOUND" });
            }

            _unitOfWork.Coupons.SoftDelete(coupon);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult("Error deleting coupon", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ValidateCouponResponse>> ValidateCouponAsync(ValidateCouponRequest request)
    {
        try
        {
            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode);
            if (coupon == null)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Coupon not found",
                    EstimatedDiscount = 0
                });
            }

            // Check if coupon is active
            if (!coupon.IsActive)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Coupon is not active",
                    EstimatedDiscount = 0
                });
            }

            // Check if coupon is expired
            if (DateTime.UtcNow > coupon.EndDate || DateTime.UtcNow < coupon.StartDate)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Coupon is expired or not yet valid",
                    EstimatedDiscount = 0
                });
            }

            // Check if coupon is already used (for one-time coupons)
            if (coupon.IsUsed)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Coupon has already been used",
                    EstimatedDiscount = 0
                });
            }

            // Check max usage count
            if (coupon.MaxUsageCount.HasValue && coupon.UsageCount >= coupon.MaxUsageCount.Value)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Coupon has reached maximum usage limit",
                    EstimatedDiscount = 0
                });
            }

            // Check user eligibility
            if (!coupon.IsSystemCoupon && request.UserId.HasValue && coupon.UserId != request.UserId.Value)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "This coupon is not available for your account",
                    EstimatedDiscount = 0
                });
            }

            // Check if user is logged in for personal coupons
            if (!coupon.IsSystemCoupon && !request.UserId.HasValue)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = "Please log in to use this coupon",
                    EstimatedDiscount = 0
                });
            }

            // Check minimum order amount
            if (coupon.MinOrderAmount.HasValue && request.OrderAmount < coupon.MinOrderAmount.Value)
            {
                return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
                {
                    IsValid = false,
                    Message = $"Minimum order amount of {coupon.MinOrderAmount.Value:C} required",
                    EstimatedDiscount = 0
                });
            }

            // Calculate estimated discount
            var estimatedDiscount = CalculateDiscount(coupon, request.OrderAmount);

            return ApiResponse<ValidateCouponResponse>.SuccessResult(new ValidateCouponResponse
            {
                IsValid = true,
                Message = "Coupon is valid",
                Coupon = MapToDto(coupon),
                EstimatedDiscount = estimatedDiscount
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<ValidateCouponResponse>.FailureResult("Error validating coupon", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ApplyCouponResponse>> ApplyCouponAsync(ApplyCouponRequest request)
    {
        try
        {
            // First validate the coupon
            var validationRequest = new ValidateCouponRequest
            {
                CouponCode = request.CouponCode,
                OrderAmount = request.OrderAmount,
                UserId = request.UserId
            };

            var validationResponse = await ValidateCouponAsync(validationRequest);
            if (!validationResponse.Success || validationResponse.Data == null || !validationResponse.Data.IsValid)
            {
                return ApiResponse<ApplyCouponResponse>.SuccessResult(new ApplyCouponResponse
                {
                    Success = false,
                    Message = validationResponse.Data?.Message ?? "Coupon validation failed",
                    OriginalAmount = request.OrderAmount,
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                });
            }

            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode);
            if (coupon == null)
            {
                return ApiResponse<ApplyCouponResponse>.FailureResult("Coupon not found", new List<string> { "NOT_FOUND" });
            }

            // Calculate discount
            var discountAmount = CalculateDiscount(coupon, request.OrderAmount);
            var finalAmount = request.OrderAmount - discountAmount;

            // Generate notice message
            var noticeMessage = coupon.Type == CouponType.OneTime
                ? "This is a one-time use coupon and will be marked as used after this order."
                : $"This is a gradual use coupon. Remaining balance after this order: {(coupon.RemainingBalance!.Value - discountAmount):C}";

            // Update coupon based on type
            if (coupon.Type == CouponType.OneTime)
            {
                coupon.IsUsed = true;
            }
            else // Gradual
            {
                coupon.RemainingBalance -= discountAmount;
                if (coupon.RemainingBalance <= 0)
                {
                    coupon.IsUsed = true;
                    coupon.RemainingBalance = 0;
                }
            }

            coupon.UsageCount++;
            _unitOfWork.Coupons.Update(coupon);

            // Create usage record
            var couponUsage = new CouponUsage
            {
                CouponId = coupon.Id,
                OrderId = Guid.Empty, // This should be updated when order is created
                UserId = request.UserId,
                SessionId = request.SessionId,
                OriginalAmount = request.OrderAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                RemainingBalance = coupon.RemainingBalance,
                UsedAt = DateTime.UtcNow
            };

            await _unitOfWork.CouponUsages.AddAsync(couponUsage);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ApplyCouponResponse>.SuccessResult(new ApplyCouponResponse
            {
                Success = true,
                Message = "Coupon applied successfully",
                Coupon = MapToDto(coupon),
                OriginalAmount = request.OrderAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                RemainingBalance = coupon.RemainingBalance,
                NoticeMessage = noticeMessage
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<ApplyCouponResponse>.FailureResult("Error applying coupon", new List<string> { ex.Message });
        }
    }

    private decimal CalculateDiscount(Coupon coupon, decimal orderAmount)
    {
        decimal discount = 0;

        if (coupon.DiscountType == DiscountType.Percentage)
        {
            discount = orderAmount * (coupon.DiscountValue / 100);
        }
        else // FixedAmount
        {
            discount = coupon.DiscountValue;
        }

        // For gradual coupons, discount cannot exceed remaining balance
        if (coupon.Type == CouponType.Gradual && coupon.RemainingBalance.HasValue)
        {
            discount = Math.Min(discount, coupon.RemainingBalance.Value);
        }

        // Apply max discount amount if specified
        if (coupon.MaxDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
        }

        // Discount cannot exceed order amount
        discount = Math.Min(discount, orderAmount);

        return discount;
    }

    private CouponDto MapToDto(Coupon coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Name = coupon.Name,
            Description = coupon.Description,
            Type = coupon.Type,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            MinOrderAmount = coupon.MinOrderAmount,
            RemainingBalance = coupon.RemainingBalance,
            IsSystemCoupon = coupon.IsSystemCoupon,
            UserId = coupon.UserId,
            IsActive = coupon.IsActive,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MaxUsageCount = coupon.MaxUsageCount,
            UsageCount = coupon.UsageCount,
            IsUsed = coupon.IsUsed,
            ImageUrl = coupon.ImageUrl
        };
    }
}
