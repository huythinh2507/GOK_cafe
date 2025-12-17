using GOKCafe.Application.DTOs.Common;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductAttributesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductAttributesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttribute([FromBody] CreateProductAttributeRequest request)
    {
        var attribute = new ProductAttribute
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DisplayName = request.DisplayName ?? request.Name,
            Description = request.Description,
            ProductTypeId = request.ProductTypeId,
            IsRequired = request.IsRequired,
            AllowMultipleSelection = request.AllowMultipleSelection,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductAttributes.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Product attribute created successfully",
            Data = new { attribute.Id, attribute.Name, attribute.DisplayName }
        });
    }
}

public class CreateProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public Guid ProductTypeId { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultipleSelection { get; set; }
    public int DisplayOrder { get; set; }
}
