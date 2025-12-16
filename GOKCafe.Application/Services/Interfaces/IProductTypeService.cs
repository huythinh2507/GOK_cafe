using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductType;

namespace GOKCafe.Application.Services.Interfaces;

public interface IProductTypeService
{
    Task<ApiResponse<List<ProductTypeDto>>> GetAllProductTypesAsync();
    Task<ApiResponse<ProductTypeDto>> GetProductTypeByIdAsync(Guid id);
    Task<ApiResponse<ProductTypeDto>> GetProductTypeBySlugAsync(string slug);
    Task<ApiResponse<ProductTypeWithAttributesDto>> GetProductTypeWithAttributesAsync(Guid id);
    Task<ApiResponse<ProductTypeDto>> CreateProductTypeAsync(CreateProductTypeDto dto);
    Task<ApiResponse<ProductTypeDto>> UpdateProductTypeAsync(Guid id, UpdateProductTypeDto dto);
    Task<ApiResponse<bool>> DeleteProductTypeAsync(Guid id);
}
