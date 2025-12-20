using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductAttributeValue;

namespace GOKCafe.Application.Services.Interfaces;

public interface IProductAttributeValueService
{
    Task<ApiResponse<List<ProductAttributeValueDto>>> GetAllProductAttributeValuesAsync();
    Task<ApiResponse<List<ProductAttributeValueDto>>> GetValuesByAttributeIdAsync(Guid productAttributeId);
    Task<ApiResponse<ProductAttributeValueDto>> GetProductAttributeValueByIdAsync(Guid id);
    Task<ApiResponse<ProductAttributeValueDto>> CreateProductAttributeValueAsync(CreateProductAttributeValueDto dto);
    Task<ApiResponse<List<ProductAttributeValueDto>>> BulkCreateProductAttributeValuesAsync(BulkCreateProductAttributeValuesDto dto);
    Task<ApiResponse<ProductAttributeValueDto>> UpdateProductAttributeValueAsync(Guid id, UpdateProductAttributeValueDto dto);
    Task<ApiResponse<bool>> DeleteProductAttributeValueAsync(Guid id);
}
