using GOKCafe.Application.DTOs.Event;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    // Public Endpoints

    /// <summary>
    /// Get all events with optional filters (Public)
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicEvents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? city = null,
        [FromQuery] bool? isFeatured = null)
    {
        try
        {
            var result = await _eventService.GetEventsAsync(pageNumber, pageSize, city, "Published", isFeatured);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public events");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving events"));
        }
    }

    /// <summary>
    /// Get event by ID (Public)
    /// </summary>
    [HttpGet("public/{id}")]
    public async Task<IActionResult> GetPublicEventById(Guid id)
    {
        try
        {
            var result = await _eventService.GetEventByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            if (result.Status != "Published")
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            return Ok(ApiResponse<EventDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public event by ID {EventId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving the event"));
        }
    }

    /// <summary>
    /// Get event by slug (Public)
    /// </summary>
    [HttpGet("public/slug/{slug}")]
    public async Task<IActionResult> GetPublicEventBySlug(string slug)
    {
        try
        {
            var result = await _eventService.GetEventBySlugAsync(slug);
            if (result == null)
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            if (result.Status != "Published")
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            return Ok(ApiResponse<EventDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public event by slug {Slug}", slug);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving the event"));
        }
    }

    /// <summary>
    /// Get cities with events (Public)
    /// </summary>
    [HttpGet("cities")]
    public async Task<IActionResult> GetCitiesWithEvents()
    {
        try
        {
            var result = await _eventService.GetCitiesWithEventsAsync();
            return Ok(ApiResponse<List<string>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cities with events");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving cities"));
        }
    }

    /// <summary>
    /// Register for an event (Public)
    /// </summary>
    [HttpPost("registrations")]
    public async Task<IActionResult> RegisterForEvent([FromBody] CreateEventRegistrationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid registration data"));

            var userId = User.Identity?.IsAuthenticated == true
                ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : (Guid?)null;

            var result = await _eventService.RegisterForEventAsync(createDto, userId);
            return Ok(ApiResponse<EventRegistrationDto>.SuccessResult(result, "Registration successful"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering for event");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while processing your registration"));
        }
    }

    /// <summary>
    /// Get event reviews (Public - only approved reviews)
    /// </summary>
    [HttpGet("{eventId}/reviews")]
    public async Task<IActionResult> GetEventReviews(
        Guid eventId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isFeatured = null)
    {
        try
        {
            var result = await _eventService.GetEventReviewsAsync(eventId, pageNumber, pageSize, true, isFeatured);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event reviews for event {EventId}", eventId);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving reviews"));
        }
    }

    /// <summary>
    /// Submit event review (Public)
    /// </summary>
    [HttpPost("reviews")]
    public async Task<IActionResult> CreateEventReview([FromBody] CreateEventReviewDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid review data"));

            var userId = User.Identity?.IsAuthenticated == true
                ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : (Guid?)null;

            var result = await _eventService.CreateEventReviewAsync(createDto, userId);
            return Ok(ApiResponse<EventReviewDto>.SuccessResult(result, "Review submitted successfully. It will be visible after approval."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event review");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while submitting your review"));
        }
    }

    /// <summary>
    /// Subscribe to event notifications (Public)
    /// </summary>
    [HttpPost("notifications/subscribe")]
    public async Task<IActionResult> SubscribeToEvents([FromBody] EventNotificationSubscriptionDto subscriptionDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid subscription data"));

            var result = await _eventService.SubscribeToEventsAsync(subscriptionDto);
            return Ok(ApiResponse<bool>.SuccessResult(result, "Subscribed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to event notifications");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while processing your subscription"));
        }
    }

    /// <summary>
    /// Unsubscribe from event notifications (Public)
    /// </summary>
    [HttpDelete("notifications/unsubscribe/{email}")]
    public async Task<IActionResult> UnsubscribeFromEvents(string email)
    {
        try
        {
            var result = await _eventService.UnsubscribeFromEventsAsync(email);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResult("Subscription not found"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Unsubscribed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from event notifications");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Get event highlights (Public)
    /// </summary>
    [HttpGet("{eventId}/highlights")]
    public async Task<IActionResult> GetEventHighlights(Guid eventId)
    {
        try
        {
            var result = await _eventService.GetEventHighlightsAsync(eventId);
            return Ok(ApiResponse<List<string>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event highlights for event {EventId}", eventId);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving highlights"));
        }
    }

    // Admin Endpoints

    /// <summary>
    /// Get all events (Admin)
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? city = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isFeatured = null)
    {
        try
        {
            var result = await _eventService.GetEventsAsync(pageNumber, pageSize, city, status, isFeatured);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving events"));
        }
    }

    /// <summary>
    /// Get event by ID (Admin)
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        try
        {
            var result = await _eventService.GetEventByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            return Ok(ApiResponse<EventDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by ID {EventId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving the event"));
        }
    }

    /// <summary>
    /// Create a new event (Admin)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid event data"));

            var result = await _eventService.CreateEventAsync(createDto);
            return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while creating the event"));
        }
    }

    /// <summary>
    /// Update an event (Admin)
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid event data"));

            var result = await _eventService.UpdateEventAsync(id, updateDto);
            return Ok(ApiResponse<EventDto>.SuccessResult(result, "Event updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while updating the event"));
        }
    }

    /// <summary>
    /// Delete an event (Admin)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            var result = await _eventService.DeleteEventAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResult("Event not found"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Event deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while deleting the event"));
        }
    }

    /// <summary>
    /// Get event registrations (Admin)
    /// </summary>
    [Authorize]
    [HttpGet("{eventId}/registrations")]
    public async Task<IActionResult> GetEventRegistrations(
        Guid eventId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            var result = await _eventService.GetEventRegistrationsAsync(eventId, pageNumber, pageSize, status);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event registrations for event {EventId}", eventId);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving registrations"));
        }
    }

    /// <summary>
    /// Get registration by ID (Admin)
    /// </summary>
    [Authorize]
    [HttpGet("registrations/{id}")]
    public async Task<IActionResult> GetRegistrationById(Guid id)
    {
        try
        {
            var result = await _eventService.GetRegistrationByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.FailureResult("Registration not found"));

            return Ok(ApiResponse<EventRegistrationDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration by ID {RegistrationId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving the registration"));
        }
    }

    /// <summary>
    /// Update registration status (Admin)
    /// </summary>
    [Authorize]
    [HttpPatch("registrations/{id}/status")]
    public async Task<IActionResult> UpdateRegistrationStatus(Guid id, [FromBody] string status)
    {
        try
        {
            var result = await _eventService.UpdateRegistrationStatusAsync(id, status);
            return Ok(ApiResponse<EventRegistrationDto>.SuccessResult(result, "Registration status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration status {RegistrationId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while updating the registration status"));
        }
    }

    /// <summary>
    /// Cancel registration (Admin)
    /// </summary>
    [Authorize]
    [HttpDelete("registrations/{id}")]
    public async Task<IActionResult> CancelRegistration(Guid id, [FromQuery] string? reason = null)
    {
        try
        {
            var result = await _eventService.CancelRegistrationAsync(id, reason);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResult("Registration not found"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Registration cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling registration {RegistrationId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while cancelling the registration"));
        }
    }

    /// <summary>
    /// Get all event reviews (Admin - includes unapproved)
    /// </summary>
    [Authorize]
    [HttpGet("admin/reviews")]
    public async Task<IActionResult> GetAllEventReviews(
        [FromQuery] Guid eventId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isApproved = null,
        [FromQuery] bool? isFeatured = null)
    {
        try
        {
            var result = await _eventService.GetEventReviewsAsync(eventId, pageNumber, pageSize, isApproved, isFeatured);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all event reviews");
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while retrieving reviews"));
        }
    }

    /// <summary>
    /// Approve/reject event review (Admin)
    /// </summary>
    [Authorize]
    [HttpPatch("reviews/{id}/approve")]
    public async Task<IActionResult> ApproveReview(Guid id, [FromBody] bool isApproved)
    {
        try
        {
            var result = await _eventService.ApproveReviewAsync(id, isApproved);
            return Ok(ApiResponse<EventReviewDto>.SuccessResult(result, "Review approval status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while updating the review"));
        }
    }

    /// <summary>
    /// Toggle featured status of review (Admin)
    /// </summary>
    [Authorize]
    [HttpPatch("reviews/{id}/toggle-featured")]
    public async Task<IActionResult> ToggleFeaturedReview(Guid id)
    {
        try
        {
            var result = await _eventService.ToggleFeaturedReviewAsync(id);
            return Ok(ApiResponse<EventReviewDto>.SuccessResult(result, "Review featured status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured review {ReviewId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while updating the review"));
        }
    }

    /// <summary>
    /// Delete event review (Admin)
    /// </summary>
    [Authorize]
    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        try
        {
            var result = await _eventService.DeleteReviewAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResult("Review not found"));

            return Ok(ApiResponse<bool>.SuccessResult(result, "Review deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("An error occurred while deleting the review"));
        }
    }
}
