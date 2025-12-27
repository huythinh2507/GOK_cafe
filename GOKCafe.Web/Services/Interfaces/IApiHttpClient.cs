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
            string? search = null,
            List<Guid>? flavourProfileIds = null,
            List<Guid>? equipmentIds = null,
            bool? inStock = null);

        Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id);

        Task<ApiResponse<ProductFiltersDto>> GetProductFiltersAsync();

        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();

        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id);

        // Product Comments
        Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetProductCommentsAsync(
            Guid productId,
            ProductCommentFilterDto filter);

        Task<ApiResponse<ProductCommentSummaryDto>> GetProductCommentSummaryAsync(Guid productId);
    }
}
