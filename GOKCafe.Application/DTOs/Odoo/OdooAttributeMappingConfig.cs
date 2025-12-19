namespace GOKCafe.Application.DTOs.Odoo;

/// <summary>
/// Configuration for mapping Odoo categories and attributes to ProductType system
/// </summary>
public class OdooAttributeMappingConfig
{
    /// <summary>
    /// Default ProductType name to use if no mapping is found
    /// </summary>
    public string DefaultProductType { get; set; } = "General";

    /// <summary>
    /// Maps Odoo category names to ProductType names
    /// Example: { "Beverages": "Coffee", "Clothing": "Clothes" }
    /// </summary>
    public Dictionary<string, string> CategoryToProductTypeMap { get; set; } = new();

    /// <summary>
    /// Maps Odoo attribute names to ProductAttribute names for each ProductType
    /// Example: { "Coffee": { "Weight": "size", "Grind": "grind" } }
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> AttributeMapping { get; set; } = new();

    /// <summary>
    /// Whether to enable automatic attribute mapping from Odoo
    /// </summary>
    public bool EnableAutoMapping { get; set; } = true;
}
