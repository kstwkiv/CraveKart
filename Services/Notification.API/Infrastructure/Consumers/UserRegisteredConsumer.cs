using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    public UserRegisteredConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        await _emailService.SendAsync(
            context.Message.Email,
            "Welcome to CraveKart! 🎉",
            EmailTemplates.Welcome(context.Message.FullName));
    }
}
