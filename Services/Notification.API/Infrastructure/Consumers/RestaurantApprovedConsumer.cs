using FoodFleet.Shared.Events.Restaurants;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class RestaurantApprovedConsumer : IConsumer<RestaurantApprovedEvent>
{
    private readonly IEmailService _emailService;

    public RestaurantApprovedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<RestaurantApprovedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.OwnerEmail,
            $"🎉 Your Restaurant is Live on CraveKart!",
            EmailTemplates.RestaurantApproved(msg.RestaurantName, msg.ApprovedAt.ToString("f")));
    }
}
