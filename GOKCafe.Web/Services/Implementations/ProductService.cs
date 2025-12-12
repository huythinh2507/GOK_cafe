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

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8)
        {
            try
            {
                var response = await _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: count,
                    isFeatured: true
                );

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

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var response = await _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: 1000 // Get all products
                );

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

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            try
            {
                var response = await _apiClient.GetProductsAsync(
                    pageNumber: 1,
                    pageSize: 1000,
                    categoryIds: new List<Guid> { categoryId }
                );

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

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            try
            {
                var response = await _apiClient.GetProductByIdAsync(id);

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

        public async Task<ProductDto?> GetProductBySlugAsync(string slug)
        {
            try
            {
                // API doesn't have slug endpoint, so get all and filter
                var allProducts = await GetAllProductsAsync();
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

        public async Task<PaginatedResponse<ProductDto>> GetProductsAsync(
            int pageNumber = 1,
            int pageSize = 12,
            List<string>? categoryIds = null,
            string? searchTerm = null,
            List<string>? flavourProfileIds = null,
            List<string>? equipmentIds = null,
            bool? inStock = null)
        {
            try
            {
                List<Guid>? categoryGuids = null;
                if (categoryIds != null && categoryIds.Any())
                {
                    categoryGuids = categoryIds
                        .Where(id => Guid.TryParse(id, out _))
                        .Select(id => Guid.Parse(id))
                        .ToList();
                }

                List<Guid>? flavourGuids = null;
                if (flavourProfileIds != null && flavourProfileIds.Any())
                {
                    flavourGuids = flavourProfileIds
                        .Where(id => Guid.TryParse(id, out _))
                        .Select(id => Guid.Parse(id))
                        .ToList();
                }

                List<Guid>? equipmentGuids = null;
                if (equipmentIds != null && equipmentIds.Any())
                {
                    equipmentGuids = equipmentIds
                        .Where(id => Guid.TryParse(id, out _))
                        .Select(id => Guid.Parse(id))
                        .ToList();
                }

                var response = await _apiClient.GetProductsAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    categoryIds: categoryGuids,
                    search: searchTerm,
                    flavourProfileIds: flavourGuids,
                    equipmentIds: equipmentGuids,
                    inStock: inStock
                );

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

        public async Task<ProductFiltersDto?> GetProductFiltersAsync()
        {
            try
            {
                var response = await _apiClient.GetProductFiltersAsync();

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("Failed to get product filters from API: {Message}", response.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product filters");
                return null;
            }
        }
    }
}
