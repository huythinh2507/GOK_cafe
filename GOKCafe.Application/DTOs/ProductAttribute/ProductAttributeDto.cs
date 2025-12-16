namespace GOKCafe.Application.DTOs.ProductAttribute;

public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public Guid ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultipleSelection { get; set; }
    public bool IsActive { get; set; }
    public int ValueCount { get; set; }
}

public class CreateProductAttributeDto
{
    public Guid ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultipleSelection { get; set; }
}

public class UpdateProductAttributeDto
{
    public Guid ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultipleSelection { get; set; }
    public bool IsActive { get; set; }
}
