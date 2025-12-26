using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Partner;

public class UpdatePartnerDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string LogoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
