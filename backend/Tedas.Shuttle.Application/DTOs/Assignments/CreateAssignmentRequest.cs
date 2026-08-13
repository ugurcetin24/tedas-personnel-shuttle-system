namespace Tedas.Shuttle.Application.DTOs.Assignments;

public sealed record CreateAssignmentRequest(
    Guid PersonnelId,
    Guid ShuttleShiftId,
    Guid? BoardingRoutePointId);

