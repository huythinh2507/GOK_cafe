using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages blog category operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/blog-categories")]
public class BlogCategoriesController : ControllerBase
{
    private readonly IBlogCategoryService _categoryService;

    public BlogCategoriesController(IBlogCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all blog categories with pagination
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <returns>A paginated list of blog categories</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCategoryDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<PaginatedResponse<BlogCategoryDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetCategoriesAsync(pageNumber, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all blog categories without pagination
    /// </summary>
    /// <returns>A list of all blog categories</returns>
    [HttpGet("all")]
    [ProducesResponseType<ApiResponse<List<BlogCategoryDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<BlogCategoryDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific blog category by ID
    /// </summary>
    /// <param name="id">The unique identifier of the category</param>
    /// <returns>The category with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get a specific blog category by slug
    /// </summary>
    /// <param name="slug">The unique slug of the category</param>
    /// <returns>The category with the specified slug</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var result = await _categoryService.GetCategoryBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new blog category (requires authentication)
    /// </summary>
    /// <param name="dto">The category creation data</param>
    /// <returns>The newly created category</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateBlogCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetCategoryById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing blog category (requires admin authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the category to update</param>
    /// <param name="dto">The category update data</param>
    /// <returns>The updated category</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiResponse<BlogCategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateBlogCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a blog category (requires admin authentication)
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete</param>
    /// <returns>Success status</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
