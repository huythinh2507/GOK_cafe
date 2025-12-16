using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductAttribute;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductAttributeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<ProductAttributeDto>>> GetAllProductAttributesAsync()
    {
        try
        {
            var attributes = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.ProductType)
                .Include(pa => pa.AttributeValues)
                .ToListAsync();

            var attributeDtos = attributes
                .Where(pa => pa.IsActive)
                .OrderBy(pa => pa.ProductTypeId)
                .ThenBy(pa => pa.DisplayOrder)
                .Select(pa => MapToDto(pa))
                .ToList();

            return ApiResponse<List<ProductAttributeDto>>.SuccessResult(attributeDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductAttributeDto>>.FailureResult(
                "An error occurred while retrieving product attributes",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<List<ProductAttributeDto>>> GetAttributesByProductTypeIdAsync(Guid productTypeId)
    {
        try
        {
            var attributes = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.ProductType)
                .Include(pa => pa.AttributeValues)
                .Where(pa => pa.ProductTypeId == productTypeId && pa.IsActive)
                .OrderBy(pa => pa.DisplayOrder)
                .ToListAsync();

            var attributeDtos = attributes.Select(pa => MapToDto(pa)).ToList();

            return ApiResponse<List<ProductAttributeDto>>.SuccessResult(attributeDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductAttributeDto>>.FailureResult(
                "An error occurred while retrieving product attributes",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeDto>> GetProductAttributeByIdAsync(Guid id)
    {
        try
        {
            var attribute = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.ProductType)
                .Include(pa => pa.AttributeValues)
                .FirstOrDefaultAsync(pa => pa.Id == id);

            if (attribute == null)
                return ApiResponse<ProductAttributeDto>.FailureResult("Product attribute not found");

            var attributeDto = MapToDto(attribute);
            return ApiResponse<ProductAttributeDto>.SuccessResult(attributeDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeDto>.FailureResult(
                "An error occurred while retrieving the product attribute",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeDto>> CreateProductAttributeAsync(CreateProductAttributeDto dto)
    {
        try
        {
            // Verify product type exists
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(dto.ProductTypeId);
            if (productType == null)
                return ApiResponse<ProductAttributeDto>.FailureResult("Product type not found");

            var attribute = new ProductAttribute
            {
                Id = Guid.NewGuid(),
                ProductTypeId = dto.ProductTypeId,
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                IsRequired = dto.IsRequired,
                AllowMultipleSelection = dto.AllowMultipleSelection,
                IsActive = true
            };

            await _unitOfWork.ProductAttributes.AddAsync(attribute);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            attribute = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.ProductType)
                .Include(pa => pa.AttributeValues)
                .FirstOrDefaultAsync(pa => pa.Id == attribute.Id);

            var attributeDto = MapToDto(attribute!);
            return ApiResponse<ProductAttributeDto>.SuccessResult(attributeDto, "Product attribute created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeDto>.FailureResult(
                "An error occurred while creating the product attribute",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeDto>> UpdateProductAttributeAsync(Guid id, UpdateProductAttributeDto dto)
    {
        try
        {
            var attribute = await _unitOfWork.ProductAttributes.GetByIdAsync(id);
            if (attribute == null)
                return ApiResponse<ProductAttributeDto>.FailureResult("Product attribute not found");

            // Verify product type exists
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(dto.ProductTypeId);
            if (productType == null)
                return ApiResponse<ProductAttributeDto>.FailureResult("Product type not found");

            attribute.ProductTypeId = dto.ProductTypeId;
            attribute.Name = dto.Name;
            attribute.DisplayName = dto.DisplayName;
            attribute.Description = dto.Description;
            attribute.DisplayOrder = dto.DisplayOrder;
            attribute.IsRequired = dto.IsRequired;
            attribute.AllowMultipleSelection = dto.AllowMultipleSelection;
            attribute.IsActive = dto.IsActive;

            _unitOfWork.ProductAttributes.Update(attribute);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            attribute = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.ProductType)
                .Include(pa => pa.AttributeValues)
                .FirstOrDefaultAsync(pa => pa.Id == id);

            var attributeDto = MapToDto(attribute!);
            return ApiResponse<ProductAttributeDto>.SuccessResult(attributeDto, "Product attribute updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeDto>.FailureResult(
                "An error occurred while updating the product attribute",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAttributeAsync(Guid id)
    {
        try
        {
            var attribute = await _unitOfWork.ProductAttributes.GetByIdAsync(id);
            if (attribute == null)
                return ApiResponse<bool>.FailureResult("Product attribute not found");

            // Check for dependencies (product selections)
            var hasSelections = await _unitOfWork.ProductAttributeSelections
                .AnyAsync(pas => pas.ProductAttributeId == id);
            if (hasSelections)
                return ApiResponse<bool>.FailureResult("Cannot delete product attribute with existing product selections");

            _unitOfWork.ProductAttributes.SoftDelete(attribute);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Product attribute deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deleting the product attribute",
                new List<string> { ex.Message });
        }
    }

    private ProductAttributeDto MapToDto(ProductAttribute attribute)
    {
        return new ProductAttributeDto
        {
            Id = attribute.Id,
            ProductTypeId = attribute.ProductTypeId,
            ProductTypeName = attribute.ProductType?.Name ?? string.Empty,
            Name = attribute.Name,
            DisplayName = attribute.DisplayName,
            Description = attribute.Description,
            DisplayOrder = attribute.DisplayOrder,
            IsRequired = attribute.IsRequired,
            AllowMultipleSelection = attribute.AllowMultipleSelection,
            IsActive = attribute.IsActive,
            ValueCount = attribute.AttributeValues?.Count ?? 0
        };
    }
}
