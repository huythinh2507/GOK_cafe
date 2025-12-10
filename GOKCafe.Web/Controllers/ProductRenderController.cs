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

            // Try to get product ID from query string first
            var productIdParam = Request.Query["id"].ToString();
            Guid productId;

            if (!string.IsNullOrEmpty(productIdParam) && Guid.TryParse(productIdParam, out var parsedId))
            {
                // Use product ID from query string
                productId = parsedId;
            }
            else
            {
                // Check if current page has productInformation as child
                var productInfoNode = currentPage.Children()?
                    .FirstOrDefault(x => x.ContentType.Alias == "productInformation");

                if (productInfoNode != null)
                {
                    productId = productInfoNode.Key;
                }
                else
                {
                    productId = currentPage.Key;
                }
            }

            // Get product async
            var productTask = _productService.GetProductByIdAsync(productId);
            Task.WaitAll(productTask);
            product = productTask.Result;

            if (product == null)
                return NotFound();

            // Get category async if product has one
            Models.DTOs.CategoryDto? category = null;
            if (product.CategoryId != Guid.Empty)
            {
                var categoryTask = _categoryService.GetCategoryByIdAsync(product.CategoryId);
                Task.WaitAll(categoryTask);
                category = categoryTask.Result;
            }

            // Get recommended products from Umbraco content tree or from API
            var relatedProducts = new List<Models.DTOs.ProductDto>();
            var recommendProductNodes = currentPage.Children()?.Where(x => x.ContentType.Alias == "recommendProduct");

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
            else
            {
                // Get related products from API (same category)
                if (product.CategoryId != Guid.Empty)
                {
                    var relatedProductsTask = _productService.GetProductsAsync(
                        pageNumber: 1,
                        pageSize: 4,
                        categoryIds: new List<string> { product.CategoryId.ToString() }
                    );
                    Task.WaitAll(relatedProductsTask);
                    var relatedProductsResponse = relatedProductsTask.Result;

                    if (relatedProductsResponse?.Items != null)
                    {
                        relatedProducts = relatedProductsResponse.Items
                            .Where(p => p.Id != product.Id) // Exclude current product
                            .Take(4)
                            .ToList();
                    }
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
