using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Contact;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages contact messages and contact information in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    #region Contact Messages

    /// <summary>
    /// Submit a new contact message (public endpoint for contact form)
    /// </summary>
    /// <param name="dto">The contact message data from the form</param>
    /// <returns>Confirmation message</returns>
    [HttpPost("messages")]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContactMessage([FromBody] CreateContactMessageDto dto)
    {
        var result = await _contactService.CreateContactMessageAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetContactMessageById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Get all contact messages with pagination and filtering (admin only)
    /// </summary>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="isRead">Filter by read status</param>
    /// <param name="status">Filter by status (Pending, InProgress, Resolved, Closed)</param>
    /// <param name="category">Filter by category</param>
    /// <returns>Paginated list of contact messages</returns>
    [Authorize]
    [HttpGet("messages")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ContactMessageDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ContactMessageDto>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ContactMessageDto>>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContactMessages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null)
    {
        var result = await _contactService.GetContactMessagesAsync(pageNumber, pageSize, isRead, status, category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific contact message by ID (admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the contact message</param>
    /// <returns>The contact message details</returns>
    [Authorize]
    [HttpGet("messages/{id:guid}")]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContactMessageById(Guid id)
    {
        var result = await _contactService.GetContactMessageByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Update a contact message (reply or change status) (admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the contact message</param>
    /// <param name="dto">The update data (reply and/or status)</param>
    /// <returns>The updated contact message</returns>
    [Authorize]
    [HttpPut("messages/{id:guid}")]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContactMessage(Guid id, [FromBody] UpdateContactMessageDto dto)
    {
        var result = await _contactService.UpdateContactMessageAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a contact message (admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the contact message</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete("messages/{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContactMessage(Guid id)
    {
        var result = await _contactService.DeleteContactMessageAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark a contact message as read (admin only)
    /// </summary>
    /// <param name="id">The unique identifier of the contact message</param>
    /// <returns>The updated contact message</returns>
    [Authorize]
    [HttpPatch("messages/{id:guid}/mark-as-read")]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<ContactMessageDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _contactService.MarkAsReadAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get available contact categories for the form dropdown
    /// </summary>
    /// <returns>List of contact categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType<ApiResponse<List<string>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactCategories()
    {
        var result = await _contactService.GetContactCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Contact Info

    /// <summary>
    /// Get active contact information for display on contact page
    /// </summary>
    /// <returns>Active contact information</returns>
    [HttpGet("info")]
    [ProducesResponseType<ApiResponse<ContactInfoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ContactInfoDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveContactInfo()
    {
        var result = await _contactService.GetActiveContactInfoAsync();
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get all contact information records (admin only)
    /// </summary>
    /// <returns>List of all contact information records</returns>
    [Authorize]
    [HttpGet("info/all")]
    [ProducesResponseType<ApiResponse<List<ContactInfoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<ContactInfoDto>>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllContactInfos()
    {
        var result = await _contactService.GetAllContactInfosAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
