using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductType;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class ProductTypeService : IProductTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<ProductTypeDto>>> GetAllProductTypesAsync()
    {
        try
        {
            var productTypes = await _unitOfWork.ProductTypes.GetAllAsync();
            var productTypeDtos = productTypes
                .Where(pt => pt.IsActive)
                .OrderBy(pt => pt.DisplayOrder)
                .Select(pt => MapToDto(pt))
                .ToList();

            return ApiResponse<List<ProductTypeDto>>.SuccessResult(productTypeDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductTypeDto>>.FailureResult(
                "An error occurred while retrieving product types",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductTypeDto>> GetProductTypeByIdAsync(Guid id)
    {
        try
        {
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(id);
            if (productType == null)
                return ApiResponse<ProductTypeDto>.FailureResult("Product type not found");

            var productTypeDto = MapToDto(productType);
            return ApiResponse<ProductTypeDto>.SuccessResult(productTypeDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductTypeDto>.FailureResult(
                "An error occurred while retrieving the product type",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductTypeDto>> GetProductTypeBySlugAsync(string slug)
    {
        try
        {
            var productType = await _unitOfWork.ProductTypes.FirstOrDefaultAsync(pt => pt.Slug == slug);
            if (productType == null)
                return ApiResponse<ProductTypeDto>.FailureResult("Product type not found");

            var productTypeDto = MapToDto(productType);
            return ApiResponse<ProductTypeDto>.SuccessResult(productTypeDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductTypeDto>.FailureResult(
                "An error occurred while retrieving the product type",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductTypeWithAttributesDto>> GetProductTypeWithAttributesAsync(Guid id)
    {
        try
        {
            var productType = await _unitOfWork.ProductTypes.GetQueryable()
                .Include(pt => pt.ProductAttributes.Where(pa => pa.IsActive))
                    .ThenInclude(pa => pa.AttributeValues.Where(pav => pav.IsActive))
                .FirstOrDefaultAsync(pt => pt.Id == id);

            if (productType == null)
                return ApiResponse<ProductTypeWithAttributesDto>.FailureResult("Product type not found");

            var dto = new ProductTypeWithAttributesDto
            {
                Id = productType.Id,
                Name = productType.Name,
                Description = productType.Description,
                Slug = productType.Slug,
                DisplayOrder = productType.DisplayOrder,
                IsActive = productType.IsActive,
                Attributes = productType.ProductAttributes
                    .OrderBy(pa => pa.DisplayOrder)
                    .Select(pa => new ProductAttributeWithValuesDto
                    {
                        Id = pa.Id,
                        Name = pa.Name,
                        DisplayName = pa.DisplayName,
                        Description = pa.Description,
                        DisplayOrder = pa.DisplayOrder,
                        IsRequired = pa.IsRequired,
                        AllowMultipleSelection = pa.AllowMultipleSelection,
                        IsActive = pa.IsActive,
                        Values = pa.AttributeValues
                            .OrderBy(pav => pav.DisplayOrder)
                            .Select(pav => new ProductAttributeValueSimpleDto
                            {
                                Id = pav.Id,
                                Value = pav.Value,
                                DisplayOrder = pav.DisplayOrder,
                                IsActive = pav.IsActive
                            }).ToList()
                    }).ToList()
            };

            return ApiResponse<ProductTypeWithAttributesDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductTypeWithAttributesDto>.FailureResult(
                "An error occurred while retrieving the product type with attributes",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductTypeDto>> CreateProductTypeAsync(CreateProductTypeDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            var existingProductType = await _unitOfWork.ProductTypes.FirstOrDefaultAsync(pt => pt.Slug == slug);
            if (existingProductType != null)
                return ApiResponse<ProductTypeDto>.FailureResult("A product type with this name already exists");

            var productType = new ProductType
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };

            await _unitOfWork.ProductTypes.AddAsync(productType);
            await _unitOfWork.SaveChangesAsync();

            var productTypeDto = MapToDto(productType);
            return ApiResponse<ProductTypeDto>.SuccessResult(productTypeDto, "Product type created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductTypeDto>.FailureResult(
                "An error occurred while creating the product type",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductTypeDto>> UpdateProductTypeAsync(Guid id, UpdateProductTypeDto dto)
    {
        try
        {
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(id);
            if (productType == null)
                return ApiResponse<ProductTypeDto>.FailureResult("Product type not found");

            var slug = GenerateSlug(dto.Name);
            var existingProductType = await _unitOfWork.ProductTypes
                .FirstOrDefaultAsync(pt => pt.Slug == slug && pt.Id != id);
            if (existingProductType != null)
                return ApiResponse<ProductTypeDto>.FailureResult("A product type with this name already exists");

            productType.Name = dto.Name;
            productType.Description = dto.Description;
            productType.Slug = slug;
            productType.DisplayOrder = dto.DisplayOrder;
            productType.IsActive = dto.IsActive;

            _unitOfWork.ProductTypes.Update(productType);
            await _unitOfWork.SaveChangesAsync();

            var productTypeDto = MapToDto(productType);
            return ApiResponse<ProductTypeDto>.SuccessResult(productTypeDto, "Product type updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductTypeDto>.FailureResult(
                "An error occurred while updating the product type",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductTypeAsync(Guid id)
    {
        try
        {
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(id);
            if (productType == null)
                return ApiResponse<bool>.FailureResult("Product type not found");

            // Check for dependencies
            var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.ProductTypeId == id);
            if (hasProducts)
                return ApiResponse<bool>.FailureResult("Cannot delete product type with existing products");

            _unitOfWork.ProductTypes.SoftDelete(productType);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Product type deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deleting the product type",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> SyncProductTypeAttributesAsync(Guid productTypeId, SyncProductTypeAttributesDto dto)
    {
        try
        {
            // 1. Verify ProductType exists
            var productType = await _unitOfWork.ProductTypes.GetByIdAsync(productTypeId);
            if (productType == null)
                return ApiResponse<bool>.FailureResult("Product type not found");

            // 2. Get existing attributes with values
            var existingAttributes = await _unitOfWork.ProductAttributes.GetQueryable()
                .Include(pa => pa.AttributeValues)
                .Where(pa => pa.ProductTypeId == productTypeId)
                .ToListAsync();

            // 3. Determine which attributes to delete (existing but not in new list)
            var incomingIds = dto.Attributes
                .Where(a => a.Id.HasValue)
                .Select(a => a.Id.Value)
                .ToHashSet();

            var attributesToDelete = existingAttributes
                .Where(a => !incomingIds.Contains(a.Id))
                .ToList();

            // 4. Delete removed attributes (soft delete)
            foreach (var attr in attributesToDelete)
            {
                _unitOfWork.ProductAttributes.SoftDelete(attr);
            }

            // 5. Create or Update attributes
            foreach (var attrDto in dto.Attributes)
            {
                ProductAttribute attribute;

                if (attrDto.Id.HasValue)
                {
                    // UPDATE existing
                    attribute = existingAttributes.FirstOrDefault(a => a.Id == attrDto.Id.Value);
                    if (attribute == null) continue;

                    attribute.Name = attrDto.Name;
                    attribute.DisplayName = attrDto.DisplayName;
                    attribute.IsRequired = attrDto.IsRequired;
                    attribute.AllowMultipleSelection = attrDto.AllowMultipleSelection;
                    attribute.DisplayOrder = attrDto.DisplayOrder;

                    _unitOfWork.ProductAttributes.Update(attribute);
                }
                else
                {
                    // CREATE new
                    attribute = new ProductAttribute
                    {
                        Id = Guid.NewGuid(),
                        ProductTypeId = productTypeId,
                        Name = attrDto.Name,
                        DisplayName = attrDto.DisplayName,
                        IsRequired = attrDto.IsRequired,
                        AllowMultipleSelection = attrDto.AllowMultipleSelection,
                        DisplayOrder = attrDto.DisplayOrder,
                        IsActive = true
                    };

                    await _unitOfWork.ProductAttributes.AddAsync(attribute);
                }

                // 6. Sync attribute values
                await SyncAttributeValuesAsync(attribute.Id, attrDto.Values, existingAttributes);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResult(true, "Attributes synced successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while syncing attributes",
                new List<string> { ex.Message });
        }
    }

    private async Task SyncAttributeValuesAsync(Guid attributeId, List<AttributeValueItemDto> valueDtos, List<ProductAttribute> existingAttributes)
    {
        // Get existing values for this attribute
        var existingAttribute = existingAttributes.FirstOrDefault(a => a.Id == attributeId);
        var existingValues = existingAttribute?.AttributeValues.ToList() ?? new List<ProductAttributeValue>();

        // Determine which values to delete
        var incomingValueIds = valueDtos
            .Where(v => v.Id.HasValue)
            .Select(v => v.Id.Value)
            .ToHashSet();

        var valuesToDelete = existingValues
            .Where(v => !incomingValueIds.Contains(v.Id))
            .ToList();

        // Delete removed values (soft delete)
        foreach (var val in valuesToDelete)
        {
            _unitOfWork.ProductAttributeValues.SoftDelete(val);
        }

        // Create or update values
        foreach (var valDto in valueDtos)
        {
            if (valDto.Id.HasValue)
            {
                // UPDATE existing
                var existingVal = existingValues.FirstOrDefault(v => v.Id == valDto.Id.Value);
                if (existingVal != null)
                {
                    existingVal.Value = valDto.Value;
                    existingVal.DisplayOrder = valDto.DisplayOrder;
                    _unitOfWork.ProductAttributeValues.Update(existingVal);
                }
            }
            else
            {
                // CREATE new
                var newValue = new ProductAttributeValue
                {
                    Id = Guid.NewGuid(),
                    ProductAttributeId = attributeId,
                    Value = valDto.Value,
                    DisplayOrder = valDto.DisplayOrder,
                    IsActive = true
                };
                await _unitOfWork.ProductAttributeValues.AddAsync(newValue);
            }
        }
    }

    private ProductTypeDto MapToDto(ProductType productType)
    {
        return new ProductTypeDto
        {
            Id = productType.Id,
            Name = productType.Name,
            Description = productType.Description,
            Slug = productType.Slug,
            DisplayOrder = productType.DisplayOrder,
            IsActive = productType.IsActive,
            ProductCount = productType.Products?.Count ?? 0,
            AttributeCount = productType.ProductAttributes?.Count ?? 0
        };
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Trim();
    }
}
