using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    // Controller for 'homepage' document type
    // Umbraco naming: [DocumentTypeAlias]Controller
    public class HomepageController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<HomepageController> _logger;

        public HomepageController(
            ILogger<HomepageController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IProductService productService,
            ICategoryService categoryService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _productService = productService;
            _categoryService = categoryService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        public override IActionResult Index()
        {
            var homepage = CurrentPage;
            if (homepage == null)
            {
                return NotFound();
            }

            // Get dynamic data
            var featuredCount = homepage.Value<int>("featuredProductsCount");
            if (featuredCount == 0) featuredCount = 8;

            try
            {
                _logger.LogInformation($"[Homepage] Fetching {featuredCount} featured products");

                // Call directly to Application layer service (no HTTP API)
                var productsResponse = _productService.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: featuredCount,
                    isFeatured: true
                ).GetAwaiter().GetResult();

                _logger.LogInformation($"[Homepage] Products Response - Success: {productsResponse.Success}, Count: {productsResponse.Data?.Items.Count ?? 0}");

                var categoriesResponse = _categoryService.GetAllCategoriesAsync()
                    .GetAwaiter().GetResult();

                // Extract data from API responses
                var featuredProducts = productsResponse.Success && productsResponse.Data != null
                    ? productsResponse.Data.Items.ToList()
                    : new List<ProductDto>();

                var categories = categoriesResponse.Success && categoriesResponse.Data != null
                    ? categoriesResponse.Data.ToList()
                    : new List<GOKCafe.Application.DTOs.Category.CategoryDto>();

                // Debug info
                var debugMsg = $"Success: {productsResponse.Success}, Message: {productsResponse.Message}, Count: {featuredProducts.Count}";
                var errorMsg = productsResponse.Errors != null && productsResponse.Errors.Any()
                    ? string.Join(", ", productsResponse.Errors)
                    : "No errors";

                ViewData["DebugInfo"] = debugMsg;
                ViewData["ErrorInfo"] = errorMsg;

                _logger.LogInformation($"[Homepage] Debug: {debugMsg}");
                if (errorMsg != "No errors")
                {
                    _logger.LogError($"[Homepage] Errors: {errorMsg}");
                }

                // Pass data to view via ViewData so view can access it
                ViewData["FeaturedProducts"] = featuredProducts;
                ViewData["Categories"] = categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Homepage] Exception occurred while fetching data");
                ViewData["DebugInfo"] = $"Exception: {ex.Message}";
                ViewData["ErrorInfo"] = ex.ToString();
                ViewData["FeaturedProducts"] = new List<ProductDto>();
                ViewData["Categories"] = new List<GOKCafe.Application.DTOs.Category.CategoryDto>();
            }

            // Return CurrentTemplate with CurrentPage for Umbraco routing
            return CurrentTemplate(CurrentPage);
        }
    }
}
