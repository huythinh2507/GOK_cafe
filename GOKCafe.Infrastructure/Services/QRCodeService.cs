using GOKCafe.Domain.Interfaces;
using System.Text;
using System.Web;

namespace GOKCafe.Infrastructure.Services;

public class QRCodeService : IQRCodeService
{
    public string GenerateVietQRData(string bankCode, string accountNumber, string accountName, decimal amount, string description)
    {
        // VietQR uses EMVCo standard format
        // Format: 00020101021238{length}{content}...6304{checksum}

        var builder = new StringBuilder();

        // Payload Format Indicator
        builder.Append("000201");

        // Point of Initiation Method (12 = QR only used once with dynamic data)
        builder.Append("010212");

        // Merchant Account Information
        var beneficiary = $"0010A000000727{FormatSubField("01", bankCode)}{FormatSubField("02", accountNumber)}";
        builder.Append($"38{beneficiary.Length:D2}{beneficiary}");

        // Transaction Currency (704 = VND)
        builder.Append("5303704");

        // Transaction Amount (if specified)
        if (amount > 0)
        {
            var amountStr = amount.ToString("F0");
            builder.Append($"54{amountStr.Length:D2}{amountStr}");
        }

        // Country Code (VN = Vietnam)
        builder.Append("5802VN");

        // Merchant Name
        var merchantName = RemoveVietnameseTones(accountName);
        if (merchantName.Length > 25)
            merchantName = merchantName.Substring(0, 25);
        builder.Append($"59{merchantName.Length:D2}{merchantName}");

        // Additional Data Field
        if (!string.IsNullOrEmpty(description))
        {
            var desc = RemoveVietnameseTones(description);
            if (desc.Length > 25)
                desc = desc.Substring(0, 25);
            var additionalData = FormatSubField("08", desc);
            builder.Append($"62{additionalData.Length:D2}{additionalData}");
        }

        // CRC (will be calculated)
        builder.Append("6304");

        // Calculate CRC16-CCITT
        var crc = CalculateCRC16(builder.ToString());
        builder.Append(crc);

        return builder.ToString();
    }

    public string GenerateQRCodeImage(string qrData, int pixelsPerModule = 20)
    {
        // Note: This requires QRCoder NuGet package
        // For now, return a placeholder - will implement after package installation
        // This method will be implemented using QRCoder library
        return $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUg..."; // Placeholder
    }

    public string GenerateVietQRImageUrl(string bankCode, string accountNumber, string accountName, decimal amount, string description)
    {
        // Use VietQR API to generate QR code image
        // API: https://img.vietqr.io/image/{bankCode}-{accountNumber}-{template}.png?amount={amount}&addInfo={description}&accountName={accountName}

        var template = "compact2"; // or "compact", "print", "qr_only"
        var encodedAccountName = HttpUtility.UrlEncode(accountName);
        var encodedDescription = HttpUtility.UrlEncode(description);

        var url = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-{template}.png";

        var queryParams = new List<string>();
        if (amount > 0)
            queryParams.Add($"amount={amount:F0}");
        if (!string.IsNullOrEmpty(description))
            queryParams.Add($"addInfo={encodedDescription}");
        if (!string.IsNullOrEmpty(accountName))
            queryParams.Add($"accountName={encodedAccountName}");

        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        return url;
    }

    private string FormatSubField(string id, string value)
    {
        return $"{id}{value.Length:D2}{value}";
    }

    private string CalculateCRC16(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        ushort crc = 0xFFFF;

        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }

        return (crc & 0xFFFF).ToString("X4");
    }

    private string RemoveVietnameseTones(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = text.ToUpper();

        // Remove Vietnamese diacritics
        result = result.Replace("À", "A").Replace("Á", "A").Replace("Ạ", "A").Replace("Ả", "A").Replace("Ã", "A")
                      .Replace("Â", "A").Replace("Ầ", "A").Replace("Ấ", "A").Replace("Ậ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A")
                      .Replace("Ă", "A").Replace("Ằ", "A").Replace("Ắ", "A").Replace("Ặ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A")
                      .Replace("È", "E").Replace("É", "E").Replace("Ẹ", "E").Replace("Ẻ", "E").Replace("Ẽ", "E")
                      .Replace("Ê", "E").Replace("Ề", "E").Replace("Ế", "E").Replace("Ệ", "E").Replace("Ể", "E").Replace("Ễ", "E")
                      .Replace("Ì", "I").Replace("Í", "I").Replace("Ị", "I").Replace("Ỉ", "I").Replace("Ĩ", "I")
                      .Replace("Ò", "O").Replace("Ó", "O").Replace("Ọ", "O").Replace("Ỏ", "O").Replace("Õ", "O")
                      .Replace("Ô", "O").Replace("Ồ", "O").Replace("Ố", "O").Replace("Ộ", "O").Replace("Ổ", "O").Replace("Ỗ", "O")
                      .Replace("Ơ", "O").Replace("Ờ", "O").Replace("Ớ", "O").Replace("Ợ", "O").Replace("Ở", "O").Replace("Ỡ", "O")
                      .Replace("Ù", "U").Replace("Ú", "U").Replace("Ụ", "U").Replace("Ủ", "U").Replace("Ũ", "U")
                      .Replace("Ư", "U").Replace("Ừ", "U").Replace("Ứ", "U").Replace("Ự", "U").Replace("Ử", "U").Replace("Ữ", "U")
                      .Replace("Ỳ", "Y").Replace("Ý", "Y").Replace("Ỵ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y")
                      .Replace("Đ", "D");

        return result;
    }
}
