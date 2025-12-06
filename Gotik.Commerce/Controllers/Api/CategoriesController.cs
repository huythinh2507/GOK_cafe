using Microsoft.AspNetCore.Http;
using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gotik.Commerce.Controllers.Api;

/// <summary>
/// Manages category operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Categories API")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>A list of all categories</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<List<CategoryDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<CategoryDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">The unique identifier of the category</param>
    /// <returns>The category with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the category</param>
    /// <returns>The category with the specified slug</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var result = await _categoryService.GetCategoryBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="dto">The category creation data</param>
    /// <returns>The newly created category</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetCategoryById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">The unique identifier of the category to update</param>
    /// <param name="dto">The category update data</param>
    /// <returns>The updated category</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
