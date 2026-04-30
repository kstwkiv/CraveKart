using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;

namespace Delivery.API.Application.Interfaces;

/// <summary>
/// Service interface defining the core delivery lifecycle operations.
/// </summary>
public interface IDeliveryService
{
    /// <summary>Automatically assigns an available delivery agent to an order.</summary>
    /// <param name="request">The command containing order and customer details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="DeliveryDto"/> representing the created delivery assignment.</returns>
    Task<DeliveryDto> AssignAsync(AssignDeliveryCommand request, CancellationToken cancellationToken = default);

    /// <summary>Assigns a specific agent to an order (agent self-pickup flow).</summary>
    /// <param name="request">The command containing the order and agent identifiers.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="DeliveryDto"/> representing the created delivery assignment.</returns>
    Task<DeliveryDto> AssignToAgentAsync(AssignToAgentCommand request, CancellationToken cancellationToken = default);

    /// <summary>Updates the real-time GPS location of an agent's active delivery.</summary>
    /// <param name="request">The command containing the agent ID and new coordinates.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the location was updated; <c>false</c> if no active delivery was found.</returns>
    Task<bool> UpdateLocationAsync(UpdateLocationCommand request, CancellationToken cancellationToken = default);

    /// <summary>Marks a delivery as completed and updates agent statistics.</summary>
    /// <param name="request">The command containing the order ID to complete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the delivery was completed; <c>false</c> if not found.</returns>
    Task<bool> CompleteAsync(CompleteDeliveryCommand request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the delivery record associated with a specific order.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="DeliveryDto"/> if found; otherwise <c>null</c>.</returns>
    Task<DeliveryDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
