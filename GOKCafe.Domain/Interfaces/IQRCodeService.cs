namespace GOKCafe.Domain.Interfaces;

public interface IQRCodeService
{
    /// <summary>
    /// Generates VietQR-compliant QR code data for bank transfer
    /// </summary>
    /// <param name="bankCode">Bank code (e.g., "970422" for MB Bank)</param>
    /// <param name="accountNumber">Bank account number</param>
    /// <param name="accountName">Account holder name</param>
    /// <param name="amount">Transfer amount</param>
    /// <param name="description">Payment description/reference</param>
    /// <returns>QR code data string</returns>
    string GenerateVietQRData(string bankCode, string accountNumber, string accountName, decimal amount, string description);

    /// <summary>
    /// Generates QR code image as base64 string
    /// </summary>
    /// <param name="qrData">QR code data string</param>
    /// <param name="pixelsPerModule">Size of QR code (default: 20)</param>
    /// <returns>Base64 encoded PNG image</returns>
    string GenerateQRCodeImage(string qrData, int pixelsPerModule = 20);

    /// <summary>
    /// Generates QR code image URL using external API (VietQR API)
    /// </summary>
    /// <param name="bankCode">Bank code</param>
    /// <param name="accountNumber">Account number</param>
    /// <param name="accountName">Account holder name</param>
    /// <param name="amount">Transfer amount</param>
    /// <param name="description">Payment description</param>
    /// <returns>URL to QR code image</returns>
    string GenerateVietQRImageUrl(string bankCode, string accountNumber, string accountName, decimal amount, string description);
}
