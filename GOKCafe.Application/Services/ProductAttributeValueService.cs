using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductAttributeValue;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class ProductAttributeValueService : IProductAttributeValueService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductAttributeValueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<ProductAttributeValueDto>>> GetAllProductAttributeValuesAsync()
    {
        try
        {
            var values = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .ToListAsync();

            var valueDtos = values
                .Where(pav => pav.IsActive)
                .OrderBy(pav => pav.ProductAttributeId)
                .ThenBy(pav => pav.DisplayOrder)
                .Select(pav => MapToDto(pav))
                .ToList();

            return ApiResponse<List<ProductAttributeValueDto>>.SuccessResult(valueDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductAttributeValueDto>>.FailureResult(
                "An error occurred while retrieving product attribute values",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<List<ProductAttributeValueDto>>> GetValuesByAttributeIdAsync(Guid productAttributeId)
    {
        try
        {
            var values = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .Where(pav => pav.ProductAttributeId == productAttributeId && pav.IsActive)
                .OrderBy(pav => pav.DisplayOrder)
                .ToListAsync();

            var valueDtos = values.Select(pav => MapToDto(pav)).ToList();

            return ApiResponse<List<ProductAttributeValueDto>>.SuccessResult(valueDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductAttributeValueDto>>.FailureResult(
                "An error occurred while retrieving product attribute values",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeValueDto>> GetProductAttributeValueByIdAsync(Guid id)
    {
        try
        {
            var value = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .FirstOrDefaultAsync(pav => pav.Id == id);

            if (value == null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("Product attribute value not found");

            var valueDto = MapToDto(value);
            return ApiResponse<ProductAttributeValueDto>.SuccessResult(valueDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeValueDto>.FailureResult(
                "An error occurred while retrieving the product attribute value",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeValueDto>> CreateProductAttributeValueAsync(CreateProductAttributeValueDto dto)
    {
        try
        {
            // Verify product attribute exists
            var productAttribute = await _unitOfWork.ProductAttributes.GetByIdAsync(dto.ProductAttributeId);
            if (productAttribute == null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("Product attribute not found");

            // Check for duplicate value
            var existingValue = await _unitOfWork.ProductAttributeValues
                .FirstOrDefaultAsync(pav => pav.ProductAttributeId == dto.ProductAttributeId && pav.Value == dto.Value);
            if (existingValue != null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("This value already exists for the attribute");

            var value = new ProductAttributeValue
            {
                Id = Guid.NewGuid(),
                ProductAttributeId = dto.ProductAttributeId,
                Value = dto.Value,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };

            await _unitOfWork.ProductAttributeValues.AddAsync(value);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            value = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .FirstOrDefaultAsync(pav => pav.Id == value.Id);

            var valueDto = MapToDto(value!);
            return ApiResponse<ProductAttributeValueDto>.SuccessResult(valueDto, "Product attribute value created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeValueDto>.FailureResult(
                "An error occurred while creating the product attribute value",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<List<ProductAttributeValueDto>>> BulkCreateProductAttributeValuesAsync(BulkCreateProductAttributeValuesDto dto)
    {
        try
        {
            // Verify product attribute exists
            var productAttribute = await _unitOfWork.ProductAttributes.GetByIdAsync(dto.ProductAttributeId);
            if (productAttribute == null)
                return ApiResponse<List<ProductAttributeValueDto>>.FailureResult("Product attribute not found");

            // Get existing values to avoid duplicates
            var existingValues = await _unitOfWork.ProductAttributeValues
                .FindAsync(pav => pav.ProductAttributeId == dto.ProductAttributeId);
            var existingValueSet = new HashSet<string>(existingValues.Select(ev => ev.Value), StringComparer.OrdinalIgnoreCase);

            // Get max display order
            var maxDisplayOrder = existingValues.Any() ? existingValues.Max(ev => ev.DisplayOrder) : 0;

            var newValues = new List<ProductAttributeValue>();
            foreach (var value in dto.Values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                if (!existingValueSet.Contains(value.Trim()))
                {
                    maxDisplayOrder++;
                    newValues.Add(new ProductAttributeValue
                    {
                        Id = Guid.NewGuid(),
                        ProductAttributeId = dto.ProductAttributeId,
                        Value = value.Trim(),
                        DisplayOrder = maxDisplayOrder,
                        IsActive = true
                    });
                }
            }

            if (newValues.Any())
            {
                await _unitOfWork.ProductAttributeValues.AddRangeAsync(newValues);
                await _unitOfWork.SaveChangesAsync();
            }

            // Reload with navigation properties
            var createdValues = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .Where(pav => newValues.Select(nv => nv.Id).Contains(pav.Id))
                .ToListAsync();

            var valueDtos = createdValues.Select(v => MapToDto(v)).ToList();
            return ApiResponse<List<ProductAttributeValueDto>>.SuccessResult(valueDtos, $"{newValues.Count} product attribute value(s) created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductAttributeValueDto>>.FailureResult(
                "An error occurred while creating product attribute values",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductAttributeValueDto>> UpdateProductAttributeValueAsync(Guid id, UpdateProductAttributeValueDto dto)
    {
        try
        {
            var value = await _unitOfWork.ProductAttributeValues.GetByIdAsync(id);
            if (value == null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("Product attribute value not found");

            // Verify product attribute exists
            var productAttribute = await _unitOfWork.ProductAttributes.GetByIdAsync(dto.ProductAttributeId);
            if (productAttribute == null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("Product attribute not found");

            // Check for duplicate value
            var existingValue = await _unitOfWork.ProductAttributeValues
                .FirstOrDefaultAsync(pav => pav.ProductAttributeId == dto.ProductAttributeId && pav.Value == dto.Value && pav.Id != id);
            if (existingValue != null)
                return ApiResponse<ProductAttributeValueDto>.FailureResult("This value already exists for the attribute");

            value.ProductAttributeId = dto.ProductAttributeId;
            value.Value = dto.Value;
            value.DisplayOrder = dto.DisplayOrder;
            value.IsActive = dto.IsActive;

            _unitOfWork.ProductAttributeValues.Update(value);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            value = await _unitOfWork.ProductAttributeValues.GetQueryable()
                .Include(pav => pav.ProductAttribute)
                .FirstOrDefaultAsync(pav => pav.Id == id);

            var valueDto = MapToDto(value!);
            return ApiResponse<ProductAttributeValueDto>.SuccessResult(valueDto, "Product attribute value updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductAttributeValueDto>.FailureResult(
                "An error occurred while updating the product attribute value",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAttributeValueAsync(Guid id)
    {
        try
        {
            var value = await _unitOfWork.ProductAttributeValues.GetByIdAsync(id);
            if (value == null)
                return ApiResponse<bool>.FailureResult("Product attribute value not found");

            // Check for dependencies (product selections)
            var hasSelections = await _unitOfWork.ProductAttributeSelections
                .AnyAsync(pas => pas.ProductAttributeValueId == id);
            if (hasSelections)
                return ApiResponse<bool>.FailureResult("Cannot delete product attribute value with existing product selections");

            _unitOfWork.ProductAttributeValues.SoftDelete(value);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Product attribute value deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deleting the product attribute value",
                new List<string> { ex.Message });
        }
    }

    private ProductAttributeValueDto MapToDto(ProductAttributeValue value)
    {
        return new ProductAttributeValueDto
        {
            Id = value.Id,
            ProductAttributeId = value.ProductAttributeId,
            ProductAttributeName = value.ProductAttribute?.Name ?? string.Empty,
            Value = value.Value,
            DisplayOrder = value.DisplayOrder,
            IsActive = value.IsActive
        };
    }
}
