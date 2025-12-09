using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    public class CategoryController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public CategoryController(
            ILogger<CategoryController> logger,
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

            // Get category and products in parallel
            var categoryTask = _categoryService.GetCategoryByIdAsync(currentPage.Key);

            // Get query parameters
            var pageNumber = int.TryParse(Request.Query["page"], out var page) ? page : 1;
            var pageSize = 12;

            Task.WaitAll(categoryTask);
            var category = categoryTask.Result;

            if (category == null)
                return NotFound();

            // Get products for this category
            var productsTask = _productService.GetProductsAsync(pageNumber, pageSize, category.Id.ToString());
            Task.WaitAll(productsTask);
            var products = productsTask.Result;

            var viewModel = new CategoryViewModel
            {
                Category = category,
                Products = products
            };

            return View("~/Views/Category.cshtml", viewModel);
        }
    }
}
