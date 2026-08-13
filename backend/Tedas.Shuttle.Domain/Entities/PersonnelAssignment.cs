namespace Tedas.Shuttle.Domain.Entities;

public sealed class PersonnelAssignment
{
    public Guid Id { get; private set; }

    public Guid PersonnelId { get; private set; }

    public Guid ShuttleShiftId { get; private set; }

    public Guid? BoardingRoutePointId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset? DeactivatedAt { get; private set; }

    public Personnel? Personnel { get; private set; }

    public ShuttleShift? ShuttleShift { get; private set; }

    private PersonnelAssignment()
    {
    }

    public PersonnelAssignment(
        Guid personnelId,
        Guid shuttleShiftId,
        Guid? boardingRoutePointId,
        DateTimeOffset assignedAt)
    {
        Id = Guid.NewGuid();
        PersonnelId = personnelId;
        ShuttleShiftId = shuttleShiftId;
        BoardingRoutePointId = boardingRoutePointId;
        AssignedAt = assignedAt;
        IsActive = true;
    }

    public void Deactivate(DateTimeOffset deactivatedAt)
    {
        IsActive = false;
        DeactivatedAt = deactivatedAt;
    }
}

