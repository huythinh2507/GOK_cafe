namespace GOKCafe.Application.DTOs.ProductType;

/// <summary>
/// DTO for syncing all attributes of a product type
/// Supports bulk create, update, and delete operations
/// </summary>
public class SyncProductTypeAttributesDto
{
    public List<AttributeItemDto> Attributes { get; set; } = new();
}

/// <summary>
/// Represents a single attribute in the sync operation
/// </summary>
public class AttributeItemDto
{
    /// <summary>
    /// Attribute ID (null = create new, has value = update existing)
    /// </summary>
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid ProductTypeId { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultipleSelection { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Attribute values (for dropdown type attributes)
    /// </summary>
    public List<AttributeValueItemDto> Values { get; set; } = new();
}

/// <summary>
/// Represents a single attribute value in the sync operation
/// </summary>
public class AttributeValueItemDto
{
    /// <summary>
    /// Value ID (null = create new, has value = update existing)
    /// </summary>
    public Guid? Id { get; set; }

    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
