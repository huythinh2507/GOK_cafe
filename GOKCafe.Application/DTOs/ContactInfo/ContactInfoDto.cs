namespace GOKCafe.Application.DTOs.ContactInfo;

public class ContactInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WorkingHours { get; set; }
    public string? ImageUrl { get; set; }
    public string? MapUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public bool IsActive { get; set; }
}
