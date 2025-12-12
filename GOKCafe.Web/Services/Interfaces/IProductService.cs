using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<ProductDto?> GetProductBySlugAsync(string slug);
        Task<PaginatedResponse<ProductDto>> GetProductsAsync(
            int pageNumber = 1,
            int pageSize = 12,
            List<string>? categoryIds = null,
            string? searchTerm = null,
            List<string>? flavourProfileIds = null,
            List<string>? equipmentIds = null,
            bool? inStock = null);
        Task<ProductFiltersDto?> GetProductFiltersAsync();
    }
}
