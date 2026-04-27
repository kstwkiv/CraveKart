namespace Delivery.API.Application.Commands;

public record AssignToAgentCommand(Guid OrderId, Guid AgentId);
