using GOKCafe.Web.Models.ViewModels;
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
        }

        public override IActionResult Index()
        {
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            // Get query parameters
            var pageNumber = int.TryParse(Request.Query["page"], out var page) ? page : 1;
            var categoryId = Request.Query["category"].ToString();
            var searchTerm = Request.Query["search"].ToString();

            var pageSize = currentPage.Value<int>("productsPerPage");
            if (pageSize == 0) pageSize = 12;

            // Get products with pagination
            var products = _productService.GetProducts(pageNumber, pageSize, categoryId, searchTerm);
            var categories = _categoryService.GetAllCategories().ToList();

            var viewModel = new ProductListViewModel
            {
                PageTitle = currentPage.Value<string>("pageTitle") ?? currentPage.Name,
                Introduction = currentPage.Value<string>("introduction") ?? string.Empty,
                Products = products,
                Categories = categories,
                SelectedCategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId)
                    ? catId
                    : null,
                SearchTerm = searchTerm
            };

            return View("~/Views/ProductList.cshtml", viewModel);
        }
    }
}
