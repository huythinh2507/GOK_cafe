using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Payment;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages payment operations including QR code generation for bank transfers
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Create a payment for an order (generates QR code for bank transfer)
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <returns>Payment details with QR code information</returns>
    [HttpPost]
    [ProducesResponseType<ApiResponse<CreatePaymentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _paymentService.CreatePaymentAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get payment details by payment ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<PaymentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get payment details by order ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType<ApiResponse<PaymentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
    {
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Verify payment status
    /// </summary>
    /// <param name="request">Verification request</param>
    /// <returns>Payment verification result</returns>
    [HttpPost("verify")]
    [ProducesResponseType<ApiResponse<VerifyPaymentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _paymentService.VerifyPaymentAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Mark payment as paid (admin/system use)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/mark-paid")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaymentAsPaid(Guid id)
    {
        var result = await _paymentService.MarkPaymentAsPaidAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Cancel payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(Guid id)
    {
        var result = await _paymentService.CancelPaymentAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create bank transfer configuration (admin only)
    /// </summary>
    /// <param name="dto">Bank config data</param>
    /// <returns>Created bank configuration</returns>
    [HttpPost("bank-configs")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<BankTransferConfigDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBankConfig([FromBody] CreateBankTransferConfigDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _paymentService.CreateBankConfigAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all active bank transfer configurations
    /// </summary>
    /// <returns>List of active bank configurations</returns>
    [HttpGet("bank-configs")]
    [ProducesResponseType<ApiResponse<List<BankTransferConfigDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveBankConfigs()
    {
        var result = await _paymentService.GetActiveBankConfigsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get bank configuration by bank code
    /// </summary>
    /// <param name="bankCode">Bank code (e.g., "970422" for MB Bank)</param>
    /// <returns>Bank configuration details</returns>
    [HttpGet("bank-configs/{bankCode}")]
    [ProducesResponseType<ApiResponse<BankTransferConfigDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBankConfigByCode(string bankCode)
    {
        var result = await _paymentService.GetBankConfigByCodeAsync(bankCode);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete bank configuration (admin only)
    /// </summary>
    /// <param name="id">Bank config ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("bank-configs/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBankConfig(Guid id)
    {
        var result = await _paymentService.DeleteBankConfigAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
