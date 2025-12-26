namespace GOKCafe.Domain.Entities;

public class EventRegistration : BaseEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    // User info (can be guest or registered user)
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    // Guest registration details
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Registration details
    public int NumberOfAttendees { get; set; } = 1;
    public string Status { get; set; } = "Confirmed"; // Pending, Confirmed, Cancelled, Attended
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }

    // Payment
    public decimal AmountPaid { get; set; }
    public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }

    // Additional info
    public string? Notes { get; set; }
    public string? SpecialRequirements { get; set; }
}
