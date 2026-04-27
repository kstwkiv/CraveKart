using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class UserLoggedInConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly IEmailService _emailService;

    public UserLoggedInConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.Email,
            "New Sign-In to Your CraveKart Account 🔐",
            EmailTemplates.LoginAlert(msg.FullName, msg.LoggedInAt.ToString("f")));
    }
}
