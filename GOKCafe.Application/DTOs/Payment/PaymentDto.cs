using GOKCafe.Domain.Entities;

namespace GOKCafe.Application.DTOs.Payment;

public class PaymentDto
{
    public Guid Id { get; set; }
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
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? BankCode { get; set; }
}

public class CreatePaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentDto? Payment { get; set; }
    public QRCodeInfo? QRCodeInfo { get; set; }
}

public class QRCodeInfo
{
    public string QRCodeData { get; set; } = string.Empty;
    public string QRCodeImageUrl { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentDescription { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int ExpiresInMinutes { get; set; }
}

public class VerifyPaymentRequest
{
    public Guid PaymentId { get; set; }
    public string? TransactionReference { get; set; }
}

public class VerifyPaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class BankTransferConfigDto
{
    public Guid Id { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankBranch { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? LogoUrl { get; set; }
}

public class CreateBankTransferConfigDto
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
