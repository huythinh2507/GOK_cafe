using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers.Api
{
    [Route("api/test/products")]
    public class TestProductsController : UmbracoApiController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public TestProductsController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Test endpoint to verify direct database access
        /// GET: /api/test/products
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _productService.GetProductsAsync(
                pageNumber: 1,
                pageSize: 10,
                isFeatured: true
            );

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                errors = result.Errors,
                productCount = result.Data?.Items.Count ?? 0,
                totalItems = result.Data?.TotalItems ?? 0,
                products = result.Data?.Items
            });
        }

        /// <summary>
        /// Test endpoint to get all products (not just featured)
        /// GET: /api/test/products/all
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _productService.GetProductsAsync(
                pageNumber: 1,
                pageSize: 100
            );

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                productCount = result.Data?.Items.Count ?? 0,
                totalItems = result.Data?.TotalItems ?? 0,
                products = result.Data?.Items
            });
        }

        /// <summary>
        /// Test endpoint to get all categories
        /// GET: /api/test/products/categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                categoryCount = result.Data?.Count ?? 0,
                categories = result.Data
            });
        }
    }
}
