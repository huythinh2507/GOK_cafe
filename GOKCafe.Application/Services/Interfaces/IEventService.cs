using GOKCafe.Application.DTOs.Event;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface IEventService
{
    // Event Management
    Task<PaginatedResponse<EventDto>> GetEventsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? city = null,
        string? status = null,
        bool? isFeatured = null);

    Task<EventDto?> GetEventByIdAsync(Guid id);
    Task<EventDto?> GetEventBySlugAsync(string slug);
    Task<EventDto> CreateEventAsync(CreateEventDto createDto);
    Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto updateDto);
    Task<bool> DeleteEventAsync(Guid id);
    Task<List<string>> GetCitiesWithEventsAsync();

    // Event Registration
    Task<EventRegistrationDto> RegisterForEventAsync(CreateEventRegistrationDto createDto, Guid? userId = null);
    Task<PaginatedResponse<EventRegistrationDto>> GetEventRegistrationsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        string? status = null);
    Task<EventRegistrationDto?> GetRegistrationByIdAsync(Guid id);
    Task<EventRegistrationDto> UpdateRegistrationStatusAsync(Guid id, string status);
    Task<bool> CancelRegistrationAsync(Guid id, string? reason = null);

    // Event Reviews
    Task<PaginatedResponse<EventReviewDto>> GetEventReviewsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = null,
        bool? isFeatured = null);
    Task<EventReviewDto> CreateEventReviewAsync(CreateEventReviewDto createDto, Guid? userId = null);
    Task<EventReviewDto> ApproveReviewAsync(Guid id, bool isApproved);
    Task<EventReviewDto> ToggleFeaturedReviewAsync(Guid id);
    Task<bool> DeleteReviewAsync(Guid id);

    // Event Notifications
    Task<bool> SubscribeToEventsAsync(EventNotificationSubscriptionDto subscriptionDto);
    Task<bool> UnsubscribeFromEventsAsync(string email);

    // Event Highlights
    Task<List<string>> GetEventHighlightsAsync(Guid eventId);
}
