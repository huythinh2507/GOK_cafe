using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Product;

namespace GOKCafe.Application.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(int pageNumber, int pageSize, List<Guid>? categoryIds = null, bool? isFeatured = null, string? search = null);
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id);
}
