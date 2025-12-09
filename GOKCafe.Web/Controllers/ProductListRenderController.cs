using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    public class ProductListController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductListController> _logger;

        public ProductListController(
            ILogger<ProductListController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IProductService productService,
            ICategoryService categoryService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public override IActionResult Index()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            try
            {
                // Get query parameters
                var pageNumber = int.TryParse(Request.Query["page"], out var page) ? page : 1;
                var categoryIds = Request.Query["category"].Where(x => x != null).Select(x => x!).ToList();
                var searchTerm = Request.Query["search"].ToString();

                // Get filter parameters
                var flavourProfileIds = Request.Query["flavour"].Where(x => x != null).Select(x => x!).ToList();
                var equipmentIds = Request.Query["equipment"].Where(x => x != null).Select(x => x!).ToList();
                bool? inStock = Request.Query["inStock"].ToString() switch
                {
                    "true" => true,
                    "false" => false,
                    _ => null
                };

                var pageSize = 12; // Default page size

                _logger.LogInformation($"ProductListController: Fetching products - Page: {pageNumber}, PageSize: {pageSize}, Categories: {categoryIds.Count}, Search: {searchTerm}, Flavours: {flavourProfileIds.Count}, Equipment: {equipmentIds.Count}, InStock: {inStock}");

                // Get products with pagination and filters - run in parallel
                var productsTask = _productService.GetProductsAsync(pageNumber, pageSize, categoryIds, searchTerm, flavourProfileIds, equipmentIds, inStock);
                var categoriesTask = _categoryService.GetAllCategoriesAsync();
                var filtersTask = _productService.GetProductFiltersAsync();

                // Wait for all tasks to complete in parallel
                Task.WaitAll(productsTask, categoriesTask, filtersTask);

                var products = productsTask.Result;
                var categories = categoriesTask.Result.ToList();
                var filters = filtersTask.Result;

                _logger.LogInformation($"ProductListController: Got {products.Items.Count()} products, {categories.Count} categories");

                // Pass data to view via ViewData
                ViewData["Products"] = products;
                ViewData["Categories"] = categories;
                ViewData["Filters"] = filters;
                ViewData["SelectedCategoryIds"] = categoryIds;
                ViewData["SearchTerm"] = searchTerm;
                ViewData["SelectedFlavourIds"] = flavourProfileIds;
                ViewData["SelectedEquipmentIds"] = equipmentIds;
                ViewData["SelectedInStock"] = inStock;

                // Return CurrentTemplate with CurrentPage
                return CurrentTemplate(CurrentPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductListController.Index");

                // Return empty data so page doesn't crash
                ViewData["Products"] = new PaginatedResponse<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    PageNumber = 1,
                    PageSize = 12,
                    TotalItems = 0,
                    TotalPages = 0
                };
                ViewData["Categories"] = new List<CategoryDto>();

                return CurrentTemplate(CurrentPage);
            }
        }
    }
}
