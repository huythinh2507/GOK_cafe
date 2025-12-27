using GOKCafe.Web.Models.ViewModels;
using GOKCafe.Web.Services.Interfaces;
using GOKCafe.Web.Models.DTOs;
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
        private readonly Services.Interfaces.IBreadcrumbService _breadcrumbService;
        private readonly IProductCommentService _commentService;
        private readonly ILogger<ProductDetailController> _logger;

        public ProductDetailController(
            ILogger<ProductDetailController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IProductService productService,
            ICategoryService categoryService,
            Services.Interfaces.IBreadcrumbService breadcrumbService,
            IProductCommentService commentService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _productService = productService;
            _categoryService = categoryService;
            _breadcrumbService = breadcrumbService;
            _commentService = commentService;
            _logger = logger;
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

            // Build dynamic breadcrumbs using BreadcrumbService
            var breadcrumbs = _breadcrumbService.BuildProductDetailBreadcrumbs(
                currentPage,
                product.Name,
                category?.Name);

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Category = category,
                RelatedProducts = relatedProducts,
                Breadcrumbs = breadcrumbs
            };

            ViewData["ProductDetailViewModel"] = viewModel;

            // Load product comments
            LoadProductComments(product.Id);

            // Return CurrentTemplate with the current page model, not our custom viewModel
            return CurrentTemplate(CurrentPage);
        }

        private void LoadProductComments(Guid productId)
        {
            try
            {
                // Get query parameters for filtering
                var pageNumber = int.TryParse(Request.Query["page"], out var page) ? page : 1;
                var pageSize = 5;
                var ratings = Request.Query["ratings"].Where(x => x != null).Select(x => int.Parse(x!)).ToList();
                var hasReplies = Request.Query["hasReplies"].ToString() == "true" ? (bool?)true : null;
                var hasImages = Request.Query["hasImages"].ToString() == "true" ? (bool?)true : null;
                var search = Request.Query["search"].ToString();

                var filter = new ProductCommentFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsApproved = true,
                    Ratings = ratings.Any() ? ratings : null,
                    HasReplies = hasReplies,
                    HasImages = hasImages,
                    Search = string.IsNullOrWhiteSpace(search) ? null : search
                };

                // Fetch comments and summary in parallel
                var commentsTask = _commentService.GetProductCommentsAsync(productId, filter);
                var summaryTask = _commentService.GetProductCommentSummaryAsync(productId);

                Task.WaitAll(commentsTask, summaryTask);

                var comments = commentsTask.Result;
                var summary = summaryTask.Result;

                // Pass data to view via ViewData
                ViewData["ProductComments"] = comments;
                ViewData["CommentSummary"] = summary;
                ViewData["SelectedRatings"] = ratings;
                ViewData["SearchTerm"] = search;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product comments for product {ProductId}", productId);

                // Provide empty data so page doesn't crash
                ViewData["ProductComments"] = new PaginatedResponse<ProductCommentDto>
                {
                    Items = new List<ProductCommentDto>(),
                    PageNumber = 1,
                    PageSize = 5,
                    TotalItems = 0,
                    TotalPages = 0
                };
                ViewData["CommentSummary"] = new ProductCommentSummaryDto
                {
                    TotalComments = 0,
                    AverageRating = 0,
                    RatingDistribution = new Dictionary<int, int>()
                };
                ViewData["SelectedRatings"] = new List<int>();
                ViewData["SearchTerm"] = string.Empty;
            }
        }
    }
}
