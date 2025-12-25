using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Contact;

public class UpdateContactMessageDto
{
    [MaxLength(1000)]
    public string? Reply { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }
}
