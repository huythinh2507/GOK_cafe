using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace GOKCafe.Web.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger<CategoryService> logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        public IEnumerable<CategoryDto> GetAllCategories()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return Enumerable.Empty<CategoryDto>();

            var root = umbracoContext.Content?.GetAtRoot().FirstOrDefault();
            if (root == null)
                return Enumerable.Empty<CategoryDto>();

            var categories = root
                .DescendantsOfType("category")
                .Where(x => x.Value<bool>("isActive"))
                .OrderBy(x => x.Value<int>("displayOrder"))
                .Select(MapToCategoryDto)
                .ToList();

            return categories ?? Enumerable.Empty<CategoryDto>();
        }

        public CategoryDto? GetCategoryById(Guid id)
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                return null;

            var category = umbracoContext.Content?.GetById(id);
            return category != null ? MapToCategoryDto(category) : null;
        }

        public CategoryDto? GetCategoryBySlug(string slug)
        {
            var allCategories = GetAllCategories();
            return allCategories.FirstOrDefault(c => c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }

        private CategoryDto MapToCategoryDto(IPublishedContent category)
        {
            var imageUrl = category.Value<IPublishedContent>("imageUrl")?.Url() ?? string.Empty;

            // Count products in this category
            var productCount = 0;
            if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                var root = umbracoContext.Content?.GetAtRoot().FirstOrDefault();
                if (root != null)
                {
                    productCount = root
                        .DescendantsOfType("product")
                        .Count(p => p.Value<IPublishedContent>("category")?.Key == category.Key);
                }
            }

            return new CategoryDto
            {
                Id = category.Key,
                Name = category.Value<string>("name") ?? string.Empty,
                Slug = category.Value<string>("slug") ?? string.Empty,
                Description = category.Value<string>("description") ?? string.Empty,
                ImageUrl = imageUrl,
                DisplayOrder = category.Value<int>("displayOrder"),
                IsActive = category.Value<bool>("isActive"),
                ProductCount = productCount,
                CreatedDate = category.CreateDate
            };
        }
    }
}
