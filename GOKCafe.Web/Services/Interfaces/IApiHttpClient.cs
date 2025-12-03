using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface IApiHttpClient
    {
        Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            List<Guid>? categoryIds = null,
            bool? isFeatured = null,
            string? search = null);

        Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id);

        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();

        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id);
    }
}
