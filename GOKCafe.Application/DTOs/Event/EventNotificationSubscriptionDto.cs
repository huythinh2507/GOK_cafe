using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Event;

public class EventNotificationSubscriptionDto
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }
}
