namespace GOKCafe.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send password reset email
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="resetUrl">Full URL for password reset</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, string resetUrl);

    /// <summary>
    /// Send welcome email to new users
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string toName);

    /// <summary>
    /// Send generic email
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML email body</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
}
