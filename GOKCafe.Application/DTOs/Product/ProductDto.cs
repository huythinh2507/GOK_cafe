using GOKCafe.Application.DTOs.Equipment;
using GOKCafe.Application.DTOs.FlavourProfile;

namespace GOKCafe.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = new();
    public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
    public List<EquipmentDto> Equipments { get; set; } = new();
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}
