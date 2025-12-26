using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Contact;

public class CreateContactMessageDto
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? RegisteredPhoneNumber { get; set; }
}
