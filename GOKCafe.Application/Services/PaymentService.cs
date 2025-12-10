using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Payment;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQRCodeService _qrCodeService;
    private const int PaymentExpirationMinutes = 15;

    public PaymentService(IUnitOfWork unitOfWork, IQRCodeService qrCodeService)
    {
        _unitOfWork = unitOfWork;
        _qrCodeService = qrCodeService;
    }

    public async Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            // Validate order exists
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return ApiResponse<CreatePaymentResponse>.FailureResult("Order not found", new List<string> { "ORDER_NOT_FOUND" });
            }

            // Check if payment already exists for this order
            var existingPayment = await _unitOfWork.Payments
                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.Status != PaymentStatus.Failed);
            if (existingPayment != null)
            {
                return ApiResponse<CreatePaymentResponse>.FailureResult("Payment already exists for this order", new List<string> { "PAYMENT_EXISTS" });
            }

            // Generate transaction ID
            var transactionId = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{order.OrderNumber}";

            Payment payment;
            QRCodeInfo? qrCodeInfo = null;

            if (request.PaymentMethod == PaymentMethod.BankTransfer)
            {
                // Get bank config
                var bankCode = request.BankCode ?? await GetDefaultBankCodeAsync();
                var bankConfig = await _unitOfWork.BankTransferConfigs
                    .FirstOrDefaultAsync(b => b.BankCode == bankCode && b.IsActive);

                if (bankConfig == null)
                {
                    return ApiResponse<CreatePaymentResponse>.FailureResult("Bank configuration not found", new List<string> { "BANK_CONFIG_NOT_FOUND" });
                }

                // Generate payment description
                var paymentDescription = $"GOKCAFE {order.OrderNumber}";

                // Generate QR code data
                var qrData = _qrCodeService.GenerateVietQRData(
                    bankConfig.BankCode,
                    bankConfig.AccountNumber,
                    bankConfig.AccountName,
                    order.TotalAmount,
                    paymentDescription
                );

                // Generate QR code image URL
                var qrImageUrl = _qrCodeService.GenerateVietQRImageUrl(
                    bankConfig.BankCode,
                    bankConfig.AccountNumber,
                    bankConfig.AccountName,
                    order.TotalAmount,
                    paymentDescription
                );

                payment = new Payment
                {
                    OrderId = request.OrderId,
                    TransactionId = transactionId,
                    Amount = order.TotalAmount,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Pending,
                    QRCodeData = qrData,
                    QRCodeImageUrl = qrImageUrl,
                    BankCode = bankConfig.BankCode,
                    BankAccountNumber = bankConfig.AccountNumber,
                    BankAccountName = bankConfig.AccountName,
                    PaymentDescription = paymentDescription,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(PaymentExpirationMinutes)
                };

                qrCodeInfo = new QRCodeInfo
                {
                    QRCodeData = qrData,
                    QRCodeImageUrl = qrImageUrl,
                    BankName = bankConfig.BankName,
                    BankCode = bankConfig.BankCode,
                    AccountNumber = bankConfig.AccountNumber,
                    AccountName = bankConfig.AccountName,
                    Amount = order.TotalAmount,
                    PaymentDescription = paymentDescription,
                    ExpiresAt = payment.ExpiresAt,
                    ExpiresInMinutes = PaymentExpirationMinutes
                };
            }
            else
            {
                // Handle other payment methods
                payment = new Payment
                {
                    OrderId = request.OrderId,
                    TransactionId = transactionId,
                    Amount = order.TotalAmount,
                    PaymentMethod = request.PaymentMethod,
                    Status = PaymentStatus.Pending,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(PaymentExpirationMinutes)
                };
            }

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            var response = new CreatePaymentResponse
            {
                Success = true,
                Message = "Payment created successfully",
                Payment = MapToDto(payment),
                QRCodeInfo = qrCodeInfo
            };

            return ApiResponse<CreatePaymentResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<CreatePaymentResponse>.FailureResult("Error creating payment", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                return ApiResponse<PaymentDto>.FailureResult("Payment not found", new List<string> { "NOT_FOUND" });
            }

            return ApiResponse<PaymentDto>.SuccessResult(MapToDto(payment));
        }
        catch (Exception ex)
        {
            return ApiResponse<PaymentDto>.FailureResult("Error getting payment", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId)
    {
        try
        {
            var payment = await _unitOfWork.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
            {
                return ApiResponse<PaymentDto>.FailureResult("Payment not found", new List<string> { "NOT_FOUND" });
            }

            return ApiResponse<PaymentDto>.SuccessResult(MapToDto(payment));
        }
        catch (Exception ex)
        {
            return ApiResponse<PaymentDto>.FailureResult("Error getting payment", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<VerifyPaymentResponse>> VerifyPaymentAsync(VerifyPaymentRequest request)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);
            if (payment == null)
            {
                return ApiResponse<VerifyPaymentResponse>.FailureResult("Payment not found", new List<string> { "NOT_FOUND" });
            }

            // Check if payment is expired
            if (DateTime.UtcNow > payment.ExpiresAt && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Failed;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<VerifyPaymentResponse>.SuccessResult(new VerifyPaymentResponse
                {
                    Success = false,
                    Message = "Payment has expired",
                    Status = PaymentStatus.Failed
                });
            }

            // Note: In production, you would integrate with bank API to verify actual payment
            // For now, this is a manual verification endpoint

            var response = new VerifyPaymentResponse
            {
                Success = payment.Status == PaymentStatus.Paid,
                Message = payment.Status == PaymentStatus.Paid ? "Payment completed" : "Payment pending",
                Status = payment.Status,
                PaidAt = payment.PaidAt
            };

            return ApiResponse<VerifyPaymentResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<VerifyPaymentResponse>.FailureResult("Error verifying payment", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> MarkPaymentAsPaidAsync(Guid paymentId)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return ApiResponse<bool>.FailureResult("Payment not found", new List<string> { "NOT_FOUND" });
            }

            if (payment.Status == PaymentStatus.Paid)
            {
                return ApiResponse<bool>.FailureResult("Payment already marked as paid", new List<string> { "ALREADY_PAID" });
            }

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;

            // Update order payment status
            var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Confirmed;
                _unitOfWork.Orders.Update(order);
            }

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult("Error marking payment as paid", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> CancelPaymentAsync(Guid paymentId)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return ApiResponse<bool>.FailureResult("Payment not found", new List<string> { "NOT_FOUND" });
            }

            if (payment.Status == PaymentStatus.Paid)
            {
                return ApiResponse<bool>.FailureResult("Cannot cancel paid payment", new List<string> { "PAYMENT_PAID" });
            }

            payment.Status = PaymentStatus.Failed;
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult("Error canceling payment", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<BankTransferConfigDto>> CreateBankConfigAsync(CreateBankTransferConfigDto dto)
    {
        try
        {
            var existingConfig = await _unitOfWork.BankTransferConfigs
                .FirstOrDefaultAsync(b => b.BankCode == dto.BankCode || b.AccountNumber == dto.AccountNumber);

            if (existingConfig != null)
            {
                return ApiResponse<BankTransferConfigDto>.FailureResult("Bank configuration already exists", new List<string> { "CONFIG_EXISTS" });
            }

            var config = new BankTransferConfig
            {
                BankCode = dto.BankCode,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                AccountName = dto.AccountName,
                BankBranch = dto.BankBranch,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                LogoUrl = dto.LogoUrl
            };

            await _unitOfWork.BankTransferConfigs.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<BankTransferConfigDto>.SuccessResult(MapBankConfigToDto(config));
        }
        catch (Exception ex)
        {
            return ApiResponse<BankTransferConfigDto>.FailureResult("Error creating bank config", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<List<BankTransferConfigDto>>> GetActiveBankConfigsAsync()
    {
        try
        {
            var configs = await _unitOfWork.BankTransferConfigs
                .FindAsync(b => b.IsActive);

            var configDtos = configs
                .OrderBy(b => b.DisplayOrder)
                .Select(MapBankConfigToDto)
                .ToList();

            return ApiResponse<List<BankTransferConfigDto>>.SuccessResult(configDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<BankTransferConfigDto>>.FailureResult("Error getting bank configs", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<BankTransferConfigDto>> GetBankConfigByCodeAsync(string bankCode)
    {
        try
        {
            var config = await _unitOfWork.BankTransferConfigs
                .FirstOrDefaultAsync(b => b.BankCode == bankCode && b.IsActive);

            if (config == null)
            {
                return ApiResponse<BankTransferConfigDto>.FailureResult("Bank config not found", new List<string> { "NOT_FOUND" });
            }

            return ApiResponse<BankTransferConfigDto>.SuccessResult(MapBankConfigToDto(config));
        }
        catch (Exception ex)
        {
            return ApiResponse<BankTransferConfigDto>.FailureResult("Error getting bank config", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteBankConfigAsync(Guid id)
    {
        try
        {
            var config = await _unitOfWork.BankTransferConfigs.GetByIdAsync(id);
            if (config == null)
            {
                return ApiResponse<bool>.FailureResult("Bank config not found", new List<string> { "NOT_FOUND" });
            }

            _unitOfWork.BankTransferConfigs.SoftDelete(config);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult("Error deleting bank config", new List<string> { ex.Message });
        }
    }

    private async Task<string> GetDefaultBankCodeAsync()
    {
        var defaultBank = await _unitOfWork.BankTransferConfigs
            .FirstOrDefaultAsync(b => b.IsActive);

        return defaultBank?.BankCode ?? "970422"; // Default to MB Bank
    }

    private PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            QRCodeData = payment.QRCodeData,
            QRCodeImageUrl = payment.QRCodeImageUrl,
            BankCode = payment.BankCode,
            BankAccountNumber = payment.BankAccountNumber,
            BankAccountName = payment.BankAccountName,
            PaymentDescription = payment.PaymentDescription,
            PaidAt = payment.PaidAt,
            ExpiresAt = payment.ExpiresAt,
            CreatedAt = payment.CreatedAt
        };
    }

    private BankTransferConfigDto MapBankConfigToDto(BankTransferConfig config)
    {
        return new BankTransferConfigDto
        {
            Id = config.Id,
            BankCode = config.BankCode,
            BankName = config.BankName,
            AccountNumber = config.AccountNumber,
            AccountName = config.AccountName,
            BankBranch = config.BankBranch,
            IsActive = config.IsActive,
            DisplayOrder = config.DisplayOrder,
            LogoUrl = config.LogoUrl
        };
    }
}
