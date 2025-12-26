namespace GOKCafe.Application.DTOs.Contact;

public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? RegisteredPhoneNumber { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
