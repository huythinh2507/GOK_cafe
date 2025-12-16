using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductAttribute;

namespace GOKCafe.Application.Services.Interfaces;

public interface IProductAttributeService
{
    Task<ApiResponse<List<ProductAttributeDto>>> GetAllProductAttributesAsync();
    Task<ApiResponse<List<ProductAttributeDto>>> GetAttributesByProductTypeIdAsync(Guid productTypeId);
    Task<ApiResponse<ProductAttributeDto>> GetProductAttributeByIdAsync(Guid id);
    Task<ApiResponse<ProductAttributeDto>> CreateProductAttributeAsync(CreateProductAttributeDto dto);
    Task<ApiResponse<ProductAttributeDto>> UpdateProductAttributeAsync(Guid id, UpdateProductAttributeDto dto);
    Task<ApiResponse<bool>> DeleteProductAttributeAsync(Guid id);
}
