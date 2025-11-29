using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace GOKCafe.Web.Controllers
{
    public class ProductDetailController : RenderController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductDetailController(
            ILogger<ProductDetailController> logger,
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

            Models.DTOs.ProductDto? product = null;

            // Check if current page has productInformation as child
            var productInfoNode = currentPage.Children?
                .FirstOrDefault(x => x.ContentType.Alias == "productInformation");

            if (productInfoNode != null)
            {
                // Get product from child node
                product = _productService.GetProductById(productInfoNode.Key);
            }
            else
            {
                // Try to get product from current page (in case we're on the productInformation node)
                product = _productService.GetProductById(currentPage.Key);
            }

            if (product == null)
                return NotFound();

            var category = product.CategoryId != Guid.Empty
                ? _categoryService.GetCategoryById(product.CategoryId)
                : null;

            // Get recommended products from Umbraco content tree
            var relatedProducts = new List<Models.DTOs.ProductDto>();
            var recommendProductNodes = currentPage.Children?.Where(x => x.ContentType.Alias == "recommendProduct");

            if (recommendProductNodes != null && recommendProductNodes.Any())
            {
                foreach (var recommendNode in recommendProductNodes.Take(4))
                {
                    // RecommendProduct has its own properties: productName, price, productImage
                    var productName = recommendNode.Value<string>("productName") ?? recommendNode.Name;
                    var price = recommendNode.Value<decimal>("price");
                    var productImage = recommendNode.Value<Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent>("productImage");
                    var imageUrl = productImage?.Url() ?? string.Empty;

                    // Create a ProductDto from RecommendProduct data
                    var recommendedProduct = new Models.DTOs.ProductDto
                    {
                        Id = recommendNode.Key,
                        Name = productName,
                        Price = price,
                        ImageUrl = imageUrl,
                        Slug = recommendNode.UrlSegment ?? string.Empty
                    };

                    relatedProducts.Add(recommendedProduct);
                }
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Category = category,
                RelatedProducts = relatedProducts
            };

            ViewData["ProductDetailViewModel"] = viewModel;

            // Return CurrentTemplate with the current page model, not our custom viewModel
            return CurrentTemplate(CurrentPage);
        }
    }
}
