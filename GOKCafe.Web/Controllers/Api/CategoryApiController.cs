using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Api
{
    /// <summary>
    /// Manages category operations in the GOK Cafe Web system
    /// </summary>
    [Route("api/v1/categories")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Categories API")]
    public class CategoryApiController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryApiController> _logger;

        public CategoryApiController(
            ICategoryService categoryService,
            ILogger<CategoryApiController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>A list of all categories</returns>
        [HttpGet]
        [ProducesResponseType<ApiResponse<IEnumerable<CategoryDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching categories",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">The unique identifier of the category</param>
        /// <returns>The category with the specified ID</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<object>.FailureResult("Category not found"));

                return Ok(ApiResponse<CategoryDto>.SuccessResult(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching the category",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get category by slug
        /// </summary>
        /// <param name="slug">The URL-friendly slug of the category</param>
        /// <returns>The category with the specified slug</returns>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            try
            {
                var category = await _categoryService.GetCategoryBySlugAsync(slug);
                if (category == null)
                    return NotFound(ApiResponse<object>.FailureResult("Category not found"));

                return Ok(ApiResponse<CategoryDto>.SuccessResult(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching the category",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}
