using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Api
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryApiController : Controller
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
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
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
        [HttpGet("{id:guid}")]
        public IActionResult GetCategoryById(Guid id)
        {
            try
            {
                var category = _categoryService.GetCategoryById(id);
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
        [HttpGet("slug/{slug}")]
        public IActionResult GetCategoryBySlug(string slug)
        {
            try
            {
                var category = _categoryService.GetCategoryBySlug(slug);
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
