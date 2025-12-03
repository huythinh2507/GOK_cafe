using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Api
{
    /// <summary>
    /// Manages product operations in the GOK Cafe Web system
    /// </summary>
    [Route("api/v1/products")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Products API")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductApiController> _logger;

        public ProductApiController(
            IProductService productService,
            ILogger<ProductApiController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1)</param>
        /// <param name="pageSize">The number of items per page (default: 12)</param>
        /// <param name="categoryId">Optional category ID to filter products</param>
        /// <param name="searchTerm">Optional search term for product name or description</param>
        /// <returns>A paginated list of products matching the filters</returns>
        [HttpGet]
        [ProducesResponseType<ApiResponse<PaginatedResponse<ProductDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? categoryId = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = _productService.GetProducts(pageNumber, pageSize, categoryId, searchTerm);
                return Ok(ApiResponse<PaginatedResponse<ProductDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching products",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        /// <param name="count">The number of featured products to return (default: 8)</param>
        /// <returns>A list of featured products</returns>
        [HttpGet("featured")]
        [ProducesResponseType<ApiResponse<IEnumerable<ProductDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFeaturedProducts([FromQuery] int count = 8)
        {
            try
            {
                var products = _productService.GetFeaturedProducts(count);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching featured products",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">The unique identifier of the product</param>
        /// <returns>The product with the specified ID</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProductById(Guid id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                    return NotFound(ApiResponse<object>.FailureResult("Product not found"));

                return Ok(ApiResponse<ProductDto>.SuccessResult(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching the product",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get product by slug
        /// </summary>
        /// <param name="slug">The URL-friendly slug of the product</param>
        /// <returns>The product with the specified slug</returns>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProductBySlug(string slug)
        {
            try
            {
                var product = _productService.GetProductBySlug(slug);
                if (product == null)
                    return NotFound(ApiResponse<object>.FailureResult("Product not found"));

                return Ok(ApiResponse<ProductDto>.SuccessResult(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by slug {Slug}", slug);
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching the product",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category</param>
        /// <returns>A list of products in the specified category</returns>
        [HttpGet("category/{categoryId:guid}")]
        [ProducesResponseType<ApiResponse<IEnumerable<ProductDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProductsByCategory(Guid categoryId)
        {
            try
            {
                var products = _productService.GetProductsByCategory(categoryId);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for category {CategoryId}", categoryId);
                return StatusCode(500, ApiResponse<object>.FailureResult(
                    "An error occurred while fetching products",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}
