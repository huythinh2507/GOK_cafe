using GOKCafe.Application.DTOs.Common;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductAttributeValuesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductAttributeValuesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttributeValue([FromBody] CreateProductAttributeValueRequest request)
    {
        var attributeValue = new ProductAttributeValue
        {
            Id = Guid.NewGuid(),
            ProductAttributeId = request.ProductAttributeId,
            Value = request.Value,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductAttributeValues.AddAsync(attributeValue);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Product attribute value created successfully",
            Data = new { attributeValue.Id, attributeValue.Value }
        });
    }
}

public class CreateProductAttributeValueRequest
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
