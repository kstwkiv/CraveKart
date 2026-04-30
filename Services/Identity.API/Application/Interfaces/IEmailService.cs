namespace Identity.API.Application.Interfaces;

/// <summary>
/// Service interface for sending transactional emails.
/// </summary>
/// 
///  any class implementing this interface should send mails 
public interface IEmailService
{
    /// <summary>Sends an HTML email to the specified recipient.</summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The HTML body content of the email.</param>
    Task SendAsync(string to, string subject, string body);
}
