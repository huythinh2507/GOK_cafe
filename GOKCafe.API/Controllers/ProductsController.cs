using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages product operations in the GOK Cafe system
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Products API")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products with pagination and filters
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The number of items per page (default: 10)</param>
    /// <param name="categoryIds">Optional list of category IDs to filter by</param>
    /// <param name="isFeatured">Optional filter for featured products</param>
    /// <param name="search">Optional search term for product name or description</param>
    /// <param name="flavourProfileIds">Optional list of flavour profile IDs to filter by</param>
    /// <param name="equipmentIds">Optional list of equipment IDs to filter by</param>
    /// <param name="inStock">Optional filter for stock availability (true = in stock, false = out of stock)</param>
    /// <returns>A paginated list of products matching the filters</returns>
    [HttpGet]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] List<Guid>? categoryIds = null,
        [FromQuery] bool? isFeatured = null,
        [FromQuery] string? search = null,
        [FromQuery] List<Guid>? flavourProfileIds = null,
        [FromQuery] List<Guid>? equipmentIds = null,
        [FromQuery] bool? inStock = null)
    {
        var result = await _productService.GetProductsAsync(
            pageNumber,
            pageSize,
            categoryIds,
            isFeatured,
            search,
            flavourProfileIds,
            equipmentIds,
            inStock);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product filter metadata including available flavour profiles, equipment, and stock availability counts
    /// </summary>
    /// <returns>Product filter metadata</returns>
    [HttpGet("filters")]
    [ProducesResponseType<ApiResponse<ProductFiltersDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductFiltersDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductFilters()
    {
        var result = await _productService.GetProductFiltersAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product with the specified ID</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="dto">The product creation data</param>
    /// <returns>The newly created product</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetProductById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="dto">The product update data</param>
    /// <returns>The updated product</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<ProductDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
