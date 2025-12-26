namespace GOKCafe.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? RegisteredPhoneNumber { get; set; }

    // Legacy fields for backward compatibility
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Message { get; set; }

    // Admin management fields
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public string? Status { get; set; } = "Pending"; // Pending, InProgress, Resolved, Closed
}
