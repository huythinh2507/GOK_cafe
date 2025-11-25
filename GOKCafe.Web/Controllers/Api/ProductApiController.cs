using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Api
{
    [Route("api/products")]
    [ApiController]
    public class ProductApiController : Controller
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
        [HttpGet]
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
        [HttpGet("featured")]
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
        [HttpGet("{id:guid}")]
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
        [HttpGet("slug/{slug}")]
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
        [HttpGet("category/{categoryId:guid}")]
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
