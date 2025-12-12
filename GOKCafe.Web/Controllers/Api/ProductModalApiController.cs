using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers.Api
{
    [Route("umbraco/api/[controller]")]
    public class ProductModalApiController : UmbracoApiController
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductModalApiController> _logger;

        public ProductModalApiController(
            IProductService productService,
            ILogger<ProductModalApiController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = product.Id,
                        name = product.Name,
                        description = product.Description,
                        shortDescription = product.ShortDescription,
                        price = product.Price,
                        discountPrice = product.DiscountPrice,
                        imageUrl = product.ImageUrl,
                        categoryName = product.CategoryName,
                        tastingNote = product.TastingNote,
                        region = product.Region,
                        process = product.Process,
                        stockQuantity = product.StockQuantity,
                        isActive = product.IsActive,
                        slug = product.Slug,
                        availableSizes = product.AvailableSizes ?? new List<string>(),
                        availableGrinds = product.AvailableGrinds ?? new List<string>()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
