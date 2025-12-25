using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Event;

public class UpdateEventDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    public DateTime? EventEndDate { get; set; }

    [MaxLength(50)]
    public string? EventTime { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Venue { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    [Required]
    public decimal Price { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    public List<string>? GalleryImages { get; set; }

    public int? MaxCapacity { get; set; }

    public bool IsRegistrationOpen { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int DisplayOrder { get; set; }
}
