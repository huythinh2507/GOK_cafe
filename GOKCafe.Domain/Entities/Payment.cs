namespace GOKCafe.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? QRCodeData { get; set; }
    public string? QRCodeImageUrl { get; set; }
    public string? BankCode { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? PaymentDescription { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? PaymentGatewayResponse { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}

public class BankTransferConfig : BaseEntity
{
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankBranch { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? LogoUrl { get; set; }
}
