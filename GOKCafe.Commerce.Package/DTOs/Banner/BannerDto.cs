using GOKCafe.Application.DTOs.Product;
using GOKCafe.Domain.Entities;

namespace GOKCafe.Application.DTOs.Banner;

public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public Guid? ProductId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public BannerType Type { get; set; }
    public ProductDto? Product { get; set; }
}
