using GOKCafe.Domain.Entities;

namespace GOKCafe.Application.DTOs.Mission;

public class MissionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public MediaType MediaType { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
