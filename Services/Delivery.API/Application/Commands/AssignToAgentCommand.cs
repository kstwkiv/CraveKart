namespace Delivery.API.Application.Commands;

/// <summary>
/// Command for a delivery agent to self-assign a ready order without admin intervention.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to be picked up.</param>
/// <param name="AgentId">The unique identifier of the delivery agent claiming the order.</param>
public record AssignToAgentCommand(Guid OrderId, Guid AgentId);
