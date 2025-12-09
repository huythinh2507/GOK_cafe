using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;

namespace GOKCafe.Web.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IApiHttpClient _apiClient;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IApiHttpClient apiClient,
            ILogger<CategoryService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var response = await _apiClient.GetCategoriesAsync();

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("Failed to get categories from API: {Message}", response.Message);
                return Enumerable.Empty<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return Enumerable.Empty<CategoryDto>();
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var response = await _apiClient.GetCategoryByIdAsync(id);

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("Failed to get category {CategoryId} from API: {Message}", id, response.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return null;
            }
        }

        public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
        {
            try
            {
                // API doesn't have slug endpoint, so get all and filter
                var allCategories = await GetAllCategoriesAsync();
                return allCategories.FirstOrDefault(c =>
                    c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                    c.Name.Replace(" ", "-").Equals(slug, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
                return null;
            }
        }
    }
}
