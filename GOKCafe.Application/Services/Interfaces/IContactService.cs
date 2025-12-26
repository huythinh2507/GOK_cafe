using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Contact;

namespace GOKCafe.Application.Services.Interfaces;

public interface IContactService
{
    // Contact Message APIs
    Task<ApiResponse<ContactMessageDto>> CreateContactMessageAsync(CreateContactMessageDto dto);
    Task<ApiResponse<PaginatedResponse<ContactMessageDto>>> GetContactMessagesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool? isRead = null,
        string? status = null,
        string? category = null);
    Task<ApiResponse<ContactMessageDto>> GetContactMessageByIdAsync(Guid id);
    Task<ApiResponse<ContactMessageDto>> UpdateContactMessageAsync(Guid id, UpdateContactMessageDto dto);
    Task<ApiResponse<bool>> DeleteContactMessageAsync(Guid id);
    Task<ApiResponse<ContactMessageDto>> MarkAsReadAsync(Guid id);
    Task<ApiResponse<List<string>>> GetContactCategoriesAsync();

    // Contact Info APIs
    Task<ApiResponse<ContactInfoDto>> GetActiveContactInfoAsync();
    Task<ApiResponse<List<ContactInfoDto>>> GetAllContactInfosAsync();
}
