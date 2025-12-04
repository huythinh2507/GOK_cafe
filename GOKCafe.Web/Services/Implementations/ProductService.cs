using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;

namespace GOKCafe.Web.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IApiHttpClient _apiClient;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IApiHttpClient apiClient,
            ILogger<ProductService> _logger)
        {
            _apiClient = apiClient;
            this._logger = _logger;
        }

        public IEnumerable<ProductDto> GetFeaturedProducts(int count = 8)
        {
            try
            {
                var response = _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: count,
                    isFeatured: true
                ).GetAwaiter().GetResult();

                if (response.Success && response.Data != null)
                {
                    return response.Data.Items;
                }

                _logger.LogWarning("Failed to get featured products from API: {Message}", response.Message);
                return Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return Enumerable.Empty<ProductDto>();
            }
        }

        public IEnumerable<ProductDto> GetAllProducts()
        {
            try
            {
                var response = _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: 1000 // Get all products
                ).GetAwaiter().GetResult();

                if (response.Success && response.Data != null)
                {
                    return response.Data.Items;
                }

                _logger.LogWarning("Failed to get all products from API: {Message}", response.Message);
                return Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return Enumerable.Empty<ProductDto>();
            }
        }

        public IEnumerable<ProductDto> GetProductsByCategory(Guid categoryId)
        {
            try
            {
                var response = _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: 1000,
                    categoryIds: new List<Guid> { categoryId }
                ).GetAwaiter().GetResult();

                if (response.Success && response.Data != null)
                {
                    return response.Data.Items;
                }

                _logger.LogWarning("Failed to get products by category from API: {Message}", response.Message);
                return Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                return Enumerable.Empty<ProductDto>();
            }
        }

        public ProductDto? GetProductById(Guid id)
        {
            try
            {
                var response = _apiClient.GetProductByIdAsync(id).GetAwaiter().GetResult();

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("Failed to get product {ProductId} from API: {Message}", id, response.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return null;
            }
        }

        public ProductDto? GetProductBySlug(string slug)
        {
            try
            {
                // API doesn't have slug endpoint, so get all and filter
                var allProducts = GetAllProducts();
                return allProducts.FirstOrDefault(p =>
                    p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Replace(" ", "-").Equals(slug, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by slug {Slug}", slug);
                return null;
            }
        }

        public PaginatedResponse<ProductDto> GetProducts(
            int pageNumber = 1,
            int pageSize = 12,
            string? categoryId = null,
            string? searchTerm = null)
        {
            try
            {
                List<Guid>? categoryIds = null;
                if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId))
                {
                    categoryIds = new List<Guid> { catId };
                }

                var response = _apiClient.GetProductsAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    categoryIds: categoryIds,
                    search: searchTerm
                ).GetAwaiter().GetResult();

                if (response.Success && response.Data != null)
                {
                    return new PaginatedResponse<ProductDto>
                    {
                        Items = response.Data.Items,
                        PageNumber = response.Data.PageNumber,
                        PageSize = response.Data.PageSize,
                        TotalItems = response.Data.TotalItems,
                        TotalPages = response.Data.TotalPages
                    };
                }

                _logger.LogWarning("Failed to get products from API: {Message}", response.Message);
                return new PaginatedResponse<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated products");
                return new PaginatedResponse<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }
    }
}
