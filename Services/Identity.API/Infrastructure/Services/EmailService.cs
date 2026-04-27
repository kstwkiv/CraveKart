using System.Net;
using System.Net.Mail;
using Identity.API.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Identity.API.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var smtp = _configuration.GetSection("Smtp");
        var host = smtp["Host"]!;
        var port = int.Parse(smtp["Port"]!);
        var from = smtp["From"]!;
        var username = smtp["Username"]!;
        var password = smtp["Password"]!;
        var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = enableSsl
        };

        var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };
        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To} | Subject: {Subject}", to, subject);
    }
}
