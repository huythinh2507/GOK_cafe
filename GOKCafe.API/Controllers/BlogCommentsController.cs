using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages blog comment operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/blogs/{blogId:guid}/comments")]
public class BlogCommentsController : ControllerBase
{
    private readonly IBlogCommentService _commentService;

    public BlogCommentsController(IBlogCommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Get all comments for a blog with pagination
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="isApproved">Filter by approval status (default: true - approved comments only)</param>
    /// <returns>A paginated list of comments for the blog</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCommentDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCommentDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBlogComments(
        Guid blogId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isApproved = true)
    {
        var result = await _commentService.GetBlogCommentsAsync(blogId, pageNumber, pageSize, isApproved);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="id">The unique identifier of the comment</param>
    /// <returns>The comment with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(Guid blogId, Guid id)
    {
        var result = await _commentService.GetCommentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new comment for a blog (requires authentication)
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="dto">The comment creation data</param>
    /// <returns>The newly created comment</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateComment(Guid blogId, [FromBody] CreateBlogCommentDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<BlogCommentDto>.FailureResult("User authentication required"));

        dto.BlogId = blogId;

        var result = await _commentService.CreateCommentAsync(dto, userId.Value);
        return result.Success
            ? CreatedAtAction(nameof(GetCommentById), new { blogId, id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing comment (requires authentication, user must be comment owner)
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="id">The unique identifier of the comment to update</param>
    /// <param name="dto">The comment update data</param>
    /// <returns>The updated comment</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<BlogCommentDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(Guid blogId, Guid id, [FromBody] UpdateBlogCommentDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<BlogCommentDto>.FailureResult("User authentication required"));

        var result = await _commentService.UpdateCommentAsync(id, dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a comment (requires authentication, user must be comment owner)
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="id">The unique identifier of the comment to delete</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid blogId, Guid id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<bool>.FailureResult("User authentication required"));

        var result = await _commentService.DeleteCommentAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Approve a comment (Admin only)
    /// </summary>
    /// <param name="blogId">The unique identifier of the blog</param>
    /// <param name="id">The unique identifier of the comment</param>
    /// <returns>Success status</returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveComment(Guid blogId, Guid id)
    {
        var result = await _commentService.ApproveCommentAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get authenticated user's blog comments
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <returns>A paginated list of the user's blog comments</returns>
    [Authorize]
    [HttpGet("~/api/v1/my-blog-comments")]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCommentDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCommentDto>>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCommentDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyComments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<PaginatedResponse<BlogCommentDto>>.FailureResult("User authentication required"));

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
