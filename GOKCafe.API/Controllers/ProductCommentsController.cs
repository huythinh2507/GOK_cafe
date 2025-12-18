using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductComment;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages product comment operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/products/{productId:guid}/comments")]
public class ProductCommentsController : ControllerBase
{
    private readonly IProductCommentService _commentService;

    public ProductCommentsController(IProductCommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Get all comments for a product with pagination
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="isApproved">Filter by approval status (default: true - approved comments only)</param>
    /// <returns>A paginated list of comments for the product</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ProductCommentDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ProductCommentDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductComments(
        Guid productId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isApproved = true)
    {
        var result = await _commentService.GetProductCommentsAsync(productId, pageNumber, pageSize, isApproved);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get comment summary (total comments, average rating, rating distribution) for a product
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <returns>Comment summary statistics</returns>
    [HttpGet("summary")]
    [ProducesResponseType<ApiResponse<ProductCommentSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductCommentSummaryDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductCommentSummary(Guid productId)
    {
        var result = await _commentService.GetProductCommentSummaryAsync(productId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="id">The unique identifier of the comment</param>
    /// <returns>The comment with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(Guid productId, Guid id)
    {
        var result = await _commentService.GetCommentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new comment for a product (requires authentication)
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="dto">The comment creation data</param>
    /// <returns>The newly created comment</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateComment(Guid productId, [FromBody] CreateProductCommentDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<ProductCommentDto>.FailureResult("User authentication required"));

        // Ensure productId in route matches productId in body
        dto.ProductId = productId;

        var result = await _commentService.CreateCommentAsync(userId.Value, dto);
        return result.Success
            ? CreatedAtAction(nameof(GetCommentById), new { productId, id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing comment (requires authentication, user must be comment owner)
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="id">The unique identifier of the comment to update</param>
    /// <param name="dto">The comment update data</param>
    /// <returns>The updated comment</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<ProductCommentDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(Guid productId, Guid id, [FromBody] UpdateProductCommentDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<ProductCommentDto>.FailureResult("User authentication required"));

        var result = await _commentService.UpdateCommentAsync(id, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a comment (requires authentication, user must be comment owner)
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="id">The unique identifier of the comment to delete</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid productId, Guid id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<bool>.FailureResult("User authentication required"));

        var result = await _commentService.DeleteCommentAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Approve or disapprove a comment (Admin only)
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="id">The unique identifier of the comment</param>
    /// <param name="dto">The approval status</param>
    /// <returns>Success status</returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveComment(Guid productId, Guid id, [FromBody] ApproveProductCommentDto dto)
    {
        var result = await _commentService.ApproveCommentAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get authenticated user's comments
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <returns>A paginated list of the user's comments</returns>
    [Authorize]
    [HttpGet("~/api/v1/my-comments")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ProductCommentDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ProductCommentDto>>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<ProductCommentDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyComments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<PaginatedResponse<ProductCommentDto>>.FailureResult("User authentication required"));

        var result = await _commentService.GetUserCommentsAsync(userId.Value, pageNumber, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}
