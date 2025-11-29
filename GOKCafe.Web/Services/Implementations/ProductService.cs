using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace GOKCafe.Web.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger<ProductService> logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        public IEnumerable<ProductDto> GetFeaturedProducts(int count = 8)
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return Enumerable.Empty<ProductDto>();

            var root = umbracoContext.Content?.GetAtRoot().FirstOrDefault();
            if (root == null)
                return Enumerable.Empty<ProductDto>();

            var products = root
                .DescendantsOfType("productInformation")
                .Where(x => x.Value<bool>("isActive") && x.Value<bool>("isFeatured"))
                .OrderBy(x => x.Value<int>("displayOrder"))
                .Take(count)
                .Select(MapToProductDto)
                .ToList();

            return products ?? Enumerable.Empty<ProductDto>();
        }

        public IEnumerable<ProductDto> GetAllProducts()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return Enumerable.Empty<ProductDto>();

            var root = umbracoContext.Content?.GetAtRoot().FirstOrDefault();
            if (root == null)
                return Enumerable.Empty<ProductDto>();

            var products = root
                .DescendantsOfType("productInformation")
                .Where(x => x.Value<bool>("isActive"))
                .OrderBy(x => x.Value<int>("displayOrder"))
                .Select(MapToProductDto)
                .ToList();

            return products ?? Enumerable.Empty<ProductDto>();
        }

        public IEnumerable<ProductDto> GetProductsByCategory(Guid categoryId)
        {
            var allProducts = GetAllProducts();
            return allProducts.Where(p => p.CategoryId == categoryId);
        }

        public ProductDto? GetProductById(Guid id)
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return null;

            var product = umbracoContext.Content?.GetById(id);
            return product != null ? MapToProductDto(product) : null;
        }

        public ProductDto? GetProductBySlug(string slug)
        {
            var allProducts = GetAllProducts();
            return allProducts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }

        public PaginatedResponse<ProductDto> GetProducts(
            int pageNumber = 1,
            int pageSize = 12,
            string? categoryId = null,
            string? searchTerm = null)
        {
            var allProducts = GetAllProducts().AsEnumerable();

            // Filter by category
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId))
            {
                allProducts = allProducts.Where(p => p.CategoryId == catId);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allProducts = allProducts.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.ShortDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = allProducts.Count();
            var items = allProducts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResponse<ProductDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        private ProductDto MapToProductDto(IPublishedContent product)
        {
            // Use productImage property for main image
            var imageUrl = product.Value<IPublishedContent>("productImage")?.Url() ?? string.Empty;

            // Get category - can be a content picker or dropdown
            var categoryContent = product.Value<IPublishedContent>("productCategory");
            var categoryString = product.Value<string>("productCategory") ?? string.Empty;

            // Get gallery images if any
            var galleryImages = product.Value<IEnumerable<IPublishedContent>>("productImages")?
                .Select(img => img.Url() ?? string.Empty)
                .Where(url => !string.IsNullOrEmpty(url))
                .ToList() ?? new List<string>();

            return new ProductDto
            {
                Id = product.Key,
                Name = product.Value<string>("productName") ?? product.Name ?? string.Empty,
                Slug = product.UrlSegment ?? string.Empty,
                Description = product.Value<string>("description") ?? string.Empty,
                ShortDescription = product.Value<string>("shortDescription") ?? string.Empty,
                Price = product.Value<decimal>("priceMin"),
                DiscountPrice = product.Value<decimal?>("priceMax"),
                ImageUrl = imageUrl,
                ProductImages = galleryImages,
                StockQuantity = product.Value<int>("stockQuantity"),
                IsActive = product.Value<bool>("isActive"),
                IsFeatured = product.Value<bool>("isFeatured"),
                CategoryId = categoryContent?.Key ?? Guid.Empty,
                CategoryName = categoryContent?.Name ?? categoryString,
                DisplayOrder = product.Value<int>("displayOrder"),
                CreatedDate = product.CreateDate,
                UpdatedDate = product.UpdateDate,
                Region = product.Value<string>("region") ?? string.Empty,
                Process = product.Value<string>("process") ?? string.Empty,
                TastingNote = product.Value<string>("tastingNote") ?? string.Empty
            };
        }
    }
}
