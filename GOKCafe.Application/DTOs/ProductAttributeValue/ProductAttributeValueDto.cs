namespace GOKCafe.Application.DTOs.ProductAttributeValue;

public class ProductAttributeValueDto
{
    public Guid Id { get; set; }
    public Guid ProductAttributeId { get; set; }
    public string ProductAttributeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductAttributeValueDto
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class UpdateProductAttributeValueDto
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class BulkCreateProductAttributeValuesDto
{
    public Guid ProductAttributeId { get; set; }
    public List<string> Values { get; set; } = new();
}
