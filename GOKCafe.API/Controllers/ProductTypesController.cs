using GOKCafe.Application.DTOs.ProductType;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages product type operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Product Configuration API")]
public class ProductTypesController : ControllerBase
{
    private readonly IProductTypeService _productTypeService;

    public ProductTypesController(IProductTypeService productTypeService)
    {
        _productTypeService = productTypeService;
    }

    /// <summary>
    /// Get all product types
    /// </summary>
    /// <returns>A list of all product types</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<List<ProductTypeDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<List<ProductTypeDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductTypes()
    {
        var result = await _productTypeService.GetAllProductTypesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product type by ID
    /// </summary>
    /// <param name="id">The unique identifier of the product type</param>
    /// <returns>The product type with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductTypeById(Guid id)
    {
        var result = await _productTypeService.GetProductTypeByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get product type by slug
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the product type</param>
    /// <returns>The product type with the specified slug</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductTypeBySlug(string slug)
    {
        var result = await _productTypeService.GetProductTypeBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get product type with all attributes and values (for dynamic form rendering)
    /// </summary>
    /// <param name="id">The unique identifier of the product type</param>
    /// <returns>The product type with nested attributes and values</returns>
    [HttpGet("{id:guid}/attributes-with-values")]
    [ProducesResponseType<ApiResponse<ProductTypeWithAttributesDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductTypeWithAttributesDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductTypeWithAttributes(Guid id)
    {
        var result = await _productTypeService.GetProductTypeWithAttributesAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new product type
    /// </summary>
    /// <param name="dto">The product type creation data</param>
    /// <returns>The newly created product type</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductType([FromBody] CreateProductTypeDto dto)
    {
        var result = await _productTypeService.CreateProductTypeAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetProductTypeById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing product type
    /// </summary>
    /// <param name="id">The unique identifier of the product type to update</param>
    /// <param name="dto">The product type update data</param>
    /// <returns>The updated product type</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ProductTypeDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductType(Guid id, [FromBody] UpdateProductTypeDto dto)
    {
        var result = await _productTypeService.UpdateProductTypeAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a product type
    /// </summary>
    /// <param name="id">The unique identifier of the product type to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductType(Guid id)
    {
        var result = await _productTypeService.DeleteProductTypeAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Sync all attributes for a product type (bulk create/update/delete)
    /// </summary>
    /// <param name="id">The unique identifier of the product type</param>
    /// <param name="dto">The sync data containing all attributes and values</param>
    /// <returns>Success status</returns>
    [HttpPut("{id:guid}/attributes")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncProductTypeAttributes(Guid id, [FromBody] SyncProductTypeAttributesDto dto)
    {
        var result = await _productTypeService.SyncProductTypeAttributesAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
