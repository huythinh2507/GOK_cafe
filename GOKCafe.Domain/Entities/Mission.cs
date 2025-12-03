namespace GOKCafe.Domain.Entities;

public class Mission : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; } // Image or Video URL
    public MediaType MediaType { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum MediaType
{
    Image,
    Video
}
