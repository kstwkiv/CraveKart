using FoodFleet.Shared.Events.Restaurants;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class RestaurantRejectedConsumer : IConsumer<RestaurantRejectedEvent>
{
    private readonly IEmailService _emailService;

    public RestaurantRejectedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<RestaurantRejectedEvent> context)
    {
        var msg = context.Message;
        var isSuspended = msg.Reason.StartsWith("Suspended:");
        var reason = msg.Reason.Replace("Suspended: ", "");

        var subject = isSuspended
            ? $"Your Restaurant Has Been Suspended — {msg.RestaurantName}"
            : $"Your Restaurant Application Was Not Approved — {msg.RestaurantName}";

        await _emailService.SendAsync(
            msg.OwnerEmail,
            subject,
            EmailTemplates.RestaurantRejected(msg.RestaurantName, reason, isSuspended));
    }
}
