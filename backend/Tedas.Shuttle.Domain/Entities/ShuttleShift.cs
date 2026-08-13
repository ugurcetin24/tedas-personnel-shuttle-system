using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Domain.Entities;

public sealed class ShuttleShift
{
    public Guid Id { get; private set; }

    public Guid PhysicalShuttleId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ShiftType ShiftType { get; private set; }

    public int Capacity { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public PhysicalShuttle? PhysicalShuttle { get; private set; }

    private ShuttleShift()
    {
    }

    public ShuttleShift(
        Guid physicalShuttleId,
        string name,
        ShiftType shiftType,
        int capacity,
        TimeOnly startTime,
        TimeOnly endTime,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        PhysicalShuttleId = physicalShuttleId;
        Name = name;
        ShiftType = shiftType;
        Capacity = capacity;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void Update(
        string name,
        ShiftType shiftType,
        int capacity,
        TimeOnly startTime,
        TimeOnly endTime,
        DateTimeOffset updatedAt)
    {
        Name = name;
        ShiftType = shiftType;
        Capacity = capacity;
        StartTime = startTime;
        EndTime = endTime;
        UpdatedAt = updatedAt;
    }

    public void SetActiveStatus(bool isActive, DateTimeOffset updatedAt)
    {
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }
}
