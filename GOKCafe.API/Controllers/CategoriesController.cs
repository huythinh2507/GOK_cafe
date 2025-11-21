using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var result = await _categoryService.GetCategoryBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetCategoryById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
