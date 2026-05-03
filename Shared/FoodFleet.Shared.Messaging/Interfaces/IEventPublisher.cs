namespace FoodFleet.Shared.Messaging.Interfaces;

/// <summary>
/// Abstraction for publishing domain events to a message broker.
/// Implemented by <see cref="EventPublisher"/> using MassTransit.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a message of type <typeparamref name="T"/> to the message broker.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish. Must be a reference type.</typeparam>
    /// <param name="message">The message instance to publish.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
}