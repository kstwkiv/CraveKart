namespace FoodFleet.Shared.Messaging.Configuration;

/// <summary>
/// Configuration settings for connecting to a RabbitMQ message broker.
/// Bound from the "RabbitMq" section of application settings.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>Gets or sets the RabbitMQ host address. Defaults to "localhost".</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>Gets or sets the RabbitMQ username. Defaults to "guest".</summary>
    public string Username { get; set; } = "guest";

    /// <summary>Gets or sets the RabbitMQ password. Defaults to "guest".</summary>
    public string Password { get; set; } = "guest";

    /// <summary>Gets or sets the RabbitMQ AMQP port. Defaults to 5672.</summary>
    public ushort Port { get; set; } = 5672;
}