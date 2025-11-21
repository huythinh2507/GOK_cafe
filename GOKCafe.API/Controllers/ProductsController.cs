using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] List<Guid>? categoryIds = null,
        [FromQuery] bool? isFeatured = null,
        [FromQuery] string? search = null)
    {
        var result = await _productService.GetProductsAsync(pageNumber, pageSize, categoryIds, isFeatured, search);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetProductById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
