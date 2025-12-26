using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Partner;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages partnership operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    /// <summary>
    /// Get all partners with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <returns>A paginated list of partners</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<PartnerDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<PartnerDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartners(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        var result = await _partnerService.GetPartnersAsync(pageNumber, pageSize, isActive);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all active partners (public endpoint for frontend display)
    /// </summary>
    /// <returns>A list of active partners ordered by display order</returns>
    [HttpGet("public")]
    [ProducesResponseType<ApiResponse<List<PartnerDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<PartnerDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActivePartners()
    {
        var result = await _partnerService.GetActivePartnersAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific partner by ID
    /// </summary>
    /// <param name="id">The unique identifier of the partner</param>
    /// <returns>The partner with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPartnerById(Guid id)
    {
        var result = await _partnerService.GetPartnerByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new partner (requires authentication)
    /// </summary>
    /// <param name="dto">The partner creation data</param>
    /// <returns>The newly created partner</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerDto dto)
    {
        var result = await _partnerService.CreatePartnerAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetPartnerById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing partner (requires authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the partner to update</param>
    /// <param name="dto">The partner update data</param>
    /// <returns>The updated partner</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartner(Guid id, [FromBody] UpdatePartnerDto dto)
    {
        var result = await _partnerService.UpdatePartnerAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a partner (requires authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the partner to delete</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePartner(Guid id)
    {
        var result = await _partnerService.DeletePartnerAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Toggle the active status of a partner (requires authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the partner</param>
    /// <returns>The updated partner with toggled status</returns>
    [Authorize]
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<PartnerDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActiveStatus(Guid id)
    {
        var result = await _partnerService.ToggleActiveStatusAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Reorder multiple partners (requires authentication)
    /// </summary>
    /// <param name="reorderDtos">List of partner IDs with their new display order</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpPatch("reorder")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReorderPartners([FromBody] List<ReorderPartnerDto> reorderDtos)
    {
        var result = await _partnerService.ReorderPartnersAsync(reorderDtos);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
