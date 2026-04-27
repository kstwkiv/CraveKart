namespace Delivery.API.Application.Commands;

public record UpdateLocationCommand(
    Guid AgentId,
    double Lat,
    double Lng);
