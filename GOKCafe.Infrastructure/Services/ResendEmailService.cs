using GOKCafe.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace GOKCafe.Infrastructure.Services;

/// <summary>
/// Email service implementation using Resend API
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public ResendEmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _apiKey = configuration["Email:ResendApiKey"] ?? throw new InvalidOperationException("Resend API key not configured");
        _fromEmail = configuration["Email:FromEmail"] ?? "onboarding@resend.dev";
        _fromName = configuration["Email:FromName"] ?? "GOK Cafe";

        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, string resetUrl)
    {
        var subject = "Reset Your Password - GOK Cafe";
        var htmlBody = GetPasswordResetEmailTemplate(toName, resetUrl);

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
    {
        var subject = "Welcome to GOK Cafe!";
        var htmlBody = GetWelcomeEmailTemplate(toName);

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var emailRequest = new
            {
                from = $"{_fromName} <{_fromEmail}>",
                to = new[] { toEmail },
                subject = subject,
                html = htmlBody
            };

            _logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);

            var response = await _httpClient.PostAsJsonAsync("emails", emailRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}, Response: {Response}",
                    toEmail, response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
            return false;
        }
    }

    private string GetPasswordResetEmailTemplate(string toName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 30px; background-color: #8B4513; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px;"">GOK Cafe</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #333333; font-size: 24px;"">Reset Your Password</h2>
                            <p style=""margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                Hi {toName},
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                We received a request to reset your password for your GOK Cafe account. Click the button below to create a new password:
                            </p>
                            <table role=""presentation"" style=""margin: 30px 0;"">
                                <tr>
                                    <td style=""border-radius: 4px; background-color: #8B4513;"">
                                        <a href=""{resetUrl}"" target=""_blank"" style=""display: inline-block; padding: 16px 36px; color: #ffffff; text-decoration: none; border-radius: 4px; font-size: 16px; font-weight: bold;"">
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin: 0 0 10px 0; color: #666666; font-size: 14px; line-height: 1.5;"">
                                Or copy and paste this URL into your browser:
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #8B4513; font-size: 14px; word-break: break-all;"">
                                {resetUrl}
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #666666; font-size: 14px; line-height: 1.5;"">
                                This link will expire in <strong>1 hour</strong> for security reasons.
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 14px; line-height: 1.5;"">
                                If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px; background-color: #f8f8f8; text-align: center;"">
                            <p style=""margin: 0 0 10px 0; color: #999999; font-size: 12px;"">
                                This email was sent by GOK Cafe
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 12px;"">
                                &copy; 2025 GOK Cafe. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string toName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to GOK Cafe</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 30px; background-color: #8B4513; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px;"">Welcome to GOK Cafe!</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #333333; font-size: 24px;"">Thanks for joining us, {toName}!</h2>
                            <p style=""margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                We're excited to have you as part of the GOK Cafe family. Your account has been successfully created.
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                Start exploring our delicious menu and enjoy exclusive member benefits!
                            </p>
                            <p style=""margin: 0; color: #666666; font-size: 16px; line-height: 1.5;"">
                                If you have any questions, feel free to reach out to our support team.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px; background-color: #f8f8f8; text-align: center;"">
                            <p style=""margin: 0 0 10px 0; color: #999999; font-size: 12px;"">
                                This email was sent by GOK Cafe
                            </p>
                            <p style=""margin: 0; color: #999999; font-size: 12px;"">
                                &copy; 2025 GOK Cafe. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
