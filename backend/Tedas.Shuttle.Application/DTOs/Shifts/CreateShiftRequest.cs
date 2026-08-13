using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.DTOs.Shifts;

public sealed record CreateShiftRequest(
    string Name,
    ShiftType ShiftType,
    int Capacity,
    TimeOnly StartTime,
    TimeOnly EndTime);
