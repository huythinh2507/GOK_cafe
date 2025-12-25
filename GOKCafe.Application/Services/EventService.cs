using GOKCafe.Application.DTOs.Event;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GOKCafe.Application.Services;

public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EventService> _logger;
    private const string EventsCacheKey = "events_all";
    private const string CitiesCacheKey = "event_cities";
    private const int CacheExpirationMinutes = 15;

    public EventService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<EventService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    // Event Management
    public async Task<PaginatedResponse<EventDto>> GetEventsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? city = null,
        string? status = null,
        bool? isFeatured = null)
    {
        try
        {
            var events = await _unitOfWork.Events.GetAllAsync();

            // Apply filters
            var query = events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(e => e.City != null && e.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(e => e.IsFeatured == isFeatured.Value);
            }

            // Order by date
            query = query.OrderByDescending(e => e.EventDate);

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            // Load review stats for each event
            foreach (var item in items)
            {
                var reviews = await _unitOfWork.EventReviews.GetAllAsync();
                var eventReviews = reviews.Where(r => r.EventId == item.Id && r.IsApproved).ToList();
                item.AverageRating = eventReviews.Any() ? eventReviews.Average(r => r.Rating) : 0;
                item.ReviewCount = eventReviews.Count;
            }

            return new PaginatedResponse<EventDto>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events");
            throw;
        }
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                return null;

            var dto = MapToDto(eventEntity);

            // Load review stats
            var reviews = await _unitOfWork.EventReviews.GetAllAsync();
            var eventReviews = reviews.Where(r => r.EventId == id && r.IsApproved).ToList();
            dto.AverageRating = eventReviews.Any() ? eventReviews.Average(r => r.Rating) : 0;
            dto.ReviewCount = eventReviews.Count;

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by ID {EventId}", id);
            throw;
        }
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug)
    {
        try
        {
            var events = await _unitOfWork.Events.GetAllAsync();
            var eventEntity = events.FirstOrDefault(e => e.Slug == slug);

            if (eventEntity == null)
                return null;

            var dto = MapToDto(eventEntity);

            // Load review stats
            var reviews = await _unitOfWork.EventReviews.GetAllAsync();
            var eventReviews = reviews.Where(r => r.EventId == eventEntity.Id && r.IsApproved).ToList();
            dto.AverageRating = eventReviews.Any() ? eventReviews.Average(r => r.Rating) : 0;
            dto.ReviewCount = eventReviews.Count;

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by slug {Slug}", slug);
            throw;
        }
    }

    public async Task<EventDto> CreateEventAsync(CreateEventDto createDto)
    {
        try
        {
            var eventEntity = new Event
            {
                Title = createDto.Title,
                Slug = createDto.Slug ?? createDto.Title.ToLower().Replace(" ", "-"),
                Description = createDto.Description,
                ShortDescription = createDto.ShortDescription,
                EventDate = createDto.EventDate,
                EventEndDate = createDto.EventEndDate,
                EventTime = createDto.EventTime,
                Venue = createDto.Venue,
                Address = createDto.Address,
                City = createDto.City,
                MapUrl = createDto.MapUrl,
                Price = createDto.Price,
                Currency = createDto.Currency,
                MaxCapacity = createDto.MaxCapacity,
                IsRegistrationOpen = createDto.IsRegistrationOpen,
                RegistrationDeadline = createDto.RegistrationDeadline,
                IsFeatured = createDto.IsFeatured,
                IsActive = createDto.IsActive,
                FeaturedImageUrl = createDto.FeaturedImageUrl,
                GalleryImages = createDto.GalleryImages != null ? string.Join(",", createDto.GalleryImages) : null,
                Status = createDto.Status ?? "Upcoming",
                Tags = createDto.Tags,
                MetaTitle = createDto.MetaTitle,
                MetaDescription = createDto.MetaDescription,
                DisplayOrder = createDto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Events.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(EventsCacheKey);
            await _cacheService.RemoveAsync(CitiesCacheKey);

            _logger.LogInformation("Event created: {EventId}", eventEntity.Id);
            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    public async Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto updateDto)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {id} not found");

            eventEntity.Title = updateDto.Title;
            eventEntity.Description = updateDto.Description;
            eventEntity.ShortDescription = updateDto.ShortDescription;
            eventEntity.EventDate = updateDto.EventDate;
            eventEntity.EventEndDate = updateDto.EventEndDate;
            eventEntity.EventTime = updateDto.EventTime;
            eventEntity.Venue = updateDto.Venue;
            eventEntity.Address = updateDto.Address;
            eventEntity.City = updateDto.City;
            eventEntity.MapUrl = updateDto.MapUrl;
            eventEntity.Price = updateDto.Price;
            eventEntity.Currency = updateDto.Currency;
            eventEntity.MaxCapacity = updateDto.MaxCapacity;
            eventEntity.IsRegistrationOpen = updateDto.IsRegistrationOpen;
            eventEntity.RegistrationDeadline = updateDto.RegistrationDeadline;
            eventEntity.IsFeatured = updateDto.IsFeatured;
            eventEntity.IsActive = updateDto.IsActive;
            eventEntity.FeaturedImageUrl = updateDto.FeaturedImageUrl;
            eventEntity.GalleryImages = updateDto.GalleryImages != null ? string.Join(",", updateDto.GalleryImages) : null;
            eventEntity.Status = updateDto.Status;
            eventEntity.Tags = updateDto.Tags;
            eventEntity.MetaTitle = updateDto.MetaTitle;
            eventEntity.MetaDescription = updateDto.MetaDescription;
            eventEntity.DisplayOrder = updateDto.DisplayOrder;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Events.Update(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(EventsCacheKey);
            await _cacheService.RemoveAsync(CitiesCacheKey);

            _logger.LogInformation("Event updated: {EventId}", id);
            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                return false;

            _unitOfWork.Events.Remove(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(EventsCacheKey);
            await _cacheService.RemoveAsync(CitiesCacheKey);

            _logger.LogInformation("Event deleted: {EventId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            throw;
        }
    }

    public async Task<List<string>> GetCitiesWithEventsAsync()
    {
        try
        {
            var cachedCities = await _cacheService.GetAsync<List<string>>(CitiesCacheKey);
            if (cachedCities != null)
                return cachedCities;

            var events = await _unitOfWork.Events.GetAllAsync();
            var cities = events
                .Where(e => !string.IsNullOrWhiteSpace(e.City) && e.Status == "Published")
                .Select(e => e.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            await _cacheService.SetAsync(CitiesCacheKey, cities, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return cities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cities with events");
            throw;
        }
    }

    // Event Registration
    public async Task<EventRegistrationDto> RegisterForEventAsync(CreateEventRegistrationDto createDto, Guid? userId = null)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(createDto.EventId);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {createDto.EventId} not found");

            // Check if event is full
            var registrations = await _unitOfWork.EventRegistrations.GetAllAsync();
            var currentAttendees = registrations
                .Where(r => r.EventId == createDto.EventId && r.Status == "Confirmed")
                .Sum(r => r.NumberOfAttendees);

            if (eventEntity.MaxCapacity.HasValue && currentAttendees + createDto.NumberOfAttendees > eventEntity.MaxCapacity.Value)
            {
                throw new InvalidOperationException("Event is full");
            }

            var registration = new EventRegistration
            {
                EventId = createDto.EventId,
                UserId = userId,
                FullName = createDto.FullName,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                NumberOfAttendees = createDto.NumberOfAttendees,
                Notes = createDto.Notes,
                SpecialRequirements = createDto.SpecialRequirements,
                Status = "Pending",
                RegistrationDate = DateTime.UtcNow,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EventRegistrations.AddAsync(registration);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Event registration created: {RegistrationId} for Event {EventId}", registration.Id, createDto.EventId);
            return MapToRegistrationDto(registration, eventEntity.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event registration");
            throw;
        }
    }

    public async Task<PaginatedResponse<EventRegistrationDto>> GetEventRegistrationsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        string? status = null)
    {
        try
        {
            var registrations = await _unitOfWork.EventRegistrations.GetAllAsync();
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            var eventTitle = eventEntity?.Title;

            var query = registrations.Where(r => r.EventId == eventId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            query = query.OrderByDescending(r => r.RegistrationDate);

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapToRegistrationDto(r, eventTitle))
                .ToList();

            return new PaginatedResponse<EventRegistrationDto>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event registrations for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<EventRegistrationDto?> GetRegistrationByIdAsync(Guid id)
    {
        try
        {
            var registration = await _unitOfWork.EventRegistrations.GetByIdAsync(id);
            if (registration == null)
                return null;

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(registration.EventId);
            return MapToRegistrationDto(registration, eventEntity?.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration by ID {RegistrationId}", id);
            throw;
        }
    }

    public async Task<EventRegistrationDto> UpdateRegistrationStatusAsync(Guid id, string status)
    {
        try
        {
            var registration = await _unitOfWork.EventRegistrations.GetByIdAsync(id);
            if (registration == null)
                throw new KeyNotFoundException($"Registration with ID {id} not found");

            registration.Status = status;
            registration.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.EventRegistrations.Update(registration);
            await _unitOfWork.SaveChangesAsync();

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(registration.EventId);
            _logger.LogInformation("Registration status updated: {RegistrationId} to {Status}", id, status);
            return MapToRegistrationDto(registration, eventEntity?.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration status {RegistrationId}", id);
            throw;
        }
    }

    public async Task<bool> CancelRegistrationAsync(Guid id, string? reason = null)
    {
        try
        {
            var registration = await _unitOfWork.EventRegistrations.GetByIdAsync(id);
            if (registration == null)
                return false;

            registration.Status = "Cancelled";
            registration.CancellationDate = DateTime.UtcNow;
            registration.CancellationReason = reason;
            registration.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.EventRegistrations.Update(registration);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Registration cancelled: {RegistrationId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling registration {RegistrationId}", id);
            throw;
        }
    }

    // Event Reviews
    public async Task<PaginatedResponse<EventReviewDto>> GetEventReviewsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = null,
        bool? isFeatured = null)
    {
        try
        {
            var reviews = await _unitOfWork.EventReviews.GetAllAsync();
            var query = reviews.Where(r => r.EventId == eventId).AsQueryable();

            if (isApproved.HasValue)
            {
                query = query.Where(r => r.IsApproved == isApproved.Value);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(r => r.IsFeatured == isFeatured.Value);
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToReviewDto)
                .ToList();

            return new PaginatedResponse<EventReviewDto>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event reviews for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<EventReviewDto> CreateEventReviewAsync(CreateEventReviewDto createDto, Guid? userId = null)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(createDto.EventId);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {createDto.EventId} not found");

            var review = new EventReview
            {
                EventId = createDto.EventId,
                UserId = userId,
                ReviewerName = createDto.ReviewerName,
                ReviewerEmail = createDto.ReviewerEmail,
                Rating = createDto.Rating,
                Comment = createDto.Comment,
                IsApproved = false,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EventReviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Event review created: {ReviewId} for Event {EventId}", review.Id, createDto.EventId);
            return MapToReviewDto(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event review");
            throw;
        }
    }

    public async Task<EventReviewDto> ApproveReviewAsync(Guid id, bool isApproved)
    {
        try
        {
            var review = await _unitOfWork.EventReviews.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found");

            review.IsApproved = isApproved;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.EventReviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Review approval status updated: {ReviewId} to {IsApproved}", id, isApproved);
            return MapToReviewDto(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", id);
            throw;
        }
    }

    public async Task<EventReviewDto> ToggleFeaturedReviewAsync(Guid id)
    {
        try
        {
            var review = await _unitOfWork.EventReviews.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found");

            review.IsFeatured = !review.IsFeatured;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.EventReviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Review featured status toggled: {ReviewId} to {IsFeatured}", id, review.IsFeatured);
            return MapToReviewDto(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured review {ReviewId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteReviewAsync(Guid id)
    {
        try
        {
            var review = await _unitOfWork.EventReviews.GetByIdAsync(id);
            if (review == null)
                return false;

            _unitOfWork.EventReviews.Remove(review);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Review deleted: {ReviewId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            throw;
        }
    }

    // Event Notifications
    public async Task<bool> SubscribeToEventsAsync(EventNotificationSubscriptionDto subscriptionDto)
    {
        try
        {
            var subscriptions = await _unitOfWork.EventNotificationSubscriptions.GetAllAsync();
            var existing = subscriptions.FirstOrDefault(s =>
                s.Email == subscriptionDto.Email &&
                s.City == subscriptionDto.City);

            if (existing != null)
            {
                return true; // Already subscribed
            }

            var subscription = new EventNotificationSubscription
            {
                Email = subscriptionDto.Email,
                City = subscriptionDto.City,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EventNotificationSubscriptions.AddAsync(subscription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Event notification subscription created for {Email}", subscriptionDto.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event notification subscription");
            throw;
        }
    }

    public async Task<bool> UnsubscribeFromEventsAsync(string email)
    {
        try
        {
            var subscriptions = await _unitOfWork.EventNotificationSubscriptions.GetAllAsync();
            var subscription = subscriptions.FirstOrDefault(s => s.Email == email);

            if (subscription == null)
                return false;

            _unitOfWork.EventNotificationSubscriptions.Remove(subscription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Event notification subscription removed for {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing event notification subscription");
            throw;
        }
    }

    // Event Highlights
    public async Task<List<string>> GetEventHighlightsAsync(Guid eventId)
    {
        try
        {
            var highlights = await _unitOfWork.EventHighlights.GetAllAsync();
            return highlights
                .Where(h => h.EventId == eventId && h.IsActive)
                .OrderBy(h => h.DisplayOrder)
                .Select(h => h.ImageUrl)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event highlights for event {EventId}", eventId);
            throw;
        }
    }

    // Helper methods
    private EventDto MapToDto(Event eventEntity)
    {
        return new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Slug = eventEntity.Slug,
            Description = eventEntity.Description,
            ShortDescription = eventEntity.ShortDescription,
            EventDate = eventEntity.EventDate,
            EventEndDate = eventEntity.EventEndDate,
            EventTime = eventEntity.EventTime,
            Venue = eventEntity.Venue,
            Address = eventEntity.Address,
            City = eventEntity.City,
            MapUrl = eventEntity.MapUrl,
            Price = eventEntity.Price,
            Currency = eventEntity.Currency,
            MaxCapacity = eventEntity.MaxCapacity,
            RegisteredCount = eventEntity.RegisteredCount,
            IsRegistrationOpen = eventEntity.IsRegistrationOpen,
            RegistrationDeadline = eventEntity.RegistrationDeadline,
            IsActive = eventEntity.IsActive,
            IsFeatured = eventEntity.IsFeatured,
            FeaturedImageUrl = eventEntity.FeaturedImageUrl,
            GalleryImages = !string.IsNullOrWhiteSpace(eventEntity.GalleryImages) ? eventEntity.GalleryImages.Split(',').ToList() : null,
            Status = eventEntity.Status,
            Tags = eventEntity.Tags,
            MetaTitle = eventEntity.MetaTitle,
            MetaDescription = eventEntity.MetaDescription,
            DisplayOrder = eventEntity.DisplayOrder,
            AverageRating = 0,
            ReviewCount = 0,
            CreatedAt = eventEntity.CreatedAt
        };
    }

    private EventRegistrationDto MapToRegistrationDto(EventRegistration registration, string? eventTitle)
    {
        return new EventRegistrationDto
        {
            Id = registration.Id,
            EventId = registration.EventId,
            EventTitle = eventTitle,
            UserId = registration.UserId,
            FullName = registration.FullName,
            Email = registration.Email,
            PhoneNumber = registration.PhoneNumber,
            NumberOfAttendees = registration.NumberOfAttendees,
            Status = registration.Status,
            RegistrationDate = registration.RegistrationDate,
            CancellationDate = registration.CancellationDate,
            CancellationReason = registration.CancellationReason,
            AmountPaid = registration.AmountPaid,
            PaymentStatus = registration.PaymentStatus,
            PaymentMethod = registration.PaymentMethod,
            TransactionId = registration.TransactionId,
            Notes = registration.Notes,
            SpecialRequirements = registration.SpecialRequirements
        };
    }

    private EventReviewDto MapToReviewDto(EventReview review)
    {
        return new EventReviewDto
        {
            Id = review.Id,
            EventId = review.EventId,
            UserId = review.UserId,
            ReviewerName = review.ReviewerName,
            Rating = review.Rating,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            IsFeatured = review.IsFeatured,
            CreatedAt = review.CreatedAt
        };
    }
}
