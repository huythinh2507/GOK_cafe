namespace GOKCafe.Application.DTOs.Partner;

public class PartnerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
