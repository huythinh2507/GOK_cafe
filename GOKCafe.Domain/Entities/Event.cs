namespace GOKCafe.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Slug { get; set; } = string.Empty;

    // Date & Time
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public string? EventTime { get; set; }

    // Location
    public string City { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? MapUrl { get; set; }

    // Pricing
    public decimal Price { get; set; }
    public string? Currency { get; set; } = "VND";

    // Media
    public string? FeaturedImageUrl { get; set; }
    public string? GalleryImages { get; set; } // JSON array of image URLs

    // Capacity & Registration
    public int? MaxCapacity { get; set; }
    public int RegisteredCount { get; set; } = 0;
    public bool IsRegistrationOpen { get; set; } = true;
    public DateTime? RegistrationDeadline { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Completed, Cancelled

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }

    // Display
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    public ICollection<EventReview> Reviews { get; set; } = new List<EventReview>();
    public ICollection<EventHighlight> Highlights { get; set; } = new List<EventHighlight>();
}
