namespace GOKCafe.Application.DTOs.Odoo;

/// <summary>
/// DTO for Odoo product data
/// </summary>
public class OdooProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public bool Active { get; set; }

    /// <summary>
    /// Product variant attributes from Odoo (e.g., Size: "M", Color: "Blue")
    /// Key: Attribute name, Value: Attribute value
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class OdooSimpleProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Raw attribute value IDs from Odoo product.template.attribute.value
    /// These will be resolved to actual attribute name-value pairs
    /// </summary>
    public List<int>? ProductTemplateAttributeValueIds { get; set; }

    /// <summary>
    /// Category information [id, name]
    /// </summary>
    public object? CategId { get; set; }
}

/// <summary>
/// DTO for Odoo product.template.attribute.value data
/// Used to resolve attribute value IDs to actual attribute names and values
/// </summary>
public class OdooAttributeValueDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The attribute this value belongs to [id, name]
    /// Example: [1, "Size"] or [2, "Color"]
    /// </summary>
    public object? AttributeId { get; set; }

    /// <summary>
    /// The actual value name [id, name]
    /// Example: [10, "Medium"] or [20, "Blue"]
    /// </summary>
    public object? ProductAttributeValueId { get; set; }
}
