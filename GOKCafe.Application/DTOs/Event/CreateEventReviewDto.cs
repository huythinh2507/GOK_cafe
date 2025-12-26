using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Event;

public class CreateEventReviewDto
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ReviewerName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(100)]
    public string? ReviewerEmail { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
