using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Contact;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string ContactMessageCacheKeyPrefix = "contact-message:";
    private const string ContactMessagesCacheKeyPrefix = "contact-messages:";
    private const string ActiveContactInfoCacheKey = "contact-info:active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public ContactService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    #region Contact Messages

    public async Task<ApiResponse<ContactMessageDto>> CreateContactMessageAsync(CreateContactMessageDto dto)
    {
        try
        {
            var contactMessage = new ContactMessage
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Subject = dto.Subject,
                OrderNumber = dto.OrderNumber,
                Category = dto.Category,
                Description = dto.Description,
                RegisteredPhoneNumber = dto.RegisteredPhoneNumber,
                IsRead = false,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ContactMessages.AddAsync(contactMessage);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateContactMessageCache();

            var contactMessageDto = MapToDto(contactMessage);
            return ApiResponse<ContactMessageDto>.SuccessResult(
                contactMessageDto,
                "Your message has been sent successfully. We'll get back to you soon!");
        }
        catch (Exception ex)
        {
            return ApiResponse<ContactMessageDto>.FailureResult(
                $"Error sending contact message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ContactMessageDto>>> GetContactMessagesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool? isRead = null,
        string? status = null,
        string? category = null)
    {
        try
        {
            var cacheKey = $"{ContactMessagesCacheKeyPrefix}page:{pageNumber}:size:{pageSize}:read:{isRead}:status:{status}:cat:{category}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<ContactMessageDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.ContactMessages.GetQueryable();

            if (isRead.HasValue)
                query = query.Where(c => c.IsRead == isRead.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(c => c.Category == category);

            var totalItems = await query.CountAsync();
            var messages = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageDtos = messages.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<ContactMessageDto>>.SuccessResult(
                new PaginatedResponse<ContactMessageDto>
                {
                    Items = messageDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ContactMessageDto>>.FailureResult(
                $"Error retrieving contact messages: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ContactMessageDto>> GetContactMessageByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{ContactMessageCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ContactMessageDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
            if (message == null)
                return ApiResponse<ContactMessageDto>.FailureResult("Contact message not found");

            var messageDto = MapToDto(message);
            var response = ApiResponse<ContactMessageDto>.SuccessResult(messageDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ContactMessageDto>.FailureResult(
                $"Error retrieving contact message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ContactMessageDto>> UpdateContactMessageAsync(Guid id, UpdateContactMessageDto dto)
    {
        try
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
            if (message == null)
                return ApiResponse<ContactMessageDto>.FailureResult("Contact message not found");

            if (!string.IsNullOrWhiteSpace(dto.Reply))
            {
                message.Reply = dto.Reply;
                message.RepliedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                message.Status = dto.Status;
            }

            message.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ContactMessages.Update(message);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateContactMessageCache();

            var messageDto = MapToDto(message);
            return ApiResponse<ContactMessageDto>.SuccessResult(messageDto, "Contact message updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ContactMessageDto>.FailureResult(
                $"Error updating contact message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteContactMessageAsync(Guid id)
    {
        try
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
            if (message == null)
                return ApiResponse<bool>.FailureResult("Contact message not found");

            _unitOfWork.ContactMessages.SoftDelete(message);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateContactMessageCache();

            return ApiResponse<bool>.SuccessResult(true, "Contact message deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error deleting contact message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ContactMessageDto>> MarkAsReadAsync(Guid id)
    {
        try
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
            if (message == null)
                return ApiResponse<ContactMessageDto>.FailureResult("Contact message not found");

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ContactMessages.Update(message);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateContactMessageCache();

            var messageDto = MapToDto(message);
            return ApiResponse<ContactMessageDto>.SuccessResult(messageDto, "Message marked as read");
        }
        catch (Exception ex)
        {
            return ApiResponse<ContactMessageDto>.FailureResult(
                $"Error marking message as read: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<string>>> GetContactCategoriesAsync()
    {
        try
        {
            var categories = new List<string>
            {
                "General Inquiry",
                "Order Issue",
                "Product Question",
                "Wholesale Inquiry",
                "Partnership",
                "Feedback",
                "Complaint",
                "Other"
            };

            return ApiResponse<List<string>>.SuccessResult(categories);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<string>>.FailureResult(
                $"Error retrieving categories: {ex.Message}");
        }
    }

    #endregion

    #region Contact Info

    public async Task<ApiResponse<ContactInfoDto>> GetActiveContactInfoAsync()
    {
        try
        {
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ContactInfoDto>>(ActiveContactInfoCacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var contactInfo = await _unitOfWork.ContactInfos.GetQueryable()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (contactInfo == null)
                return ApiResponse<ContactInfoDto>.FailureResult("No active contact info found");

            var contactInfoDto = MapContactInfoToDto(contactInfo);
            var response = ApiResponse<ContactInfoDto>.SuccessResult(contactInfoDto);

            await _cacheService.SetAsync(ActiveContactInfoCacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ContactInfoDto>.FailureResult(
                $"Error retrieving contact info: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ContactInfoDto>>> GetAllContactInfosAsync()
    {
        try
        {
            var contactInfos = await _unitOfWork.ContactInfos.GetAllAsync();
            var contactInfoDtos = contactInfos.Select(MapContactInfoToDto).ToList();

            return ApiResponse<List<ContactInfoDto>>.SuccessResult(contactInfoDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ContactInfoDto>>.FailureResult(
                $"Error retrieving contact infos: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private ContactMessageDto MapToDto(ContactMessage message)
    {
        return new ContactMessageDto
        {
            Id = message.Id,
            Email = message.Email,
            Subject = message.Subject,
            OrderNumber = message.OrderNumber,
            Category = message.Category,
            Description = message.Description,
            RegisteredPhoneNumber = message.RegisteredPhoneNumber,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            Reply = message.Reply,
            RepliedAt = message.RepliedAt,
            Status = message.Status,
            CreatedAt = message.CreatedAt
        };
    }

    private ContactInfoDto MapContactInfoToDto(ContactInfo contactInfo)
    {
        return new ContactInfoDto
        {
            Id = contactInfo.Id,
            Title = contactInfo.Title,
            Subtitle = contactInfo.Subtitle,
            Description = contactInfo.Description,
            Address = contactInfo.Address,
            Phone = contactInfo.Phone,
            Email = contactInfo.Email,
            WorkingHours = contactInfo.WorkingHours,
            ImageUrl = contactInfo.ImageUrl,
            MapUrl = contactInfo.MapUrl,
            ButtonText = contactInfo.ButtonText,
            ButtonLink = contactInfo.ButtonLink,
            IsActive = contactInfo.IsActive
        };
    }

    private async Task InvalidateContactMessageCache()
    {
        await _cacheService.RemoveAsync(ContactMessagesCacheKeyPrefix);
        await _cacheService.RemoveAsync(ContactMessageCacheKeyPrefix);
    }

    #endregion
}
