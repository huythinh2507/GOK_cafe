using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Event;

public class CreateEventRegistrationDto
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Range(1, 10)]
    public int NumberOfAttendees { get; set; } = 1;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? SpecialRequirements { get; set; }
}
