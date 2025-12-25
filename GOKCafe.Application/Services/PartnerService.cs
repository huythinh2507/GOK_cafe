using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Partner;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class PartnerService : IPartnerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string PartnerCacheKeyPrefix = "partner:";
    private const string PartnersCacheKeyPrefix = "partners:";
    private const string ActivePartnersCacheKey = "partners:active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public PartnerService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<PartnerDto>>> GetPartnersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool? isActive = null)
    {
        try
        {
            var cacheKey = $"{PartnersCacheKeyPrefix}page:{pageNumber}:size:{pageSize}:active:{isActive}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<PartnerDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.Partners.GetQueryable();

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var totalItems = await query.CountAsync();
            var partners = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var partnerDtos = partners.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<PartnerDto>>.SuccessResult(
                new PaginatedResponse<PartnerDto>
                {
                    Items = partnerDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<PartnerDto>>.FailureResult(
                $"Error retrieving partners: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PartnerDto>>> GetActivePartnersAsync()
    {
        try
        {
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<List<PartnerDto>>>(ActivePartnersCacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var partners = await _unitOfWork.Partners.GetQueryable()
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();

            var partnerDtos = partners.Select(MapToDto).ToList();
            var response = ApiResponse<List<PartnerDto>>.SuccessResult(partnerDtos);

            await _cacheService.SetAsync(ActivePartnersCacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PartnerDto>>.FailureResult(
                $"Error retrieving active partners: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PartnerDto>> GetPartnerByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{PartnerCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PartnerDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                return ApiResponse<PartnerDto>.FailureResult("Partner not found");

            var partnerDto = MapToDto(partner);
            var response = ApiResponse<PartnerDto>.SuccessResult(partnerDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PartnerDto>.FailureResult(
                $"Error retrieving partner: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PartnerDto>> CreatePartnerAsync(CreatePartnerDto dto)
    {
        try
        {
            var partner = new Partner
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Partners.AddAsync(partner);
            await _unitOfWork.SaveChangesAsync();

            await InvalidatePartnerCache(partner.Id);

            var partnerDto = MapToDto(partner);
            return ApiResponse<PartnerDto>.SuccessResult(partnerDto, "Partner created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PartnerDto>.FailureResult(
                $"Error creating partner: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PartnerDto>> UpdatePartnerAsync(Guid id, UpdatePartnerDto dto)
    {
        try
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                return ApiResponse<PartnerDto>.FailureResult("Partner not found");

            partner.Name = dto.Name;
            partner.LogoUrl = dto.LogoUrl;
            partner.WebsiteUrl = dto.WebsiteUrl;
            partner.DisplayOrder = dto.DisplayOrder;
            partner.IsActive = dto.IsActive;
            partner.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Partners.Update(partner);
            await _unitOfWork.SaveChangesAsync();

            await InvalidatePartnerCache(partner.Id);

            var partnerDto = MapToDto(partner);
            return ApiResponse<PartnerDto>.SuccessResult(partnerDto, "Partner updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PartnerDto>.FailureResult(
                $"Error updating partner: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeletePartnerAsync(Guid id)
    {
        try
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                return ApiResponse<bool>.FailureResult("Partner not found");

            _unitOfWork.Partners.SoftDelete(partner);
            await _unitOfWork.SaveChangesAsync();

            await InvalidatePartnerCache(partner.Id);

            return ApiResponse<bool>.SuccessResult(true, "Partner deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error deleting partner: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PartnerDto>> ToggleActiveStatusAsync(Guid id)
    {
        try
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                return ApiResponse<PartnerDto>.FailureResult("Partner not found");

            partner.IsActive = !partner.IsActive;
            partner.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Partners.Update(partner);
            await _unitOfWork.SaveChangesAsync();

            await InvalidatePartnerCache(partner.Id);

            var partnerDto = MapToDto(partner);
            return ApiResponse<PartnerDto>.SuccessResult(
                partnerDto,
                $"Partner {(partner.IsActive ? "activated" : "deactivated")} successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<PartnerDto>.FailureResult(
                $"Error toggling partner status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ReorderPartnersAsync(List<ReorderPartnerDto> reorderDtos)
    {
        try
        {
            if (reorderDtos == null || !reorderDtos.Any())
                return ApiResponse<bool>.FailureResult("No partners to reorder");

            foreach (var dto in reorderDtos)
            {
                var partner = await _unitOfWork.Partners.GetByIdAsync(dto.Id);
                if (partner == null)
                    continue;

                partner.DisplayOrder = dto.DisplayOrder;
                partner.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Partners.Update(partner);
            }

            await _unitOfWork.SaveChangesAsync();

            // Clear all partner caches
            await _cacheService.RemoveAsync(ActivePartnersCacheKey);
            var allKeys = new[] { PartnersCacheKeyPrefix, PartnerCacheKeyPrefix };
            foreach (var keyPrefix in allKeys)
            {
                await _cacheService.RemoveAsync(keyPrefix);
            }

            return ApiResponse<bool>.SuccessResult(true, "Partners reordered successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error reordering partners: {ex.Message}");
        }
    }

    private PartnerDto MapToDto(Partner partner)
    {
        return new PartnerDto
        {
            Id = partner.Id,
            Name = partner.Name,
            LogoUrl = partner.LogoUrl,
            WebsiteUrl = partner.WebsiteUrl,
            DisplayOrder = partner.DisplayOrder,
            IsActive = partner.IsActive
        };
    }

    private async Task InvalidatePartnerCache(Guid partnerId)
    {
        await _cacheService.RemoveAsync($"{PartnerCacheKeyPrefix}{partnerId}");
        await _cacheService.RemoveAsync(ActivePartnersCacheKey);

        var allKeys = new[] { PartnersCacheKeyPrefix };
        foreach (var keyPrefix in allKeys)
        {
            await _cacheService.RemoveAsync(keyPrefix);
        }
    }
}
