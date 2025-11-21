namespace GOKCafe.Domain.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public Guid? ProductId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public BannerType Type { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}

public enum BannerType
{
    Hero,
    Featured,
    Promotional
}
