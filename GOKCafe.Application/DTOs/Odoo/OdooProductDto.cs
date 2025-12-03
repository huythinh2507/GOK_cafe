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
}

public class OdooSimpleProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
