using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    public class ProductController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            ILogger<ProductController> logger,
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
            // This is for individual product detail page
            var currentPage = CurrentPage;
            if (currentPage == null)
                return NotFound();

            var product = _productService.GetProductById(currentPage.Key);
            if (product == null)
                return NotFound();

            var category = product.CategoryId != Guid.Empty
                ? _categoryService.GetCategoryById(product.CategoryId)
                : null;

            // Get related products from same category
            var relatedProducts = product.CategoryId != Guid.Empty
                ? _productService.GetProductsByCategory(product.CategoryId)
                    .Where(p => p.Id != product.Id)
                    .Take(4)
                    .ToList()
                : new List<Models.DTOs.ProductDto>();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Category = category,
                RelatedProducts = relatedProducts
            };

            return View("~/Views/Product.cshtml", viewModel);
        }
    }
}
