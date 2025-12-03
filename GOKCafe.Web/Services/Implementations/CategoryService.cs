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

        public IEnumerable<CategoryDto> GetAllCategories()
        {
            try
            {
                var response = _apiClient.GetCategoriesAsync().GetAwaiter().GetResult();

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

        public CategoryDto? GetCategoryById(Guid id)
        {
            try
            {
                var response = _apiClient.GetCategoryByIdAsync(id).GetAwaiter().GetResult();

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

        public CategoryDto? GetCategoryBySlug(string slug)
        {
            try
            {
                // API doesn't have slug endpoint, so get all and filter
                var allCategories = GetAllCategories();
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
