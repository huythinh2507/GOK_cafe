namespace GOKCafe.Web.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ProductTypeId { get; set; }

        // Additional product detail properties
        public string? TastingNote { get; set; }
        public string? Region { get; set; }
        public string? Process { get; set; }

        // Properties from API response
        public List<ProductImageDto> Images { get; set; } = new();
        public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
        public List<EquipmentDto> Equipments { get; set; } = new();

        // Available product options
        public List<string>? AvailableSizes { get; set; }
        public List<string>? AvailableGrinds { get; set; }
        public List<string>? AvailableColors { get; set; }
        public string? Material { get; set; }
        public string? Style { get; set; }

        // Legacy/deprecated properties (kept for backward compatibility)
        public List<string> ProductImages { get; set; } = new();
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class FlavourProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class EquipmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ProductFiltersDto
    {
        public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
        public List<EquipmentDto> Equipments { get; set; } = new();
        public AvailabilityDto Availability { get; set; } = new();
    }

    public class AvailabilityDto
    {
        public int InStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}
