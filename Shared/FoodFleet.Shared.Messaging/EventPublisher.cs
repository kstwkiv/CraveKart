// 'using' — imports the Interfaces namespace so IEventPublisher is available without full qualification
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports MassTransit so IPublishEndpoint (message-bus abstraction) is in scope
using MassTransit;

// 'namespace' — logical grouping that prevents name collisions across assemblies
namespace FoodFleet.Shared.Messaging;

/// <summary>
/// MassTransit-based implementation of <see cref="IEventPublisher"/>.
/// Publishes domain events to the configured RabbitMQ exchange via MassTransit's publish endpoint.
/// </summary>
// 'public' — access modifier: visible to any assembly that references this project
// 'class' — defines a reference type; instances live on the managed heap
// ':' — implements the IEventPublisher interface (contract-based programming)
public class EventPublisher : IEventPublisher
{
    // 'private' — field is encapsulated; only this class can read or write it
    // 'readonly' — value is set once in the constructor and never changed (immutability guarantee)
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of <see cref="EventPublisher"/>.
    /// </summary>
    /// <param name="publishEndpoint">The MassTransit publish endpoint for sending messages.</param>
    // 'public' — constructor is accessible to the DI container for dependency injection
    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        // Stores the injected message-bus endpoint for use in PublishAsync
        _publishEndpoint = publishEndpoint;
    }

    /// <inheritdoc/>
    // 'public' — part of the public interface contract
    // 'async' — compiler transforms this method into an awaitable state machine
    // 'Task' — represents an asynchronous operation with no return value (void equivalent for async)
    // '<T>' — generic type parameter: makes the method work with any message type at compile time
    // 'where T : class' — generic constraint: T must be a reference type (required by MassTransit)
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        // 'await' — suspends the method until the publish operation completes, without blocking the thread
        await _publishEndpoint.Publish(message, cancellationToken);
    }
}
