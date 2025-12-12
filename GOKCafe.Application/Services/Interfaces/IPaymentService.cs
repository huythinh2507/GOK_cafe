using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Payment;

namespace GOKCafe.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id);
    Task<ApiResponse<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId);
    Task<ApiResponse<VerifyPaymentResponse>> VerifyPaymentAsync(VerifyPaymentRequest request);
    Task<ApiResponse<bool>> MarkPaymentAsPaidAsync(Guid paymentId);
    Task<ApiResponse<bool>> CancelPaymentAsync(Guid paymentId);

    // Bank Transfer Config
    Task<ApiResponse<BankTransferConfigDto>> CreateBankConfigAsync(CreateBankTransferConfigDto dto);
    Task<ApiResponse<List<BankTransferConfigDto>>> GetActiveBankConfigsAsync();
    Task<ApiResponse<BankTransferConfigDto>> GetBankConfigByCodeAsync(string bankCode);
    Task<ApiResponse<bool>> DeleteBankConfigAsync(Guid id);
}
