namespace GOKCafe.Domain.Entities;

public class InfoCard : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
