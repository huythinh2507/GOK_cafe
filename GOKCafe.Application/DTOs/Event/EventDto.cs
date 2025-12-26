namespace GOKCafe.Application.DTOs.Event;

public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public string? EventTime { get; set; }
    public string City { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? MapUrl { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public List<string>? GalleryImages { get; set; }
    public int? MaxCapacity { get; set; }
    public int RegisteredCount { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AvailableSpots => MaxCapacity.HasValue ? Math.Max(0, MaxCapacity.Value - RegisteredCount) : int.MaxValue;
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
