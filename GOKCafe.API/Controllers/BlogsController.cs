using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages blog operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogsController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    /// <summary>
    /// Get all blogs with pagination, search, and filtering
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="searchTerm">Optional search term to filter blogs</param>
    /// <param name="categoryId">Optional category ID to filter blogs</param>
    /// <param name="isPublished">Optional filter by published status</param>
    /// <param name="tag">Optional tag to filter blogs</param>
    /// <param name="authorId">Optional author ID to filter blogs</param>
    /// <returns>A paginated list of blogs</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBlogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] string? tag = null,
        [FromQuery] Guid? authorId = null)
    {
        var result = await _blogService.GetBlogsAsync(pageNumber, pageSize, searchTerm, categoryId, isPublished, tag, authorId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all tags from all blogs
    /// </summary>
    /// <returns>A list of unique tags</returns>
    [HttpGet("tags")]
    [ProducesResponseType<ApiResponse<List<string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<string>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllTags()
    {
        var result = await _blogService.GetAllTagsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific blog by ID
    /// </summary>
    /// <param name="id">The unique identifier of the blog</param>
    /// <returns>The blog with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogById(Guid id)
    {
        var result = await _blogService.GetBlogByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get a specific blog by slug
    /// </summary>
    /// <param name="slug">The unique slug of the blog</param>
    /// <returns>The blog with the specified slug</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogBySlug(string slug)
    {
        var result = await _blogService.GetBlogBySlugAsync(slug);

        if (result.Success && result.Data != null)
        {
            await _blogService.IncrementViewCountAsync(result.Data.Id);
        }

        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new blog (requires authentication)
    /// </summary>
    /// <param name="dto">The blog creation data</param>
    /// <returns>The newly created blog</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<BlogDto>.FailureResult("User authentication required"));

        var result = await _blogService.CreateBlogAsync(dto, userId.Value);
        return result.Success
            ? CreatedAtAction(nameof(GetBlogById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing blog (requires authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the blog to update</param>
    /// <param name="dto">The blog update data</param>
    /// <returns>The updated blog</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<BlogDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] UpdateBlogDto dto)
    {
        var result = await _blogService.UpdateBlogAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a blog (requires authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the blog to delete</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBlog(Guid id)
    {
        var result = await _blogService.DeleteBlogAsync(id);
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
