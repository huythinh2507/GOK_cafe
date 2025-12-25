using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Partner;

namespace GOKCafe.Application.Services.Interfaces;

public interface IPartnerService
{
    Task<ApiResponse<PaginatedResponse<PartnerDto>>> GetPartnersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool? isActive = null);

    Task<ApiResponse<List<PartnerDto>>> GetActivePartnersAsync();
    Task<ApiResponse<PartnerDto>> GetPartnerByIdAsync(Guid id);
    Task<ApiResponse<PartnerDto>> CreatePartnerAsync(CreatePartnerDto dto);
    Task<ApiResponse<PartnerDto>> UpdatePartnerAsync(Guid id, UpdatePartnerDto dto);
    Task<ApiResponse<bool>> DeletePartnerAsync(Guid id);
    Task<ApiResponse<PartnerDto>> ToggleActiveStatusAsync(Guid id);
    Task<ApiResponse<bool>> ReorderPartnersAsync(List<ReorderPartnerDto> reorderDtos);
}
