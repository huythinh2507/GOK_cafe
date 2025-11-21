namespace GOKCafe.Application.DTOs.InfoCard;

public class InfoCardDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
