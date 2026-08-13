using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.DTOs.Shifts;

public sealed record UpdateShiftRequest(
    string Name,
    ShiftType ShiftType,
    int Capacity,
    TimeOnly StartTime,
    TimeOnly EndTime);
