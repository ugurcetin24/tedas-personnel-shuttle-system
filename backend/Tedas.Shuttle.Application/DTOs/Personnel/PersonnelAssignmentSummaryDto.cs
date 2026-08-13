namespace Tedas.Shuttle.Application.DTOs.Personnel;

public sealed record PersonnelAssignmentSummaryDto(
    Guid AssignmentId,
    string ShuttleCode,
    string ShiftName,
    string? BoardingPointName);
